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
or if you supply a [prototype], it will return all objects matching your prototype</p><p>returns 
objects found as [objects], with child nodes of [objects] being the matching objects</p><p>use 
[start] and [end] to fetch a specific slice of objects, [start] defaults to 0 and [end] defaults 
to -1, which means all objects matching criteria.&nbsp;&nbsp;[start], [end] and [prototype] cannot 
be defined if [id] is given, since [id] is supposed to be unique, and will make sure only one 
object is loaded</p><p>if a [prototype] node is given, it can contain node values with % to signify 
wildcards for a match operation.&nbsp;&nbsp;both [id], [prototype], [start] and [end] can be both 
constant values, nodes or expressions pointing to an id or a prototype object, which will be used 
as the criteria to load objects</p><p>thread safe</p>";
                e.Params["magix.data.load"]["id"].Value = "object-id";
                return;
			}

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

			Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (!string.IsNullOrEmpty(ip["prototype"].Get<string>()) && ip["prototype"].Get<string>().StartsWith("["))
                {
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                }
                else
                {
                    prototype = ip["prototype"];
                }
            }

			string id = null;
            if (ip.Contains("id") && ip["id"].Value != null)
                id = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;

			int start = 0;
            if (ip.Contains("start") && ip["start"].Value != null)
            {
                if (ip["start"].Get<string>().StartsWith("["))
                    start = int.Parse(Expressions.GetExpressionValue(ip["start"].Get<string>(), dp, ip, false) as string);
                else
                    start = ip["start"].Get<int>();
            }

			int end = -1;
            if (ip.Contains("end") && ip["end"].Value != null)
            {
                if (ip["end"].Get<string>().StartsWith("["))
                    end = int.Parse(Expressions.GetExpressionValue(ip["end"].Get<string>(), dp, ip, false) as string);
                else
                    end = ip["end"].Get<int>();
            }

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
                            else if (prototype != null)
                                return obj.Node.HasNodes(prototype);
                            else
                                return true; // defaulting to true, to allow for loading of all objects, if no id or prototype is given
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
identifier will be automatically assigned to the object, and returned as [id].&nbsp;&nbsp;
if an [id] is given, and an object with that same id exists, object will be overwritten or 
updated.&nbsp;&nbsp;both the [value] and [id] can either be expressions or constant strings
/nodes</p><p>thread safe</p>";
                e.Params["magix.data.save"]["id"].Value = "object-id";
                e.Params["magix.data.save"]["value"]["some-value"].Value = "value of object";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

            if (!ip.Contains("value"))
				throw new ArgumentException("[value] must be defined for magix.data.save to actually save anything");

            Node value = null;

            if (!string.IsNullOrEmpty(ip["value"].Get<string>()) && ip["value"].Get<string>().StartsWith("["))
            {
                value = (Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as Node).Clone();
            }
            else
                value = ip["value"].Clone();

			lock (typeof(DataCore))
			{
                using (IObjectContainer db = Db4oEmbedded.OpenFile(HttpContext.Current.Request.MapPath("~/" + _dbFile)))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

                    string id = ip.Contains("id") ?
                        Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string : 
						Guid.NewGuid().ToString();
					bool found = false;
                    ip["id"].Value = id;

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
your persistent data storage</p><p>both [id] and [prototype] can be both expressions or constant values
</p><p>thread safe</p>";
                e.Params["magix.data.remove"]["id"].Value = "object-id";
                return;
			}

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

            Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (!string.IsNullOrEmpty(ip["prototype"].Get<string>()) && ip["prototype"].Get<string>().StartsWith("["))
                {
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                }
                else
                {
                    prototype = ip["prototype"];
                }
            }

            if ((!ip.Contains("id") || string.IsNullOrEmpty(ip["id"].Get<string>())) && prototype == null)
				throw new ArgumentException("missing [id] or [prototype] while trying to remove object");

			lock (typeof(DataCore))
			{
                using (IObjectContainer db = Db4oEmbedded.OpenFile(HttpContext.Current.Request.MapPath("~/" + _dbFile)))
				{
                    string id = ip.Contains("id") ? Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string : null;
					foreach (Storage idx in db.Ext().Query<Storage>(
						delegate(Storage obj)
						{
						    if (id != null)
							    return obj.Id == id;
                            else if (prototype != null)
                                return obj.Node.HasNodes(prototype);
                            else
                                return false; // defaulting to false, to not allow deletion of everything, if empty id and prototype is given
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
data storage as [count]&nbsp;&nbsp;add [prototype] to filter results</p><p>[prototype], 
if given, can be either an expression or a constant</p><p>thread safe</p>";
                e.Params["magix.data.count"].Value = null;
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

            Node prototype = null;
            if (ip.Contains("prototype"))
            {
                if (!string.IsNullOrEmpty(ip["prototype"].Get<string>()) && ip["prototype"].Get<string>().StartsWith("["))
                {
                    prototype = Expressions.GetExpressionValue(ip["prototype"].Get<string>(), dp, ip, false) as Node;
                }
                else
                {
                    prototype = ip["prototype"];
                }
            }

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

