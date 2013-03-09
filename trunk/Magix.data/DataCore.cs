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

		/**
		 * Will remove the given "object" with the ID found in Value
		 */
		[ActiveEvent(Name = "magix.data.remove")]
		public static void magix_data_remove(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["remove"]["id"].Value = "object-id";
				e.Params["inspect"].Value = @"removes the given [id] object from 
your persistent data storage";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			if (!e.Params.Contains("id") || string.IsNullOrEmpty(e.Params["id"].Get<string>()))
				throw new ArgumentException("missing [id] while trying to remove object");

			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					string key = ip["id"].Get<string>();
					foreach (Storage idx in db.QueryByExample(new Storage(null, key)))
					{
						db.Delete(idx);
						db.Commit();
					}
				}
			}
		}

		/**
		 * Will save the given "object" with the given ID found in Value
		 */
		[ActiveEvent(Name = "magix.data.save")]
		public static void magix_data_save(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["save"]["id"].Value = "object-id";
				e.Params["save"]["object"].Value = "object to save";
				e.Params["save"]["object"]["message"].Value = "more object";
				e.Params["inspect"].Value = @"will serialize the given [object] with 
the given [id] in the persistent data storage";
				return;
			}

			Node value = null;
			if (e.Params.Contains("object"))
				value = e.Params["object"];
			else
				throw new ArgumentException("object must be defined before calling magix.data.save");

			if (!e.Params.Contains("id") || string.IsNullOrEmpty(e.Params["id"].Get<string>()))
				throw new ArgumentException("missing [id] while trying to save object");

			Node parent = value.Parent;
			value.SetParent(null);

			new DeterministicExecutor(
			delegate
				{
					lock (typeof(Node))
					{
						using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
						{
							db.Ext().Configure().UpdateDepth(1000);
							db.Ext().Configure().ActivationDepth(1000);
							string key = e.Params["id"].Get<string>();
							bool found = false;
							foreach (Storage idx in db.QueryByExample(new Storage(null, key)))
							{
								idx.Node = value;
								db.Store(idx);
								found = true;
								break;
							}
							if (!found)
							{
								db.Store(new Storage(value, key));
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
		 */
		[ActiveEvent(Name = "magix.data.load")]
		public static void magix_data_load(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.data.load"].Value = null;
				e.Params["load"]["id"].Value = "object-id";
				e.Params["load"]["prototype"].Value = "optional";
				e.Params["inspect"].Value = @"loads the given [id] object, or 
use [prototype] as filter.&nbsp;&nbsp;returns objects found as [objects], with 
child nodes of [objects] being the matching objects.&nbsp;&nbsp;
use [start] and [end] to fetch a specific cut of objects, [start] defaults 
to 0 and [end] defaults to 10";
				return;
			}

			Node prototype = null;
			if (e.Params.Contains("prototype"))
				prototype = e.Params["prototype"];

			string key = null;
			if (e.Params.Contains("id") && e.Params["id"].Value != null)
				key = e.Params["id"].Get<string>();

			int start = 0;
			if (e.Params.Contains("start") && e.Params["start"].Value != null)
				start = e.Params["start"].Get<int>();

			int end = 10;
			if (e.Params.Contains("end") && e.Params["end"].Value != null)
				end = e.Params["end"].Get<int>();

			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

					IList<Storage> objects = db.Ext().Query<Storage>(
						delegate(Storage obj)
						{
							if (key != null)
								return obj.Key == key;
							else
								return obj.Node.HasNodes(prototype);
						});
					int idxNo = 0;
					foreach (Storage idx in objects)
					{
						if (idxNo >= start && idxNo < end)
							e.Params["objects"][idx.Key].ReplaceChildren(idx.Node);
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
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);

					// TODO: Refactor ...
					e.Params["count"].Value = db.QueryByExample (new Storage(null, null)).Count;
				}
			}
		}
	}
}

