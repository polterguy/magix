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
        private static Node _database;
        private static Tuple<Guid, Node> _transaction = new Tuple<Guid, Node>(Guid.Empty, new Node());

        #region [ -- initialization -- ]

        internal static void Initialize()
        {
            lock (_locker)
            {
                lock (_transactionalLocker)
                {
                    if (_dbPath != null)
                        return; // multiple initializations might occur

                    _dbPath = ConfigurationManager.AppSettings["magix.core.database-path"];
                    _database = new Node();

                    foreach (string idxDirectory in GetDirectories(_dbPath))
                    {
                        foreach (string idxFile in GetFiles(idxDirectory))
                        {
                            _database.Add(LoadFile(idxFile));
                        }
                    }
                }
            }
        }

        /*
         * returns all directories within db path
         */
        private static IEnumerable<string> GetDirectories(string directory)
        {
            Node directories = new Node();
            directories["directory"].Value = directory;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.list-directories",
                directories);

            directories["directories"].Sort(
                delegate(Node left, Node right)
                {
                    int leftInt = int.Parse(left.Name.Replace(_dbPath, "").Substring(2));
                    int rightInt = int.Parse(right.Name.Replace(_dbPath, "").Substring(2));
                    return leftInt.CompareTo(rightInt);
                });

            foreach (Node idxDirectory in directories["directories"])
                yield return idxDirectory.Name;
        }

        /*
         * returns files within directory
         */
        private static IEnumerable<string> GetFiles(string directory)
        {
            Node files = new Node();
            files["filter"].Value = "db*.hl";
            files["directory"].Value = directory;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.list-files",
                files);

            files["files"].Sort(
                delegate(Node left, Node right)
                {
                    int leftInt = int.Parse(left.Name.Replace(directory, "").Substring(3).Replace(".hl", ""));
                    int rightInt = int.Parse(right.Name.Replace(directory, "").Substring(3).Replace(".hl", ""));
                    return leftInt.CompareTo(rightInt);
                });

            foreach (Node idxFile in files["files"])
                yield return idxFile.Name;
        }

        /*
         * loads file as node
         */
        private static Node LoadFile(string filename)
        {
            Node file = new Node();
            file["file"].Value = filename;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.execute.code-2-node",
                file);

            file["node"].Name = filename;
            return file[filename].UnTie();
        }

        #endregion

        #region [ -- transactional support -- ]

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

                // removing items that should be removed first
                CommitRemoveEmptyFiles(_database);

                // updating changed items
                CommitUpdateChangedFiles(_database);

                // adding new items
                CommitSaveNewObjects(_database);
            }
        }

        /*
         * removing empty files from database in commit
         */
        private static void CommitRemoveEmptyFiles(Node database)
        {
            List<Node> toBeRemoved = new List<Node>();
            foreach (Node idxFileNode in database)
            {
                // removing empty files, unless they were added during this transaction
                if (idxFileNode.Count == 0 && idxFileNode.Name != "added:")
                    toBeRemoved.Add(idxFileNode);
            }
            foreach (Node idxFileNode in toBeRemoved)
            {
                RemoveFileFromDatabase(idxFileNode);
            }
        }

        /*
         * updating changed files from database in commit
         */
        private static void CommitUpdateChangedFiles(Node database)
        {
            foreach (Node idxFileNode in database)
            {
                // updating changed files, unless they have zero objects
                if (idxFileNode.Name.StartsWith("changed:") && idxFileNode.Count > 0)
                {
                    idxFileNode.Name = idxFileNode.Name.Substring(8);
                    SaveFileNodeToDisc(idxFileNode);
                }
            }
        }

        /*
         * saving new objects into available files in commit
         */
        private static void CommitSaveNewObjects(Node database)
        {
            List<Node> nodesToRemove = new List<Node>();
            int objectsPerFile = int.Parse(ConfigurationManager.AppSettings["magix.core.database-objects-per-file"]);
            foreach (Node idxFileNode in database)
            {
                // finding all objects in database that were added during this transaction
                if (idxFileNode.Name == "added:")
                {
                    // looping as long as we have more objects to add
                    Node availableNode = FindAvailableNode();
                    while (idxFileNode.Count > 0)
                    {
                        // making sure no files have more objects than what the database is configured to handle
                        while (availableNode.Count < objectsPerFile && idxFileNode.Count > 0)
                        {
                            availableNode.Add(idxFileNode[0].UnTie());
                        }
                        SaveFileNodeToDisc(availableNode);
                        availableNode = FindAvailableNode();
                    }
                    nodesToRemove.Add(idxFileNode);
                    break;
                }
            }
            foreach (Node idxFileNode in nodesToRemove)
            {
                _database.Remove(idxFileNode);
            }
        }

        #endregion

        #region [ -- public methods -- ]

        /*
         * loads items from database
         */
        internal static void Load(Node ip, Node prototype, int start, int end, Guid transaction, bool onlyId)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    Load(ip, prototype, start, end, transaction, onlyId);
                }
            }
            lock (_locker)
            {
                int curMatchingItem = 0;
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        // loading by prototype
                        if (idxObjectNode["value"].HasNodes(prototype))
                        {
                            if ((start == 0 || curMatchingItem >= start) && (end == -1 || curMatchingItem < end))
                            {
                                Node objectNode = new Node("object");
                                objectNode["id"].Value = idxObjectNode.Get<string>();
                                objectNode["created"].Value = idxObjectNode["created"].Value;
                                objectNode["revision-count"].Value = idxObjectNode["revision-count"].Value;
                                if (!onlyId)
                                    objectNode["value"].AddRange(idxObjectNode["value"].Clone());
                                ip["objects"].Add(objectNode);
                            }
                            curMatchingItem += 1;
                        }
                    }
                }
            }
        }

        /*
         * loads one item from database
         */
        internal static void Load(Node ip, string id, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    Load(ip, id, transaction);
                }
            }
            lock (_locker)
            {
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (id == idxObjectNode.Get<string>())
                        {
                            // loading by id
                            ip["created"].Value = idxObjectNode["created"].Value;
                            ip["revision-count"].Value = idxObjectNode["revision-count"].Value;
                            ip["value"].AddRange(idxObjectNode["value"].Clone());
                            return;
                        }
                    }
                }
            }
        }

        /*
         * counts records in database
         */
        internal static int CountRecords(Node ip, Node prototype, Guid transaction)
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
                        else if (idxObjectNode["value"].HasNodes(prototype))
                            count += 1;
                    }
                }
                return count;
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
                value.Value = id;
                value.Name = "id";

                // checking to see if object already exist
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (idxObjectNode.Get<string>() == id)
                        {
                            idxObjectNode["value"].UnTie();
                            idxObjectNode["updated"].Value = DateTime.Now;
                            idxObjectNode["revision-count"].Value = idxObjectNode["revision-count"].Get<decimal>() + 1M;
                            idxObjectNode["value"].AddRange(value);
                            SaveFileNodeToDisc(idxFileNode);
                            return;
                        }
                    }
                }

                // object didn't exist, saving new object
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
        internal static int RemoveByPrototype(Node prototype, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    return RemoveByPrototype(prototype, transaction);
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
                        if (idxObjectNode["value"].HasNodes(prototype))
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
                    idx.UnTie();
                foreach (string idx in filesToUpdate)
                {
                    if (GetDatabase()[idx].Count == 0)
                        RemoveFileFromDatabase(GetDatabase()[idx]);
                    else
                        SaveFileNodeToDisc(GetDatabase()[idx]);
                }
                return nodesToRemove.Count;
            }
        }

        /*
         * removes a node by its id
         */
        internal static int RemoveById(string id, Guid transaction)
        {
            if (transaction != _transaction.Item1)
            {
                lock (_transactionalLocker)
                {
                    return RemoveById(id, transaction);
                }
            }
            lock (_locker)
            {
                foreach (Node idxFileNode in GetDatabase())
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (idxObjectNode.Get<string>() == id)
                        {
                            idxObjectNode.UnTie();
                            if (idxFileNode.Count > 0)
                                SaveFileNodeToDisc(idxFileNode);
                            else
                                RemoveFileFromDatabase(idxFileNode);
                            return 1;
                        }
                    }
                }
                return 0;
            }
        }

        #endregion

        #region [ -- private helpers -- ]

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

            Node newObject = new Node("id", id);
            newObject["created"].Value = DateTime.Now;
            newObject["revision-count"].Value = 1M;
            newObject["value"].AddRange(value.Clone());

            Node availableFileNode = FindAvailableNode();
            availableFileNode.Add(newObject);
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

            // transforming node to code
            Node codeNode = new Node();
            codeNode["node"].Value = fileNode;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.execute.node-2-code",
                codeNode);

            // saving file
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
            int maxFilesPerDirectory = int.Parse(ConfigurationManager.AppSettings["magix.core.database-files-per-directory"]);

            // checking to see if we can use existing directory
            List<string> directoryList = new List<string>(GetDirectories(_dbPath));
            foreach (string idxDirectory in directoryList)
            {
                List<string> filesList = new List<string>(GetFiles(idxDirectory));
                if (filesList.Count >= maxFilesPerDirectory)
                    continue;
                for (int idxNo = 0; idxNo < filesList.Count; idxNo++)
                {
                    if (!filesList.Exists(
                        delegate(string file)
                        {
                            return file == idxDirectory + "/db" + idxNo + ".hl";
                        }))
                        return idxDirectory + "/db" + idxNo + ".hl";
                }
                return idxDirectory + "/db" + filesList.Count + ".hl";
            }

            // didn't find an available filename, without creating new directory
            for (int idxNo = 0; idxNo < directoryList.Count; idxNo++)
            {
                if (!directoryList.Exists(
                    delegate(string dirNode)
                    {
                        return dirNode == _dbPath + "db" + idxNo;
                    }))
                {
                    CreateNewDirectory(_dbPath + "db" + idxNo);
                    return _dbPath + "db" + idxNo + "/db0.hl";
                }
            }

            CreateNewDirectory(_dbPath + "db" + directoryList.Count);
            return _dbPath + "db" + directoryList.Count + "/db0.hl";
        }

        /*
         * helper to create directory
         */
        private static void CreateNewDirectory(string directory)
        {
            Node createDirectoryNode = new Node();
            createDirectoryNode["directory"].Value = directory;
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(Database),
                "magix.file.create-directory",
                createDirectoryNode);
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
        private static void RemoveFileFromDatabase(Node fileObject)
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

            // checking to see if directory is empty
            List<string> files = new List<string>(GetFiles(directoryName));
            if (files.Count == 0)
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

