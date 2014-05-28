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
        private static string _dbPath;
        private static string _appPath;
        private static Node _database;
        private static AutoResetEvent _resetEvent = new AutoResetEvent(true);
        private static int _resetEventLoadCount = 0;

        #region [ -- publicly available methods -- ]

        internal static void Initialize()
        {
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

        /*
         * loads items from database
         */
        internal static void LoadItems(Node ip, Node prototype, string id, int start, int end)
        {
            _resetEvent.WaitOne();
            try
            {
                int curMatchingItem = 0;
                foreach (Node idxFileNode in _database)
                {
                    foreach (Node idxObjectNode in idxFileNode)
                    {
                        if (id != null && id == idxObjectNode.Get<string>())
                        {
                            // loading by id
                            Node curNode = idxObjectNode.Clone();
                            ip["objects"][idxObjectNode.Get<string>()].AddRange(curNode);
                            return;
                        }
                        else if (id == null)
                        {
                            // loading by prototype
                            if (idxObjectNode.HasNodes(prototype))
                            {
                                if ((start == 0 || curMatchingItem >= start) && (end == -1 || curMatchingItem < end))
                                {
                                    Node curNode = idxObjectNode.Clone();
                                    ip["objects"][idxObjectNode.Get<string>()].AddRange(curNode);
                                }
                                curMatchingItem += 1;
                            }
                        }
                    }
                }
            }
            finally
            {
                _resetEvent.Set();
            }
        }

        /*
         * counts records in database
         */
        internal static void CountRecords(Node ip, Node prototype)
        {
            _resetEvent.WaitOne();
            try
            {
                int count = 0;
                foreach (Node idxFileNode in _database)
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
            finally
            {
                _resetEvent.Set();
            }
        }

        /*
         * saves an object by its id
         */
        internal static void SaveById(Node value, string id)
        {
            _resetEvent.WaitOne();
            try
            {
                Node fileNode = null;
                value.Value = id;
                value.Name = "id";
                foreach (Node idxFileNode in _database)
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
            finally
            {
                _resetEvent.Set();
            }
        }

        /*
         * saves a new object
         */
        internal static string SaveNewObject(Node value)
        {
            _resetEvent.WaitOne();
            try
            {
                return SaveNewObject(value, null);
            }
            finally
            {
                _resetEvent.Set();
            }
        }

        /*
         * removes items from database according to prototype
         */
        internal static void RemoveByPrototype(Node prototype)
        {
            _resetEvent.WaitOne();
            try
            {
                List<Node> nodesToRemove = new List<Node>();
                List<string> filesToUpdate = new List<string>();
                foreach (Node idxFileNode in _database)
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
                    if (_database[idx].Count == 0)
                        RemoveNodeFromDatabase(_database[idx]);
                    else
                        SaveFileNodeToDisc(_database[idx]);
                }
            }
            finally
            {
                _resetEvent.Set();
            }
        }

        /*
         * removes a node by its id
         */
        internal static void RemoveById(string id)
        {
            _resetEvent.WaitOne();
            try
            {
                Node objectToRemove = null;
                foreach (Node idxFileNode in _database)
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
            finally
            {
                _resetEvent.Set();
            }
        }

        #endregion

        #region [ -- private methods -- ]

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
            int objectsPerFile = int.Parse(ConfigurationManager.AppSettings["magix.core.database-objects-per-file"]);
            foreach (Node idxFileNode in _database)
            {
                if (idxFileNode.Count < objectsPerFile)
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

