/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * remoting hyperlisp support
	 */
	public class RemotingCore : ActiveController
	{
		/*
		 * remote hyperlisp support
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.app-startup-dox].value");
                return;
			}
            RemapTunneledEvents();
            RemapOpenEvents();
		}

        /*
         * remaps stored open active events
         */
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
                    ActiveEvents.Instance.MakeRemotable(idx["value"]["event"].Get<string>());
                }
            }
        }

        /*
         * remaps stored tunneled active events
         */
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
                    string activeEvent = idx["value"]["event"].Get<string>();
                    ActiveEvents.Instance.OverrideRemotely(activeEvent, idx["value"]["url"].Get<string>());
                    if (!ActiveEvents.Instance.IsOverride(activeEvent))
                    {
                        // to make sure the local system actually has an active event with that name
                        ActiveEvents.Instance.CreateEventMapping(
                            activeEvent,
                            "magix.execute._mumbo-jumbo");
                    }
                }
            }
        }

		/*
		 * tunnel hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.tunnel")]
		public static void magix_execute_tunnel(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.tunnel-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.tunnel-sample]");
                return;
			}

            if (!ip.ContainsValue("name"))
                throw new ArgumentException(
                    @"[tunnel] needs [name], being active event name, to know which event to override to go externally");

            Node dp = Dp(e.Params);

            string activeEvent = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);
			string url = ip.Contains("url") ? Expressions.GetExpressionValue<string>(ip["url"].Get<string>(), dp, ip, false) : null;

			if (string.IsNullOrEmpty(url))
			{
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                    DataBaseRemoval.Remove(activeEvent, "magix.execute.tunnel", e.Params);

				ActiveEvents.Instance.RemoveRemoteOverride(activeEvent);

                string originalName = "";
                string origActiveEvent = ActiveEvents.Instance.GetEventMappingValue(activeEvent, ref originalName);
                if (origActiveEvent == "magix.execute._mumbo-jumbo")
                {
                    ActiveEvents.Instance.RemoveMapping(activeEvent);
                }
			}
			else
			{
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    DataBaseRemoval.Remove(activeEvent, "magix.execute.tunnel", e.Params);

                    Node saveNode = new Node("magix.data.save");
                    saveNode["id"].Value = Guid.NewGuid();
                    saveNode["value"]["event"].Value = activeEvent;
                    saveNode["value"]["type"].Value = "magix.execute.tunnel";
                    saveNode["value"]["url"].Value = url;
                    BypassExecuteActiveEvent(saveNode, e.Params);
                }
				ActiveEvents.Instance.OverrideRemotely(activeEvent, url);
                if (!ActiveEvents.Instance.IsOverride(activeEvent))
                {
                    // to make sure the local system actually has an active event with that name
                    ActiveEvents.Instance.CreateEventMapping(
                        activeEvent,
                        "magix.execute._mumbo-jumbo");
                }
			}
		}

		/*
		 * open hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.open")]
		public static void magix_execute_open(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.open-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.open-sample]");
                return;
			}

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("[open] needs a [name] parameter");

            Node dp = Dp(e.Params);
            string activeEvent = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);

            if (!ip.Contains("persist") || ip["persist"].Get<bool>())
            {
                DataBaseRemoval.Remove(activeEvent, "magix.execute.open", e.Params);

                Node saveNode = new Node("magix.data.save");
                saveNode["id"].Value = Guid.NewGuid();
                saveNode["value"]["event"].Value = activeEvent;
                saveNode["value"]["type"].Value = "magix.execute.open";

                BypassExecuteActiveEvent(saveNode, e.Params);
            }
			ActiveEvents.Instance.MakeRemotable(activeEvent);
		}

		/*
		 * close hyperlisp support
		 */
		[ActiveEvent(Name = "magix.execute.close")]
		public static void magix_execute_close(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.close-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.close-sample]");
                return;
			}

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("[close] needs a [name]");

            Node dp = Dp(e.Params);
            string activeEvent = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);

            if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                DataBaseRemoval.Remove(activeEvent, "magix.execute.open", e.Params);

			ActiveEvents.Instance.RemoveRemotable(activeEvent);
		}

		/*
		 * remotely invokes an event
		 */
		[ActiveEvent(Name = "magix.execute.remote")]
		public static void magix_execute_remote(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.remote-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.remote-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("[remote] needs a [name]");
            string activeEvent = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);

            if (!ip.Contains("url") || string.IsNullOrEmpty(ip["url"].Get<string>()))
				throw new ArgumentException("[remote] needs a [url]");
            string url = Expressions.GetExpressionValue<string>(ip["url"].Get<string>(), dp, ip, false);

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
                                "something went wrong when connecting to '" +
                                url +
                                "'.&nbsp;&nbsp;server responded with: " + val);

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

