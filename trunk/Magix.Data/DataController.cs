/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;

namespace Magix.execute
{
	/**
	 */
	[ActiveController]
	public class DataController : ActiveController
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

		public class Event
		{
			private string _key;
			private Node _node;

			public Event(Node node, string key)
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
		[ActiveEvent(Name = "Magix.Data.Save")]
		public void Magix_Data_Save (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Key"))
			{
				e.Params["Key"].Value = "unique-key-of-object-to-save";
				e.Params["Value"].Value = "nodes from here and down will be saved";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["Key"].Get<string>();
				Node value = e.Params["Value"];

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

		/**
		 */
		[ActiveEvent(Name = "Magix.Data.OverrideEvent")]
		public void Magix_Data_OverrideEvent (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Event"))
			{
				e.Params["Event"].Value = "Name of active event";
				e.Params["Code"].Value = "nodes from here and down will be saved as code and executed upon raising of event";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["Event"].Get<string>();
				Node value = e.Params["Code"];

				bool found = false;
				foreach (Event idx in db.QueryByExample (new Event(null, key)))
				{
					idx.Node = value;
					db.Store (idx);
					found = true;
					break;
				}
				if (!found)
				{
					db.Store (new Event(value, key));
				}
				db.Commit ();
				ActiveEvents.Instance.CreateEventMapping (key, "Magix.Data._ActiveEvent2Code_Callback");
			}

			Node node = new Node();
			RaiseEvent ("Magix.Samples._GetActiveEvents", node);
			RaiseEvent ("Magix.Samples._PopulateEventViewer", node);
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.Data.RemoveOverride")]
		public void Magix_Data_RemoveOverride (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Event"))
			{
				e.Params["Event"].Value = "Name of active event to remove";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["Event"].Get<string>();

				foreach (Event idx in db.QueryByExample (new Event(null, key)))
				{
					db.Delete (idx);
					break;
				}
				db.Commit ();
				ActiveEvents.Instance.RemoveMapping (key);
			}

			Node node = new Node();
			RaiseEvent ("Magix.Samples._GetActiveEvents", node);
			RaiseEvent ("Magix.Samples._PopulateEventViewer", node);
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.Data._ActiveEvent2Code_Callback")]
		public void Magix_Data__ActiveEvent2Code_Callback (object sender, ActiveEventArgs e)
		{
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Name;

				foreach (Event idx in db.QueryByExample (new Event(null, key)))
				{
					idx.Node.Name = null;
					idx.Node.CleanUp ();
					if (e.Params.Contains ("inspect"))
					{
						e.Params["Event"].Value = e.Name;
						e.Params["Code"].Clear ();
						e.Params["Code"].AddRange (idx.Node);
						e.Params["inspect"].UnTie ();
						e.Params.Value = idx.Node.Value;
					}
					else
					{
						RaiseEvent ("Magix.execute", idx.Node);
					}
					return;
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.Core._ApplicationStartup")]
		public static void Magix_Core__ApplicationStartup (object sender, ActiveEventArgs e)
		{
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);

				foreach (Event idx in db.QueryByExample (new Event(null, null)))
				{
					ActiveEvents.Instance.CreateEventMapping (idx.Key, "Magix.Data._ActiveEvent2Code_Callback");
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.Data.Load")]
		public void Magix_Data_Load (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Key"))
			{
				e.Params["Key"].Value = "unique-key-of-object-to-load";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["Key"].Get<string>();

				foreach (Storage idx in db.QueryByExample (new Storage(null, key)))
				{
					e.Params["Value"].ReplaceChildren (idx.Node);
					idx.Node.CleanUp ();
					e.Params["Value"].Value = idx.Node.Value;
					db.Commit ();
					return;
				}
				db.Commit ();
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.Data.Count")]
		public void Magix_Data_Count (object sender, ActiveEventArgs e)
		{
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				e.Params["Count"].Value = db.QueryByExample (new Storage(null, null)).Count;
				db.Commit ();
			}
		}
	}
}

