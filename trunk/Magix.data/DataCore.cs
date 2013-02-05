/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using Magix.UX.Builder;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;

namespace Magix.execute
{
	/**
	 */
	[ActiveController]
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
		}

		/**
		 */
		[ActiveEvent(Name = "magix.data.save")]
		public void magix_data_save (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("key"))
			{
				e.Params["key"].Value = "unique-key-of-object-to-save";
				e.Params["object"].Value = "nodes from here and down will be saved";
				return;
			}
			Node value = e.Params["object"];
			Node parent = value.Parent;
			value.Parent = null;
			new DeterministicExecutor(
			delegate
				{
					using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
					{
						db.Ext ().Configure ().UpdateDepth (1000);
						db.Ext ().Configure ().ActivationDepth (1000);
						string key = e.Params["key"].Get<string>();

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
				},
				delegate
				{
					value.Parent = parent;
				});
		}

		/**
		 */
		[ActiveEvent(Name = "magix.data.load")]
		public void magix_data_load (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("key"))
			{
				e.Params["key"].Value = "unique-key-of-object-to-load";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["key"].Get<string>();

				foreach (Storage idx in db.QueryByExample (new Storage(null, key)))
				{
					e.Params["object"].ReplaceChildren (idx.Node);
					e.Params["object"].Value = idx.Node.Value;
					db.Commit ();
					return;
				}
				db.Commit ();
			}
		}

		/**
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public void magix_data_count (object sender, ActiveEventArgs e)
		{
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				e.Params["count"].Value = db.QueryByExample (new Storage(null, null)).Count;
				db.Commit ();
			}
		}
	}
}

