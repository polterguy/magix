/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 * However, this file, and the project files within this project,
 * as a whole is GPL, since it is linking towards Db4o, which is
 * being consumed as GPL
 */

using System;
using System.Collections;
using System.Collections.Generic;
using Magix.Core;
using Magix.UX.Builder;
using Db4objects.Db4o;
using Db4objects.Db4o.Ext;
using Db4objects.Db4o.Config;

namespace Magix.execute
{
	/**
	 * Controller for helping to save and load data from db4o, which is a minimalistic 
	 * object database used in this controller to store and retrieve data from permanent
	 * storage
	 */
	public class DataCore : ActiveController
	{
		private static string _dbFile = "store.db4o";

		public class Storage
		{
			private string _key;
			private Node _node;

			public Storage(Node node, string key)
			{
				_node = node;
				_key = key;
			}

			public string Key
			{
				get { return _key; }
				set { _key = value; }
			}

			public Node Node
			{
				get { return _node; }
				set { _node = value; }
			}

			public override bool Equals (object obj)
			{
				if (obj == null || !(obj is Storage))
					return false;

				Storage rhs = obj as Storage;
				return Key == rhs.Key && Node.Equals (rhs.Node);
			}

			public override int GetHashCode ()
			{
				return Key.GetHashCode () + Node.GetHashCode ();
			}
		}

		/**
		 * Will remove the given "object" with the given key found in Value
		 */
		[ActiveEvent(Name = "magix.data.remove")]
		public static void magix_data_remove (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["remove"].Value = "unique-key-id666";
				e.Params["inspect"].Value = @"Will remove the object
with the given Key found in Value.";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			if (string.IsNullOrEmpty (ip.Get<string>()))
				throw new ArgumentException("Missing 'key' while trying to remove object");

			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					string key = ip.Get<string>();
					foreach (Storage idx in db.QueryByExample (new Storage(null, key)))
					{
						db.Delete (idx);
						db.Commit ();
					}
				}
			}
		}

		/**
		 * Will save the given "object" with the given key found in Value
		 */
		[ActiveEvent(Name = "magix.data.save")]
		public static void magix_data_save (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["save"].Value = "unique-key-id666";
				e.Params["save"]["object"].Value = "nodes from here and down will be saved";
				e.Params["save"]["object"]["message"].Value = "Use either 'object' node!!";
				e.Params["inspect"].Value = @"Will save the given given ""object"" node, 
which should be an expression, pointing to
a node, which will become saved in its entirety.";
				return;
			}
			Node value = null;
			if (e.Params.Contains ("object"))
				value = e.Params["object"];
			else
				throw new ArgumentException("object must be defined before calling magix.data.save");
			if (string.IsNullOrEmpty (e.Params.Get<string>()))
				throw new ArgumentException("Missing Value while trying to store object");
			Node parent = value.Parent;
			value.SetParent(null);
			new DeterministicExecutor(
			delegate
				{
					lock (typeof(Node))
					{
						using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
						{
							db.Ext ().Configure ().UpdateDepth (1000);
							db.Ext ().Configure ().ActivationDepth (1000);
							string key = e.Params.Get<string>();
							bool found = false;
							foreach (Storage idx in db.QueryByExample (new Storage(null, key)))
							{
								idx.Node = value;
								db.Store (idx);
								found = true;
								break;
							}
							if (!found)
							{
								db.Store (new Storage(value, key));
							}

							db.Commit ();
						}
					}
				},
				delegate
				{
					value.SetParent(parent);
				});
		}

		/**
		 * Will load the given Key found from Value, or "prototype". If you use key, this is the same
		 * key the object was stored with. If you use "prototype", then this will 
		 * serve as a tree which must be present in the saved object for it to return 
		 * a match
		 */
		[ActiveEvent(Name = "magix.data.load")]
		public static void magix_data_load (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["load"].Value = "unique-key-of-object-to-load";
				e.Params["load"]["prototype"].Value = "optional parameter, being a 'query object' which the returned object must match";
				e.Params["inspect"].Value = @"Will load the object from the data storage
with the given ""key"" node into the ""object"" child return node.";
				return;
			}
			Node prototype = null;
			if (e.Params.Contains ("prototype"))
			{
				prototype = e.Params["prototype"];
			}
			string key = null;
			if (e.Params.Get<string>("") != string.Empty)
				key = e.Params.Get<string>();
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);

					IList<Storage> objects = db.Ext ().Query<Storage>(
						delegate(Storage obj)
						{
							if (key != null)
								return obj.Key == key;
							else
								return obj.Node.HasNodes(prototype);
						});
					if (objects.Count > 0)
					{
						Storage idx = objects[0];
						e.Params["object"].ReplaceChildren (idx.Node);
						return;
					}
				}
			}
		}

		/**
		 * Returns the number of objects in your data storage
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public static void magix_data_count (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["inspect"].Value = @"Will return the number of objects
that exists in the data storage in the ""count"" child node. Takes no parameters.";
				return;
			}
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);
					e.Params["count"].Value = db.QueryByExample (new Storage(null, null)).Count;
				}
			}
		}
	}
}

