/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using Magix.Core;
using Magix.UX.Builder;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;

namespace Magix.execute
{
	/**
	 * Contains the logic for invoking remote events
	 */
	public class RemotingCore : ActiveController
	{
		private static string _dbFile = "store.db4o";

		public class RemoteOverrides
		{
			private string _key;
			private string _url;

			public string Key
			{
				get { return _key; }
				set { _key = value; }
			}

			public string Url
			{
				get { return _url; }
				set { _url = value; }
			}
		}

		public class OpenEvents
		{
			private string _key;

			public string Key
			{
				get { return _key; }
				set { _key = value; }
			}
		}

		/**
		 * Handled to make sure we map our overridden magix.execute events during
		 * app startup
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.core.application-startup"].Value = null;
				e.Params["inspect"].Value = @"called during startup
of application to make sure our active events, 
which are dynamically remotely overridden
are being correctly re-mapped";
				return;
			}
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext().Configure().UpdateDepth(1000);
					db.Ext().Configure().ActivationDepth(1000);

					foreach (RemoteOverrides idx in db.QueryByExample(new RemoteOverrides()))
					{
						ActiveEvents.Instance.OverrideRemotely(idx.Key, idx.Url);
					}

					foreach (OpenEvents idx in db.QueryByExample(new OpenEvents()))
					{
						ActiveEvents.Instance.MakeRemotable(idx.Key);
					}
				}
			}
		}

		/**
		 * Creates a remote override for any active event in your system
		 */
		[ActiveEvent(Name = "magix.execute.tunnel")]
		public static void magix_execute_tunnel(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"creates an external override towards the given 
[url] for the event found in value.&nbsp;&nbsp;if you pass
in a null value as a [url], or no [url] node, the override is removed.&nbsp;&nbsp;
make sure the other side has marked the active event as
remotable.&nbsp;&nbsp;once a method is 'tunneled', it will no longer be
raised locally, but every time the active event is raised internally
within your server, it will be polymorphistically raised, on your 
[url] end-point instead";
				e.Params["tunnel"].Value = "magix.namespace.foo";
				e.Params["tunnel"]["url"].Value = "http://127.0.0.1:8080";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException(
					@"magix.execute.override-remotely needs event parameter to know 
which event to raise externally");

			string url = ip.Contains("url") ? ip["url"].Get<string>() : null;
			string evt = ip.Get<string>("");

			if (url == null)
			{
				lock (typeof(Node))
				{
					using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
					{
						db.Ext().Configure().UpdateDepth(1000);
						db.Ext().Configure().ActivationDepth(1000);

						foreach (RemoteOverrides idx in db.QueryByExample(new RemoteOverrides()))
						{
							if (evt == idx.Key)
							{
								db.Delete(idx);
							}
						}
						db.Commit();
					}
					ActiveEvents.Instance.RemoveRemotable(evt);
				}
			}
			else
			{
				lock (typeof(Node))
				{
					using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
					{
						db.Ext().Configure().UpdateDepth(1000);
						db.Ext().Configure().ActivationDepth(1000);

						bool found = false;
						foreach (RemoteOverrides idx in db.QueryByExample(new RemoteOverrides()))
						{
							if (evt == idx.Key)
							{
								found = true;
								idx.Url = url;
								db.Store(idx);
								break;
							}
						}
						if (!found)
						{
							RemoteOverrides nEvt = new RemoteOverrides();
							nEvt.Key = evt;
							nEvt.Url = url;
							db.Store(nEvt);
						}
						db.Commit();
						ActiveEvents.Instance.OverrideRemotely(evt, url);
					}
				}
			}
		}

		/**
		 * Allows an event to be remotly invoked within your system
		 */
		[ActiveEvent(Name = "magix.execute.open")]
		public static void magix_execute_open(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"allows the given value active event
to be remotely invoked.&nbsp;&nbsp;this means that other servers, can
call your active event, on your server.&nbsp;&nbsp;you could 
create a server-api for web-services, by opening 
active events for being remotely invoked, and such connect
servers together, either internally as a part of your
server park, or by exposing functionality to other networks";
				e.Params["open"].Value = "magix.namespace.foo";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.open needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext().Configure().UpdateDepth(1000);
				db.Ext().Configure().ActivationDepth(1000);

				bool found = false;
				foreach (OpenEvents idx in db.QueryByExample(new OpenEvents()))
				{
					if (evt == idx.Key)
					{
						found = true;
						break;
					}
				}
				if (!found)
				{
					RemoteOverrides nEvt = new RemoteOverrides();
					nEvt.Key = evt;
					db.Store(nEvt);
				}
				db.Commit();
				ActiveEvents.Instance.MakeRemotable(evt);
			}
		}

		/**
		 * Allows an event to NOT be remotly invoked within your system
		 */
		[ActiveEvent(Name = "magix.execute.close")]
		public static void magix_execute_close(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"closes the active event found in
value, such that it no longer can be remotely 
invoked from other servers";
				e.Params["close"].Value = "magix.namespace.foo";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.open needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext().Configure().UpdateDepth(1000);
				db.Ext().Configure().ActivationDepth(1000);

				foreach (OpenEvents idx in db.QueryByExample(new OpenEvents()))
				{
					if (evt == idx.Key)
					{
						db.Delete(idx);
						break;
					}
				}
				db.Commit();
			}
			ActiveEvents.Instance.RemoveRemotable(evt);
		}

		/**
		 * Raises a new event remotely, use "url" and "event" to instruct which
		 * event you wish to raise at which url end-point. Whatever you have 
		 * as "params" parameters, will be passed into the end-point server
		 * as the parameters to the active event. "params" will also
		 * contain the return value Nodes from the server, after the 
		 * active event is finished executing.
		 */
		[ActiveEvent(Name = "magix.execute.remote")]
		public static void magix_execute_remote(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"remotely invokes the active event from
value on the given [url].&nbsp;&nbsp;this effectively works like the 
magix.execute.raise keyword, except the event will be serialized
over http, and invoked on another server, returning
transparently back to the caller, as if it was invoked locally";
				e.Params["remote"].Value = "magix.namespace.foo";
				e.Params["remote"]["url"].Value = "http://127.0.0.1:8080";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("url") || ip["url"].Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.remote needs url parameter to know which endpoint to go towards");

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.remote needs event parameter to know which event to raise externally");

			string url = ip["url"].Get<string>();
			string evt = ip.Get<string>();

			HttpWebRequest req = WebRequest.Create(url) as System.Net.HttpWebRequest;
            req.Method = "POST";
            req.Referer = GetApplicationBaseUrl();
            req.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write("event=" + System.Web.HttpUtility.UrlEncode(evt));
                writer.Write("&params=" + System.Web.HttpUtility.UrlEncode(ip.ToJSONString()));
            }

            using (HttpWebResponse resp = req.GetResponse() as HttpWebResponse)
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    if ((int)resp.StatusCode >= 200 && (int)resp.StatusCode < 300)
                    {
                        string val = reader.ReadToEnd();
                        if (!val.StartsWith("return:"))
                            throw new Exception(
                                "Something went wrong when connecting to '" +
                                url +
                                "'. Server responded with: " + val);

                        if (val.Length > 7)
						{
							Node tmp = Node.FromJSONString(val.Substring(7));
                            ip.ReplaceChildren (tmp);
							ip.Name = tmp.Name;
							ip.Value = tmp.Value;
						}
                    }
					else
						throw new ArgumentException("Couldn't find event '" + evt + "' on " + url);
                }
            }
		}
	}
}
