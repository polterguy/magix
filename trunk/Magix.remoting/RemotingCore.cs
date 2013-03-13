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

namespace Magix.execute
{
	/**
	 * Contains the logic for invoking remote events
	 */
	public class RemotingCore : ActiveController
	{
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
of application to make sure our active events, which are dynamically remotely overridden
are being correctly re-mapped";
				return;
			}

			Node tmp = new Node();
			tmp["prototype"]["type"].Value = "magix.execute.tunneled";

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

			tmp = new Node();
			tmp["prototype"]["type"].Value = "magix.execute.opened";

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
		 * Creates a remote override for any active event in your system
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

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException(
					@"magix.execute.tunnel needs value, being active event name, to know 
which event to override to go externally.&nbsp;&nbsp;tunnel cannot override null event handler");

			string url = ip.Contains("url") ? ip["url"].Get<string>() : null;
			string evt = ip.Get<string>();

			if (url == null)
			{
				// removing event
				Node n = new Node();

				n["prototype"]["event"].Value = evt;
				n["prototype"]["type"].Value = "magix.execute.tunneled";

				RaiseActiveEvent(
					"magix.data.remove",
					n);
			}
			else
			{
				// adding event
				Node n = new Node();

				n["id"].Value = Guid.NewGuid();
				n["object"]["event"].Value = evt;
				n["object"]["type"].Value = "magix.execute.tunneled";

				RaiseActiveEvent(
					"magix.data.save",
					n);

				ActiveEvents.Instance.OverrideRemotely(evt, url);
			}
		}

		/**
		 * Allows an event to be remotly invoked within your system
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

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.open needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			// adding event
			Node n = new Node();

			n["id"].Value = Guid.NewGuid();
			n["object"]["event"].Value = evt;
			n["object"]["type"].Value = "magix.execute.opened";

			RaiseActiveEvent(
				"magix.data.save",
				n);

			ActiveEvents.Instance.MakeRemotable(evt);
		}

		/**
		 * Allows an event to NOT be remotly invoked within your system
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

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (ip.Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.open needs event parameter to know which event to raise externally");

			string evt = ip.Get<string>();

			// adding event
			Node n = new Node();

			n["prototype"]["event"].Value = evt;
			n["prototype"]["type"].Value = "magix.execute.opened";

			RaiseActiveEvent(
				"magix.data.remove",
				n);

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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"remotely invokes the active event from
value on the given [url].&nbsp;&nbsp;this effectively works like the 
magix.execute.raise keyword, except the event will be serialized
over http, and invoked on another server, returning
transparently back to the caller, as if it was invoked locally.&nbsp;&nbsp;thread safe";
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

