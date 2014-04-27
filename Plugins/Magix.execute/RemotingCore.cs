/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * remoting hyper lisp support
	 */
	public class RemotingCore : ActiveController
	{
		/**
		 * remoted hyper lisp support
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.core.application-startup"].Value = null;
				e.Params["inspect"].Value = @"called during startup
of application to make sure our active events, which are dynamically remotely overridden
are being correctly re-mapped";
				return;
			}

			// first tunneled events
			Node tmp = new Node();
			tmp["prototype"]["type"].Value = "magix.execute.tunnel";

			RaiseActiveEvent(
				"magix.data.load",
				tmp);

			if (tmp.Contains("objects"))
			{
				foreach (Node idx in tmp["objects"])
				{
					ActiveEvents.Instance.OverrideRemotely(idx.Name, idx["url"].Get<string>());
				}
			}

			// then open events
			tmp = new Node();
			tmp["prototype"]["type"].Value = "magix.execute.open";

			RaiseActiveEvent(
				"magix.data.load",
				tmp);

			if (tmp.Contains("objects"))
			{
				foreach (Node idx in tmp["objects"])
				{
					ActiveEvents.Instance.MakeRemotable(idx.Name);
				}
			}
		}

		/**
		 * tunnel hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.tunnel")]
		public static void magix_execute_tunnel(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"creates an external override towards the given 
[url] for the event found in value.&nbsp;&nbsp;if you pass
in a null value as a [url], or no [url] node, the override is removed.&nbsp;&nbsp;
make sure the other side has marked the active event as
remotable.&nbsp;&nbsp;once a method is 'tunneled', it will no longer be
raised locally, but every time the active event is raised internally
within your server, it will be polymorphistically raised, on your 
[url] end-point server instead.&nbsp;&nbsp;thread safe";
				e.Params["tunnel"].Value = "magix.namespace.foo";
				e.Params["tunnel"]["url"].Value = "http://127.0.0.1:8080";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.tunnel] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			if (string.IsNullOrEmpty(ip.Get<string>()))
				throw new ArgumentException(
					@"[magix.execute.tunnel] needs value, being active event name, to know 
which event to override to go externally.&nbsp;&nbsp;tunnel cannot override null event handler");

			string url = ip.Contains("url") ? ip["url"].Get<string>() : null;
			string evt = ip.Get<string>();

			if (string.IsNullOrEmpty(url))
			{
				// removing event
				Node n = new Node();

				n["prototype"]["event"].Value = evt;
				n["prototype"]["type"].Value = "magix.execute.tunnel";

				RaiseActiveEvent(
					"magix.data.remove",
					n);

				ActiveEvents.Instance.RemoveRemoteOverride(evt);
			}
			else
			{
				// adding event
				Node n = new Node();

				n["id"].Value = Guid.NewGuid();
				n["value"]["event"].Value = evt;
				n["value"]["type"].Value = "magix.execute.tunnel";

				RaiseActiveEvent(
					"magix.data.save",
					n);

				ActiveEvents.Instance.OverrideRemotely(evt, url);
			}
		}

		/**
		 * open hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.open")]
		public static void magix_execute_open(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"allows the given value active event
to be remotely invoked.&nbsp;&nbsp;this means that other servers, can
call your active event, on your server.&nbsp;&nbsp;you could 
create a server-api for web-services, by opening 
active events for being remotely invoked, and such connect
servers together, either internally as a part of your
server park, or by exposing functionality to other networks.&nbsp;&nbsp;thread safe";
				e.Params["open"].Value = "magix.namespace.foo";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.tunnel] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			if (string.IsNullOrEmpty(ip.Get<string>()))
				throw new ArgumentException("magix.execute.open needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			// adding event
			Node n = new Node();

			n["id"].Value = Guid.NewGuid();
			n["value"]["event"].Value = evt;
			n["value"]["type"].Value = "magix.execute.open";

			RaiseActiveEvent(
				"magix.data.save",
				n);

			ActiveEvents.Instance.MakeRemotable(evt);
		}

		/**
		 * close hyper lisp support
		 */
		[ActiveEvent(Name = "magix.execute.close")]
		public static void magix_execute_close(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"closes the active event found in
value, such that it no longer can be remotely invoked from other servers.&nbsp;&nbsp;thread safe";
				e.Params["close"].Value = "magix.namespace.foo";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.tunnel] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			if (string.IsNullOrEmpty(ip.Get<string>()))
				throw new ArgumentException("magix.execute.open needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			// adding event
			Node n = new Node();

			n["prototype"]["event"].Value = evt;
			n["prototype"]["type"].Value = "magix.execute.open";

			RaiseActiveEvent(
				"magix.data.remove",
				n);

			ActiveEvents.Instance.RemoveRemotable(evt);
		}

		/**
		 * remotely invokes an event
		 */
		[ActiveEvent(Name = "magix.execute.remote")]
		public static void magix_execute_remote(object sender, ActiveEventArgs e)
		{
			// TODO: is this event necessary since we've got tunnel?
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"remotely invokes the active event from
value on [remote] on the given [url], passing in all
nodes in [pars] as parameters to your active event, returning
any return values from event beneath [params].&nbsp;&nbsp;
this effectively works like the magix.execute.raise keyword, 
except the event will be serialized over http, and invoked on 
another server, returning transparently back to the caller, 
as if it was invoked locally.&nbsp;&nbsp;thread safe";
				e.Params["remote"].Value = "magix.namespace.foo";
                e.Params["remote"]["url"].Value = "http://127.0.0.1:8080";
                e.Params["remote"]["params"]["your-parameters-goes-here"].Value = "http://127.0.0.1:8080";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.tunnel] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("url") || string.IsNullOrEmpty(ip["url"].Get<string>()))
				throw new ArgumentException("magix.execute.remote needs url parameter to know which endpoint to go towards");

			if (string.IsNullOrEmpty(ip.Get<string>()))
				throw new ArgumentException("magix.execute.remote needs event parameter to know which event to raise externally");

			string url = ip["url"].Get<string>();
			string evt = ip.Get<string>();

			HttpWebRequest req = WebRequest.Create(url) as System.Net.HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write("event=" + System.Web.HttpUtility.UrlEncode(evt));
                if (ip.Contains("params"))
                {
                    Node tmp = new Node(ip.Name, ip.Value);
                    tmp.ReplaceChildren(ip["params"]);
                    writer.Write("&params=" + System.Web.HttpUtility.UrlEncode(tmp.ToJSONString()));
                }
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
                            ip["params"].ReplaceChildren(tmp);
						}
                    }
					else
						throw new ArgumentException("Couldn't find event '" + evt + "' on " + url);
                }
            }
		}
	}
}
