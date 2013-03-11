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
		private static string _dbFile = "data-storage.db4o";

		[ActiveEvent(Name = "magix.data.remove")]
		public static void magix_data_remove(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.data.remove"].Value = null;
				e.Params["id"].Value = "object-id";
				e.Params["inspect"].Value = @"removes the given [id] or [prototype] object(s) from 
your persistent data storage";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;


			Node prototype = null;
			if (ip.Contains("prototype"))
				prototype = ip["prototype"];

			if ((!e.Params.Contains("id") || string.IsNullOrEmpty(e.Params["id"].Get<string>())) && prototype == null)
				throw new ArgumentException("missing [id] or [prototype] while trying to remove object");

			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					string id = ip["id"].Get<string>();
					foreach (Storage idx in db.Ext().Query<Storage>(
						delegate(Storage obj)
						{
							if (id != null)
								return obj.Id == id;
							else
								return obj.Node.HasNodes(prototype);
						}))
					{
						db.Delete(idx);
					}
					db.Commit();
				}
			}
		}

		[ActiveEvent(Name = "magix.data.save")]
		public static void magix_data_save(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.data.save"].Value = null;
				e.Params["id"].Value = "object-id";
				e.Params["object"]["value"].Value = "value of object";
				e.Params["inspect"].Value = @"will serialize the given [object] with 
the given [id] in the persistent data storage.&nbsp;&nbsp;if no [id] is given, 
a global unique identifier will be automatically assigned to the object";
				return;
			}

			if (!e.Params.Contains("object"))
				throw new ArgumentException("[object] must be defined for magix.data.save to actually save anything");

			Node value = e.Params["object"].Clone();

			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

					string id = e.Params.Contains("id") ? 
						e.Params["id"].Get<string>() : 
						Guid.NewGuid().ToString();
					bool found = false;

					// checking to see if we should update existing object
					foreach (Storage idx in db.QueryByExample(new Storage(null, id)))
					{
						idx.Node = value;
						db.Store(idx);
						found = true;
						break;
					}
					if (!found)
					{
						db.Store(new Storage(value, id));
					}
					db.Commit ();
				}
			}
		}

		[ActiveEvent(Name = "magix.data.load")]
		public static void magix_data_load(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.data.load"].Value = null;
				e.Params["id"].Value = "object-id";
				e.Params["inspect"].Value = @"loads the given [id] object, or 
use [prototype] as filter.&nbsp;&nbsp;returns objects found as [objects], with 
child nodes of [objects] being the matching objects.&nbsp;&nbsp;
use [start] and [end] to fetch a specific slice of objects, [start] defaults 
to 0 and [end] defaults to -1, which means all objects matching criteria.&nbsp;&nbsp;
[start], [end] and [prototype] cannot be defined if [id] is given, since [id] is unique,
and will make sure only one object is loaded";
				return;
			}

			Node prototype = null;
			if (e.Params.Contains("prototype"))
				prototype = e.Params["prototype"];

			string id = null;
			if (e.Params.Contains("id") && e.Params["id"].Value != null)
				id = e.Params["id"].Get<string>();

			int start = 0;
			if (e.Params.Contains("start") && e.Params["start"].Value != null)
				start = e.Params["start"].Get<int>();

			int end = -1;
			if (e.Params.Contains("end") && e.Params["end"].Value != null)
				end = e.Params["end"].Get<int>();

			if (id != null && start != 0 && end != -1 && prototype != null)
				throw new ArgumentException("if you supply an [id], then [start], [end] and [prototype] cannot be defined");

			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

					int idxNo = 0;
					foreach (Storage idx in db.Ext().Query<Storage>(
						delegate(Storage obj)
						{
							if (id != null)
								return obj.Id == id;
							else
								return obj.Node.HasNodes(prototype);
						}))
					{
						if (idxNo >= start && (end == -1 || idxNo < end))
							e.Params["objects"][idx.Id].ReplaceChildren(idx.Node);
						idxNo++;
					}
				}
			}
		}

		/**
		 * Returns the total number of objects in your data storage
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public static void magix_data_count(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.data.count"].Value = null;
				e.Params["inspect"].Value = @"returns the total number 
of objects in data storage as [count]";
				return;
			}
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

					// TODO: Refactor ...
					e.Params["count"].Value = db.QueryByExample (new Storage(null, null)).Count;
				}
			}
		}
	}
}

