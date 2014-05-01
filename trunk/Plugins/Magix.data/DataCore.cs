/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
using System.Web;

namespace Magix.execute
{
	/**
	 * data storage
	 */
	public class DataCore : ActiveController
	{
		private static string _dbFile = "database/data-storage.db4o";

		/**
		 * loads an object from database
		 */
		[ActiveEvent(Name = "magix.data.load")]
		public static void magix_data_load(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>loads the first object matching the given [id], 
or if you supply a [prototype], it will return all objects matching your prototype</p>
<p>returns objects found as [objects], with child nodes of [objects] being the matching 
objects</p>
<p>use [start] and [end] to fetch a specific slice of objects, [start] defaults 
to 0 and [end] defaults to -1, which means all objects matching criteria.&nbsp;&nbsp;
[start], [end] and [prototype] cannot be defined if [id] is given, since [id] is supposed 
to be unique, and will make sure only one object is loaded</p><p>if a [prototype] node is 
given, it can contain node values with % to signify wildcards for a match operation</p>
<p>thread safe</p>";
                e.Params["magix.data.load"]["id"].Value = "object-id";
                return;
			}

            Node ip = Ip(e.Params);

			Node prototype = null;
            if (ip.Contains("prototype"))
				prototype = ip["prototype"];

			string id = null;
            if (ip.Contains("id") && ip["id"].Value != null)
                id = ip["id"].Get<string>();

			int start = 0;
            if (ip.Contains("start") && ip["start"].Value != null)
                start = ip["start"].Get<int>();

			int end = -1;
            if (ip.Contains("end") && ip["end"].Value != null)
                end = ip["end"].Get<int>();

			if (id != null && start != 0 && end != -1 && prototype != null)
				throw new ArgumentException("if you supply an [id], then [start], [end] and [prototype] cannot be defined");

			lock (typeof(DataCore))
			{
				using (IObjectContainer db = Db4oEmbedded.OpenFile(HttpContext.Current.Request.MapPath("~/" + _dbFile)))
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
                            ip["objects"][idx.Id].ReplaceChildren(idx.Node.Clone());
						idxNo++;
					}
					db.Close();
				}
			}
		}

		/**
		 * saves an object to database
		 */
		[ActiveEvent(Name = "magix.data.save")]
		public static void magix_data_save(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>will serialize the given [value] node with 
the given [id] in the persistent data storage</p><p>if no [id] is given, a global unique 
identifier will be automatically assigned to the object.&nbsp;&nbsp;if an [id] is given, 
and an object with that same id exists, object will be overwritten or updated</p><p>
thread safe</p>";
                e.Params["magix.data.save"]["id"].Value = "object-id";
                e.Params["magix.data.save"]["value"]["some-value"].Value = "value of object";
				return;
			}

            Node ip = Ip(e.Params);

            if (!ip.Contains("value"))
				throw new ArgumentException("[value] must be defined for magix.data.save to actually save anything");

            Node value = ip["value"].Clone();

			lock (typeof(DataCore))
			{
                using (IObjectContainer db = Db4oEmbedded.OpenFile(HttpContext.Current.Request.MapPath("~/" + _dbFile)))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

                    string id = ip.Contains("id") ?
                        ip["id"].Get<string>() : 
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
					db.Commit();
					db.Close();
				}
			}
		}

		/**
		 * removes an object from database
		 */
		[ActiveEvent(Name = "magix.data.remove")]
		public static void magix_data_remove(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>removes the given [id] or [prototype] object(s) from 
your persistent data storage</p><p>thread safe</p>";
                e.Params["magix.data.remove"]["id"].Value = "object-id";
                return;
			}

            Node ip = Ip(e.Params);

			Node prototype = null;
            if (ip.Contains("prototype"))
                prototype = ip["prototype"];

            if ((!ip.Contains("id") || string.IsNullOrEmpty(ip["id"].Get<string>())) && prototype == null)
				throw new ArgumentException("missing [id] or [prototype] while trying to remove object");

			lock (typeof(DataCore))
			{
                using (IObjectContainer db = Db4oEmbedded.OpenFile(HttpContext.Current.Request.MapPath("~/" + _dbFile)))
				{
                    string id = ip.Contains("id") ? ip["id"].Get<string>() : null;
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
					db.Close();
				}
			}
		}

		/**
		 * counts objects in database
		 */
		[ActiveEvent(Name = "magix.data.count")]
		public static void magix_data_count(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>returns the total number of objects in 
data storage as [count]&nbsp;&nbsp;add [prototype] to filter results</p><p>thread safe</p>";
                e.Params["magix.data.count"].Value = null;
				return;
			}

            Node ip = Ip(e.Params);

			Node prototype = null;
            if (ip.Contains("prototype"))
                prototype = ip["prototype"];

			lock (typeof(DataCore))
			{
				using (IObjectContainer db = Db4oEmbedded.OpenFile(HttpContext.Current.Request.MapPath("~/" + _dbFile)))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

                    ip["count"].Value = db.Ext().Query<Storage>(
						delegate(Storage obj)
						{
							if (prototype != null)
								return obj.Node.HasNodes(prototype);
							return true;
						}
					).Count;

					db.Close();
				}
			}
		}
	}
}

