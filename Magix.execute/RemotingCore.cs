/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
	 */
	public class RemotingCore : ActiveController
	{
		/**
		 * Handled to make sure we map our overridden magix.execute events during
		 * app startup
		 */
		[ActiveEvent(Name = "magix.execute.remote")]
		public static void magix_execute_remote (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") &&
			    e.Params["inspect"].Get<string>("") == "")
			{
				e.Params["inspect"].Value = @"Remotely invokes an active event on the given 
""URL"". The event raised is found in ""event"".";
				e.Params["URL"].Value = "http://127.0.0.1";
				e.Params["event"].Value = "magix.namespace.foo";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains ("URL") || ip["URL"].Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.remote needs URL parameter to know which endpoint to go towards");

			if (!ip.Contains ("event") || ip["event"].Get<string>("") == string.Empty)
				throw new ArgumentException("magix.execute.remote needs event parameter to know which event to raise externally");

			string url = ip["URL"].Get<string>();
			string evt = ip["event"].Get<string>();

		
            HttpWebRequest req = WebRequest.Create(url) as System.Net.HttpWebRequest;
            req.Method = "POST";
            req.Referer = GetApplicationBaseUrl();
            req.ContentType = "application/x-www-form-urlencoded";

            using (StreamWriter writer = new StreamWriter(req.GetRequestStream()))
            {
                writer.Write("event=" + System.Web.HttpUtility.UrlEncode(evt));

				if (ip.Contains ("params") && !string.IsNullOrEmpty (ip["params"].Get<string>()))
                    writer.Write("&params=" + System.Web.HttpUtility.UrlEncode(ip["params"].ToJSONString()));
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
                            ip["params"].ReplaceChildren (tmp);
						}
                    }
                }
            }
		}
	}
}

