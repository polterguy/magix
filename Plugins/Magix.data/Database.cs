/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Threading;
using System.Configuration;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.data
{
	/*
	 * data storage
	 */
	internal static class Database
	{
        private static object _locker = new object();
        private static object _transactionalLocker = new object();
        private static string _dbPath;
        private static string _appPath;
        private static Node _database;
        private static Tuple<Guid, Node> _transaction = new Tuple<Guid, Node>(Guid.Empty, new Node());

        #region [ -- publicly available methods -- ]

        internal static void Initialize()
        {
            lock (_locker)
            {
                lock (_transactionalLocker)
                {
                    if (_appPath != null)
                        return; // multiple initializations might occur

                    _appPath = HttpContext.Current.Server.MapPath("/");
                    _appPath = _appPath.Replace("\\", "/");
                    _dbPath = ConfigurationManager.AppSettings["magix.core.database-path"];
                    _database = new Node();

                    Node listDirectoriesNode = new Node();
                    listDirectoriesNode["directory"].Value = _dbPath;
                    ActiveEvents.Instance.RaiseActiveEvent(
                        typeof(Database),
                        "magix.file.list-directories",
                        listDirectoriesNode);

                    foreach (Node idxDir in listDirectoriesNode["directories"])
                    {
                        // retrieving data files
                        Node listFilesNode = new Node();
                        listFilesNode["filter"].Value = "db*.hl";
                        listFilesNode["directory"].Value = idxDir.Name;
                        ActiveEvents.Instance.RaiseActiveEvent(
                            typeof(Database),
                            "magix.file.list-files",
                            listFilesNode);

                        foreach (Node idxFileNode in listFilesNode["files"])
                        {
                            Node loadFile = new Node();
                            loadFile["file"].Value = idxFileNode.Name;
                            ActiveEvents.Instance.RaiseActiveEvent(
                                typeof(Database),
                                "magix.execute.code-2-node",
                                loadFile);

                            _database[idxFileNode.Name].AddRange(loadFile["node"]);
                        }
                    }
                }
            }
        }

        /*
         * creates a database transaction
         */
        internal static void ExecuteTransaction(Node pars)
        {
            if (pars.Contains("_database-transaction"))
                throw new ApplicationException("there is already an open transaction towards the database");

            lock (_transactionalLocker)
            {
                _transaction = new Tuple<Guid, Node>(Guid.NewGuid(), _database.Clone());
                pars["_database-transaction"].Value = _transaction.Item1;
                try
                {
                    ActiveEvents.Instance.RaiseActiveEvent(
                        typeof(Database),
                        "magix.execute",
                        pars);
                }
                finally
                {
                    pars["_database-transaction"].UnTie();
                    Rollback(_transaction.Item1);
                }
            }
        }

        /*
         * rolls back a transaction
         */
        internal static void Rollback(Guid transaction)
        {
            if (_transaction.Item1 != transaction)
                return; // already committed ...
            _transaction = new Tuple<Guid, Node>(Guid.Empty, new Node());
        }

        /*
         * commits the active transaction
         */
        internal static void Commit(Node pars)
        {
            lock (_locker)
            {
                Guid transaction = pars["_database-transaction"].UnTie().Get<Guid>();
                if (_transaction.Item1 != transaction)
                    throw new ApplicationException("transaction id mismatch in commit");

                _database = _transaction.Item2;
                _transaction = new Tuple<Guid, Node>(Guid.Empty, new Node());
                List<Node> toBeRemoved = new List<Node>();

                // removing items that should be removed first
                foreach (Node idx in _database)
                {
                    if (idx.Count == 0 && idx.Name != "added:")
                    {
                        toBeRemoved.Add(idx);
                    }
                }
                foreach (Node idx in toBeRemoved)
                {
                    RemoveNodeFromDatabase(idx);
                }

                // updating changed items
                foreach (Node idx in _database)
                {
                    if (idx.Name.StartsWith("changed:") && idx.Count > 0)
                    {
                        idx.Name = idx.Name.Substring(8);
                        SaveFileNodeToDisc(idx);
                    }
                }

                // adding new items
                List<Node> toBeRemovedFromDatabase = new List<Node>();
                int objectsPerFile = int.Parse(ConfigurationManager.AppSettings["magix.core.database-objects-per-file"]);
                foreach (Node idx in _database)
                {
                    if (idx.Name == "added:")
                    {
                        Node availableNode = FindAvailableNode();
                        while (idx.Count > 0)
                        {
                            while (availableNode.Count < objectsPerFile && idx.Count > 0)
                            {
                                availableNode.Add(idx[0].UnTie());
                            }
                            SaveFileNodeToDisc(availableNode);
                            availableNode = FindAvailableNode();
                        }
                        toBeRemovedFromDatabase.Add(idx);
                        break;
                    }
                }
                foreach (Node idx in toBeRemovedFromDatabase)
                {
                    _database.Remove(idx);
                }

            }
        }

        /*
         * loads items from database
         */
        internal static void LoadItems(Node ip, Node prototype, string id, int start, int end, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    LoadItems(ip, prototype, id, start, end, transaction);
                }
            }
            lock (_locker)
            {
                bool onlyId = ip.ContainsValue("only-id") && ip["only-id"].Get<bool>();
                int curMatchingItem = 0;
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (id != null && id == idxObjectNode.Get<string>())
                        {
                            // loading by id
                            Node curNode = idxObjectNode.Clone();
                            ip["value"].AddRange(curNode);
                            return;
                        }
                        else if (id == null)
                        {
                            // loading by prototype
                            if (idxObjectNode.HasNodes(prototype))
                            {
                                if ((start == 0 || curMatchingItem >= start) && (end == -1 || curMatchingItem < end))
                                {
                                    Node objectNode = new Node("object");
                                    objectNode["id"].Value = idxObjectNode.Get<string>();
                                    if (!onlyId)
                                        objectNode["value"].AddRange(idxObjectNode.Clone());
                                    ip["objects"].Add(objectNode);
                                }
                                curMatchingItem += 1;
                            }
                        }
                    }
                }
            }
        }

        /*
         * counts records in database
         */
        internal static void CountRecords(Node ip, Node prototype, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    CountRecords(ip, prototype, transaction);
                }
            }
            lock (_locker)
            {
                int count = 0;
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (prototype == null)
                            count += 1;
                        else
                        {
                            if (idxObjectNode.HasNodes(prototype))
                                count += 1;
                        }
                    }
                }
                ip["count"].Value = count;
            }
        }

        /*
         * saves an object by its id
         */
        internal static void SaveById(Node value, string id, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    SaveById(value, id, transaction);
                }
            }
            lock (_locker)
            {
                Node fileNode = null;
                value.Value = id;
                value.Name = "id";
                foreach (Node idxFileNode in GetDatabase())
                {
                    bool found = false;
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (idxObjectNode.Get<string>() == id)
                        {
                            idxObjectNode.Clear();
                            idxObjectNode.AddRange(value);
                            fileNode = idxFileNode;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                if (fileNode != null)
                {
                    SaveFileNodeToDisc(fileNode);
                }
                else
                    SaveNewObject(value, id);
            }
        }

        /*
         * saves a new object
         */
        internal static string SaveNewObject(Node value, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    SaveNewObject(value, transaction);
                }
            }
            lock (_locker)
            {
                return SaveNewObject(value, null);
            }
        }

        /*
         * removes items from database according to prototype
         */
        internal static void RemoveByPrototype(Node prototype, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    RemoveByPrototype(prototype, transaction);
                }
            }
            lock (_locker)
            {
                List<Node> nodesToRemove = new List<Node>();
                List<string> filesToUpdate = new List<string>();
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (idxObjectNode.HasNodes(prototype))
                        {
                            nodesToRemove.Add(idxObjectNode);
                            if (!filesToUpdate.Exists(
                                delegate(string idxFileName)
                                {
                                    return idxFileName == idxFileNode.Name;
                                }))
                                filesToUpdate.Add(idxFileNode.Name);
                        }
                    }
                }
                foreach (Node idx in nodesToRemove)
                {
                    idx.UnTie();
                }
                foreach (string idx in filesToUpdate)
                {
                    if (GetDatabase()[idx].Count == 0)
                        RemoveNodeFromDatabase(GetDatabase()[idx]);
                    else
                        SaveFileNodeToDisc(GetDatabase()[idx]);
                }
            }
        }

        /*
         * removes a node by its id
         */
        internal static void RemoveById(string id, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    RemoveById(id, transaction);
                }
            }
            lock (_locker)
            {
                Node objectToRemove = null;
                foreach (Node idxFileNode in GetDatabase())
                {
                    bool found = false;
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (idxObjectNode.Get<string>() == id)
                        {
                            objectToRemove = idxObjectNode;
                            found = true;
                            break;
                        }
                    }
                    if (found)
                        break;
                }
                if (objectToRemove != null)
                {
                    Node fileObject = objectToRemove.Parent;
                    objectToRemove.UnTie();

                    if (fileObject.Count > 0)
                        SaveFileNodeToDisc(fileObject);
                    else
                        RemoveNodeFromDatabase(fileObject);
                }
            }
        }

        #endregion

        #region [ -- private methods -- ]

        /*
         * returns database reference
         */
        private static Node GetDatabase()
        {
            if (_transaction.Item1 != Guid.Empty)
                return _transaction.Item2; // we have an open transaction

            // no open transaction
            return _database;
        }

        /*
         * saves a new object
         */
        private static string SaveNewObject(Node value, string id)
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString().Replace("-", "");
            value.Name = "id";
            value.Value = id;

            Node availableFileNode = FindAvailableNode();
            availableFileNode.Add(value);
            SaveFileNodeToDisc(availableFileNode);

            return id;
        }

        /*
         * saves a file node to disc
         */
        private static void SaveFileNodeToDisc(Node fileNode)
        {
            if (_transaction.Item1 != Guid.Empty)
            {
                // not saving to disc if we have an open transaction
                if (fileNode.Name.StartsWith("changed:") || fileNode.Name == "added:")
                    return;
                fileNode.Name = "changed:" + fileNode.Name;
                return;
            }

            // updating existing object
            Node codeNode = new Node();
            codeNode["node"].Value = fileNode;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.execute.node-2-code",
                codeNode);

            Node fileSaveNode = new Node();
            fileSaveNode["file"].Value = fileNode.Name;
            fileSaveNode["value"].Value = codeNode["code"].Get<string>();
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.save",
                fileSaveNode);
        }

        /*
         * returns next available filename
         */
        private static string FindAvailableNewFileName()
        {
            Node listDirectoriesNode = new Node();
            listDirectoriesNode["directory"].Value = _dbPath;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.list-directories",
                listDirectoriesNode);

            listDirectoriesNode["directories"].Sort(
                delegate(Node left, Node right)
                {
                    int leftInt = int.Parse(left.Name.Replace(_dbPath, "").Substring(2));
                    int rightInt = int.Parse(right.Name.Replace(_dbPath, "").Substring(2));
                    return leftInt.CompareTo(rightInt);
                });

            int maxFilesPerDirectory = int.Parse(ConfigurationManager.AppSettings["magix.core.database-files-per-directory"]);

            foreach (Node idxDirectory in listDirectoriesNode["directories"])
            {
                Node listFilesNode = new Node();
                listFilesNode["filter"].Value = "db*.hl";
                listFilesNode["directory"].Value = idxDirectory.Name;
                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(Database),
                    "magix.file.list-files",
                    listFilesNode);

                if (listFilesNode["files"].Count >= maxFilesPerDirectory)
                    continue;
                listFilesNode["files"].Sort(
                    delegate(Node left, Node right)
                    {
                        int leftInt = int.Parse(left.Name.Replace(idxDirectory.Name, "").Substring(3).Replace(".hl", ""));
                        int rightInt = int.Parse(right.Name.Replace(idxDirectory.Name, "").Substring(3).Replace(".hl", ""));
                        return leftInt.CompareTo(rightInt);
                    });
                for (int idxNo = 0; idxNo < listFilesNode["files"].Count; idxNo++)
                {
                    if (!listFilesNode["files"].Exists(
                        delegate(Node file)
                        {
                            return file.Name == idxDirectory.Name + "/db" + idxNo + ".hl";
                        }))
                    {
                        return idxDirectory.Name + "/db" + idxNo + ".hl";
                    }
                }
                return idxDirectory.Name + "/db" + listFilesNode["files"].Count + ".hl";
            }

            // didn't find an available file, without creating new directory
            for (int idxNo = 0; idxNo < listDirectoriesNode["directories"].Count; idxNo++)
            {
                if (!listDirectoriesNode["directories"].Exists(
                    delegate(Node dirNode)
                    {
                        return dirNode.Name == _dbPath + "db" + idxNo;
                    }))
                {
                    Node createDirectoryNode = new Node();
                    createDirectoryNode["directory"].Value = _dbPath + "db" + idxNo;
                    ActiveEvents.Instance.RaiseActiveEvent(
                        typeof(Database),
                        "magix.file.create-directory",
                        createDirectoryNode);

                    return createDirectoryNode["directory"].Get<string>() + "db0.hl";
                }
            }

            Node createNewDirectoryNode = new Node();
            createNewDirectoryNode["directory"].Value = _dbPath + "db" + listDirectoriesNode["directories"].Count;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.create-directory",
                createNewDirectoryNode);

            return createNewDirectoryNode["directory"].Get<string>() + "/db0.hl";
        }

        /*
         * returns an available file node
         */
        private static Node FindAvailableNode()
        {
            if (_transaction.Item1 != Guid.Empty)
            {
                // returning our "collection node" if we have an open transaction
                return GetDatabase()["added:"];
            }
            int objectsPerFile = int.Parse(ConfigurationManager.AppSettings["magix.core.database-objects-per-file"]);
            foreach (Node idxFileNode in _database)
            {
                if (idxFileNode.Count < objectsPerFile && !idxFileNode.Name.StartsWith("added:"))
                    return idxFileNode;
            }

            // no node existed, creating new
            Node newNode = new Node(FindAvailableNewFileName());
            _database.Add(newNode);
            return newNode;
        }

        /*
         * removes a file node from database
         */
        private static void RemoveNodeFromDatabase(Node fileObject)
        {
            if (_transaction.Item1 != Guid.Empty)
            {
                // not removing from disc if we have an open transaction
                return;
            }

            Node deleteFileNode = new Node();
            deleteFileNode["file"].Value = fileObject.Name;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.delete",
                deleteFileNode);

            string directoryName = fileObject.Name.Substring(0, fileObject.Name.LastIndexOf("/"));

            Node countFiles = new Node();
            countFiles["directory"].Value = directoryName;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.list-files",
                countFiles);

            if (countFiles["files"].Count == 0)
            {
                // deleting directory
                Node deleteDirectory = new Node();
                deleteDirectory["directory"].Value = directoryName;
                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(Database),
                    "magix.file.delete-directory",
                    deleteDirectory);
            }

            fileObject.UnTie();
        }

        #endregion
    }
}

