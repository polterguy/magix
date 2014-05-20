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
	 * remoting hyperlisp support
	 */
	public class RemotingCore : ActiveController
	{
		/**
		 * remoted hyperlisp support
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"<p>called during startup
of application to make sure our active events, which are dynamically remotely overridden
are being correctly re-mapped</p>";
				return;
			}
            RemapTunneledEvents();
            RemapOpenEvents();
		}

        private static void RemapOpenEvents()
        {
            Node tmp = new Node();
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

        private static void RemapTunneledEvents()
        {
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
        }

		/**
		 * tunnel hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.tunnel")]
		public static void magix_execute_tunnel(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>creates an external override towards the given 
[url] for the event found in value.&nbsp;&nbsp;if you pass in a null value as a [url], or no [url] 
node, the tunneling is removed</p><p>make sure the other side has marked the active event as 
remotable.&nbsp;&nbsp;once a method is 'tunneled', it will no longer be raised locally, but every 
time the active event is raised internally within your server, it will be polymorphistically 
raised, on your [url] end-point server instead</p></p>if a [persist] parameter exists, and it has 
a value of false, then the tunnel will not be serialized into the data layer, which is useful for 
active events which anyway will be re-mapped when the application pool is restarted<p><p>thread 
safe</p>";
				ip["tunnel"].Value = "magix.namespace.foo";
				ip["tunnel"]["url"].Value = "http://127.0.0.1:8080";
				return;
			}

            string activeEvent = ip.Get<string>();
			if (string.IsNullOrEmpty(activeEvent))
				throw new ArgumentException(
					@"[tunnel] needs value, being active event name, to know which event to override to go externally");

			string url = ip.Contains("url") ? ip["url"].Get<string>() : null;

			if (string.IsNullOrEmpty(url))
			{
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    Node removeNode = new Node();

                    removeNode["prototype"]["event"].Value = activeEvent;
                    removeNode["prototype"]["type"].Value = "magix.execute.tunnel";

                    RaiseActiveEvent(
                        "magix.data.remove",
                        removeNode);
                }

				ActiveEvents.Instance.RemoveRemoteOverride(activeEvent);
			}
			else
			{
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    Node removeNode = new Node();

                    removeNode["prototype"]["event"].Value = activeEvent;
                    removeNode["prototype"]["type"].Value = "magix.execute.tunnel";

                    RaiseActiveEvent(
                        "magix.data.remove",
                        removeNode);

                    Node saveNode = new Node();

                    saveNode["id"].Value = Guid.NewGuid();
                    saveNode["value"]["event"].Value = activeEvent;
                    saveNode["value"]["type"].Value = "magix.execute.tunnel";
                    saveNode["value"]["url"].Value = url;

                    RaiseActiveEvent(
                        "magix.data.save",
                        saveNode);
                }
				ActiveEvents.Instance.OverrideRemotely(activeEvent, url);
			}
		}

		/**
		 * open hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.open")]
		public static void magix_execute_open(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
				ip["inspect"].Value = @"<p>allows the given value active event
to be remotely invoked.&nbsp;&nbsp;this means that other servers, can call your active 
event, on your server</p><p>you can create a server-api for web-services, by opening 
active events for being remotely invoked, and such connect servers together, either 
internally as a part of your server park, or by exposing functionality to other networks
</p><p>if you set [persist] to false, then the active event will not be serialized into 
the data storage, meaning it will only last as long as the application is not restarted.
&nbsp;&nbsp;this is useful for active events whom are created for instance during the 
startup of your application, since it will save time, since they will anyway be overwritten 
the next time your application restarts</p><p>thread safe</p>";
				ip["open"].Value = "magix.namespace.foo";
				return;
			}

            string activeEvent = ip.Get<string>();
            if (string.IsNullOrEmpty(activeEvent))
				throw new ArgumentException("[open] needs event parameter to know which event to raise externally");

            if (!ip.Contains("persist") || ip["persist"].Get<bool>())
            {
                Node removeNode = new Node();

                removeNode["prototype"]["event"].Value = activeEvent;
                removeNode["prototype"]["type"].Value = "magix.execute.open";

                RaiseActiveEvent(
                    "magix.data.remove",
                    removeNode);

                Node saveNode = new Node();

                saveNode["id"].Value = Guid.NewGuid();
                saveNode["value"]["event"].Value = activeEvent;
                saveNode["value"]["type"].Value = "magix.execute.open";

                RaiseActiveEvent(
                    "magix.data.save",
                    saveNode);
            }
			ActiveEvents.Instance.MakeRemotable(activeEvent);
		}

		/**
		 * close hyperlisp support
		 */
		[ActiveEvent(Name = "magix.execute.close")]
		public static void magix_execute_close(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
				ip["inspect"].Value = @"<p>closes the active event found in
the value of [clode], such that it no longer can be remotely invoked from other 
servers</p><p>if you set [persist] to false, then the active event will not be 
serialized into the data storage, meaning it will only last as long as the application 
is not restarted.&nbsp;&nbsp;this is useful for active events whom are created for 
instance during the startup of your application, since it will save time, since they 
will anyway be overwritten the next time your application restarts</p><p>thread safe
</p>";
				ip["close"].Value = "magix.namespace.foo";
				return;
			}

            string activeEvent = ip.Get<string>();

            if (string.IsNullOrEmpty(activeEvent))
				throw new ArgumentException("[close] needs a value, pointing to an active event, to know which event to raise externally");

            if (!ip.Contains("persist") || ip["persist"].Get<bool>())
            {
                Node removeNode = new Node();

                removeNode["prototype"]["event"].Value = activeEvent;
                removeNode["prototype"]["type"].Value = "magix.execute.open";

                RaiseActiveEvent(
                    "magix.data.remove",
                    removeNode);
            }

			ActiveEvents.Instance.RemoveRemotable(activeEvent);
		}

		/**
		 * remotely invokes an event
		 */
		[ActiveEvent(Name = "magix.execute.remote")]
		public static void magix_execute_remote(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
				ip["inspect"].Value = @"<p>remotely invokes the active event from
value on [remote] on the given [url], passing in all nodes in [pars] as parameters to 
your active event, returning any return values from event beneath [params]</p><p>this 
effectively raises an active event, except the event will be serialized over http, and 
invoked on another server, returning transparently back to the caller, as if it was 
invoked locally</p><p>thread safe</p>";
				ip["remote"].Value = "magix.namespace.foo";
                ip["remote"]["url"].Value = "http://127.0.0.1:8080";
                ip["remote"]["params"]["your-parameters-goes-here"].Value = "value of parameter";
                return;
			}

            string activeEvent = ip.Get<string>();
            if (string.IsNullOrEmpty(activeEvent))
                throw new ArgumentException("[remote] needs an event parameter to know which event to raise externally");

            if (!ip.Contains("url") || string.IsNullOrEmpty(ip["url"].Get<string>()))
				throw new ArgumentException("[remote] needs a url parameter to know which endpoint to go towards");
            string url = ip["url"].Get<string>();

            RemotelyInvokeActiveEvent(ip, activeEvent, url);
		}

        /*
         * helper for above
         */
        private static void RemotelyInvokeActiveEvent(Node ip, string activeEvent, string url)
        {
            HttpWebRequest req = WebRequest.Create(url) as System.Net.HttpWebRequest;
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write("event=" + System.Web.HttpUtility.UrlEncode(activeEvent));
                if (ip.Contains("params"))
                {
                    Node tmp = new Node(ip.Name, ip.Value);
                    tmp.AddRange(ip["params"]);
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
                            ip["params"].Clear();
                            ip["params"].AddRange(tmp);
                        }
                    }
                    else
                        throw new ArgumentException("couldn't find event '" + activeEvent + "' on " + url);
                }
            }
        }
	}
}

