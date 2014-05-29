/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Configuration;
using Magix.Core;

namespace Magix.web
{
	/*
	 * session core
	 */
	internal sealed class SessionCore : ActiveController
	{
		/*
		 * sets a session object
		 */
		[ActiveEvent(Name = "magix.web.set-session")]
		private void magix_web_set_session(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.set-session-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.set-session-sample]");
                return;
			}

            string id = ip.Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("[magix.web.set-session] needs a value to know which session object to fetch");

            if (!ip.Contains("value"))
			{
				// removal of existing session object
				Page.Session.Remove(id);
			}
			else
			{
				// adding or overwiting existing value
                Node value = ip["value"].Clone();
				Page.Session[id] = value;
			}
		}

        /*
         * returns an existing session object
         */
		[ActiveEvent(Name = "magix.web.get-session")]
		private void magix_web_get_session(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-session-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-session-sample]");
                return;
			}

            string id = ip.Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("need a value for [magix.web.get-session]");

			if (Page.Session[id] != null &&
			    Page.Session[id] is Node)
                ip.Add(Page.Session[id] as Node);
		}
    }
}

