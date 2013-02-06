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
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["Content"].Value = "Or 'context' node!!!";
				e.Params["key"].Value = "unique-key-id666";
				e.Params["object"].Value = "nodes from here and down will be saved";
				e.Params["object"]["message"].Value = "Use either 'object' node!!";
				e.Params["context"].Value = "[Data]";
				e.Params["inspect"].Value = @"Will save the given given ""object"" node,
OR the given ""context"" node's Value, which should be an expression, pointing to
a node, which will become saved in its entirety.";
				return;
			}
			Node value = null;
			if (!e.Params.Contains ("object") && e.Params.Contains ("context"))
				value = Expressions.GetExpressionValue(e.Params["context"].Get<string>(), e.Params, e.Params) as Node;
			else if (e.Params.Contains ("object"))
				value = e.Params["object"];
			else
				throw new ArgumentException("Either context or object must be defined before calling magix.data.save");
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
			if (e.Params.Contains ("inspect"))
			{
				e.Params["key"].Value = "unique-key-of-object-to-load";
				e.Params["context"].Value = "[OR][Expression][Pointing][ToNode][ResultSet]";
				e.Params["inspect"].Value = @"Will load the object from the data storage
with the given ""key"" node into the ""object"" child return node, 
or the given ""context"" pointer to a node, which will be 
transformed into the returned object from the data storage.";
				return;
			}
			Node context = null;
			if (e.Params.Contains ("context"))
			{
				context = Expressions.GetExpressionValue (
					e.Params["context"].Get<string>(), 
					e.Params, 
					e.Params) as Node;
			}
			else
			{
				context = e.Params["object"];
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["key"].Get<string>();

				foreach (Storage idx in db.QueryByExample (new Storage(null, key)))
				{
					context.ReplaceChildren (idx.Node);
					context.Value = idx.Node.Value;
					context.Name = idx.Node.Name;
					return;
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public void magix_data_count (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["inspect"].Value = @"Will return the number of objects
that exists in the data storage in the ""count"" child node. Takes no parameters.";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				e.Params["count"].Value = db.QueryByExample (new Storage(null, null)).Count;
			}
		}
	}
}

