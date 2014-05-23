/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
	/*
	 * data storage
	 */
	public class DataCore : ActiveController
	{
		private static string _dbFile = "database/data-storage.db4o";

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
                        {
                            ip["objects"][idx.Id].AddRange(idx.Node.Clone());
                        }
						idxNo++;
					}
					db.Close();
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
                e.Params["inspect"].Value = @"";
                e.Params["magix.data.count-sample"].Value = null;
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

