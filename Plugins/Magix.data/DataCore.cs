/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 * However, this file, and the project files within this project,
 * as a whole is GPL, since it is linking towards Db4o, which is
 * being consumed as GPL
 */

using System;
using System.IO;
using System.Web;
using System.Collections;
using System.Configuration;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.data
{
	/*
	 * data storage
	 */
	internal sealed class DataCore : ActiveController
	{
        private static string _dbPath;
        private static string _appPath;
        private static Node _database;

        /*
         * slurps up everything from database
         */
        [ActiveEvent(Name = "magix.core.application-startup")]
        public static void magix_core_application_startup(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.application-startup-dox].Value");
                return;
            }

            lock (typeof(DataCore))
            {
                _appPath = HttpContext.Current.Server.MapPath("/");
                _appPath = _appPath.Replace("\\", "/");
                _dbPath = ConfigurationManager.AppSettings["magix.core.database-path"];
                _database = new Node();

                // retrieving data files
                Node listFilesNode = new Node();
                listFilesNode["filter"].Value = "db*.hl";
                listFilesNode["directory"].Value = _dbPath;
                RaiseActiveEvent(
                    "magix.file.list-files",
                    listFilesNode);
                foreach (Node idxFileNode in listFilesNode["files"])
                {
                    Node loadFile = new Node();
                    loadFile["file"].Value = idxFileNode.Name;
                    RaiseActiveEvent(
                        "magix.execute.code-2-node",
                        loadFile);

                    _database[idxFileNode.Name].Clear();
                    _database[idxFileNode.Name].AddRange(loadFile["node"]);
                }
            }
        }

		/*
		 * loads an object from database
		 */
		[ActiveEvent(Name = "magix.data.load")]
		public static void magix_data_load(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.load-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.load-sample]");
                return;
			}

            if (ip.Contains("id") && ip.Contains("prototype"))
                throw new ArgumentException("cannot use both [id] and [prototype] in [magix.data.load]");

            Node dp = Dp(e.Params);

			Node prototype = null;
            string id = null;
            if (ip.Contains("prototype"))
            {
                if (ip.ContainsValue("prototype"))
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                else
                    prototype = ip["prototype"];
            }
            else if (ip.ContainsValue("id"))
                id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
            else
                throw new ArgumentException("either [prototype] or [id] is needed for [magix.data.load]");


			int start = 0;
            if (ip.ContainsValue("start"))
                start = int.Parse(Expressions.GetExpressionValue(ip["start"].Get<string>(), dp, ip, false) as string);

			int end = -1;
            if (ip.ContainsValue("end"))
                end = int.Parse(Expressions.GetExpressionValue(ip["end"].Get<string>(), dp, ip, false) as string);

			if (id != null && (start != 0 || end != -1 || prototype != null))
				throw new ArgumentException("if you supply an [id], then [start], [end] and [prototype] cannot be defined");

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

		/*
		 * saves an object to database
		 */
		[ActiveEvent(Name = "magix.data.save")]
		public static void magix_data_save(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.save-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.save-sample]");
				return;
			}

            if (!ip.Contains("value"))
				throw new ArgumentException("[value] must be given to [magix.data.save]");

            Node value = null;

            Node dp = Dp(e.Params);
            if (ip.ContainsValue("value"))
                value = (Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as Node).Clone();
            else
                value = ip["value"].Clone();

            if (ip.Contains("id"))
            {
                string id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
                SaveById(value, id);
            }
            else
                ip["id"].Value = SaveNewObject(value, null);
		}

        private static void SaveById(Node value, string id)
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
                // updating existing object
                Node codeNode = new Node();
                codeNode["node"].Value = fileNode;
                RaiseActiveEvent(
                    "magix.execute.node-2-code",
                    codeNode);

                Node fileSaveNode = new Node();
                fileSaveNode["file"].Value = fileNode.Name;
                fileSaveNode["value"].Value = codeNode["code"].Get<string>();
                RaiseActiveEvent(
                    "magix.file.save",
                    fileSaveNode);
            }
            else
                SaveNewObject(value, id);
        }

        private static string SaveNewObject(Node value, string id)
        {
            if (string.IsNullOrEmpty(id))
                id = Guid.NewGuid().ToString().Replace("-", "");
            value.Name = "id";
            value.Value = id;
            Node availableFileNode = FindAvailableNode();
            if (availableFileNode == null)
            {
                availableFileNode = new Node(FindAvailableNewFileName());
                _database.Add(availableFileNode);
            }
            availableFileNode.Add(value);

            Node toCodeNode = new Node();
            toCodeNode["node"].Value = availableFileNode;
            RaiseActiveEvent(
                "magix.execute.node-2-code",
                toCodeNode);

            Node saveFileNode = new Node();
            saveFileNode["file"].Value = availableFileNode.Name;
            saveFileNode["value"].Value = toCodeNode["code"].Get<string>();
            RaiseActiveEvent(
                "magix.file.save",
                saveFileNode);

            return id;
        }

        private static string FindAvailableNewFileName()
        {
            Node listFilesNode = new Node();
            listFilesNode["filter"].Value = "db*.hl";
            listFilesNode["directory"].Value = _dbPath;
            RaiseActiveEvent(
                "magix.file.list-files",
                listFilesNode);
            listFilesNode["files"].Sort(
                delegate(Node left, Node right)
                {
                    int leftInt = int.Parse(left.Name.Replace(_dbPath, "").Substring(2).Replace(".hl", ""));
                    int rightInt = int.Parse(right.Name.Replace(_dbPath, "").Substring(2).Replace(".hl", ""));
                    return leftInt.CompareTo(rightInt);
                });
            for (int idxNo = 0; idxNo < listFilesNode["files"].Count; idxNo++)
            {
                if (!listFilesNode["files"].Exists(
                    delegate(Node file)
                    {
                        return file.Name == _dbPath + "db" + idxNo + ".hl";
                    }))
                {
                    return _dbPath + "db" + idxNo + ".hl";
                }
            }
            return _dbPath + "db" + listFilesNode["files"].Count + ".hl";
        }

        private static Node FindAvailableNode()
        {
            int objectsPerFile = int.Parse(ConfigurationManager.AppSettings["magix.core.database-objects-per-file"]);
            foreach (Node idxFileNode in _database)
            {
                if (idxFileNode.Count < objectsPerFile)
                    return idxFileNode;
            }
            return null;
        }

		/*
		 * removes an object from database
		 */
		[ActiveEvent(Name = "magix.data.remove")]
		public static void magix_data_remove(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.remove-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.remove-sample]");
                return;
			}

            if (ip.Contains("id") && ip.Contains("prototype"))
                throw new ArgumentException("cannot use both [id] and [prototype] in [magix.data.load]");

            Node dp = Dp(e.Params);

            Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (ip.ContainsValue("prototype"))
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                else
                    prototype = ip["prototype"];
            }

            if (!ip.ContainsValue("id") && prototype == null)
				throw new ArgumentException("missing [id] or [prototype] while trying to remove object");

            if (ip.Contains("id"))
            {
                string id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
                RemoveById(id);
            }
            else
                RemoveByPrototype(prototype);
        }

        private static void RemoveByPrototype(Node prototype)
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
                {
                    Node deleteFileNode = new Node();
                    deleteFileNode["file"].Value = _database[idx].Name;
                    RaiseActiveEvent(
                        "magix.file.delete",
                        deleteFileNode);
                }
                else
                {
                    Node toCodeNode = new Node();
                    toCodeNode["node"].Value = _database[idx];
                    RaiseActiveEvent(
                        "magix.execute.node-2-code",
                        toCodeNode);

                    Node fileSaveNode = new Node();
                    fileSaveNode["file"].Value = _database[idx].Name;
                    fileSaveNode["value"].Value = toCodeNode["code"].Get<string>();
                    RaiseActiveEvent(
                        "magix.file.save",
                        fileSaveNode);
                }
            }
        }

        private static void RemoveById(string id)
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
                {
                    Node toCodeNode = new Node();
                    toCodeNode["node"].Value = fileObject;
                    RaiseActiveEvent(
                        "magix.execute.node-2-code",
                        toCodeNode);

                    Node fileSaveNode = new Node();
                    fileSaveNode["file"].Value = fileObject.Name;
                    fileSaveNode["value"].Value = toCodeNode["code"].Get<string>();
                    RaiseActiveEvent(
                        "magix.file.save",
                        fileSaveNode);
                }
                else
                {
                    Node deleteFileNode = new Node();
                    deleteFileNode["file"].Value = fileObject.Name;
                    RaiseActiveEvent(
                        "magix.file.delete",
                        deleteFileNode);
                    fileObject.UnTie();
                }
            }
        }

		/*
		 * counts objects in database
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public static void magix_data_count(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(e.Params))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.count-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.data",
                    "Magix.data.hyperlisp.inspect.hl",
                    "[magix.data.count-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (ip.ContainsValue("prototype"))
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                else
                    prototype = ip["prototype"];
            }

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
	}
}

