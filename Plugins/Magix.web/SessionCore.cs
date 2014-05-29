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

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("no [name] given to [magix.web.set-session]");
            string name = Expressions.GetExpressionValue(ip["name"].Get<string>(), dp, ip, false) as string;

            if (!ip.Contains("value"))
			{
				// removal of existing session object
				Page.Session.Remove(name);
			}
			else
			{
				// adding or overwiting existing value
                Node value = null;
                if (ip.ContainsValue("value") && ip["value"].Get<string>().StartsWith("["))
                    value = (Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as Node).Clone();
                else
                    value = ip["value"].Clone();
				Page.Session[name] = value;
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

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("no [name] given to [magix.web.get-session]");
            string name = Expressions.GetExpressionValue(ip["name"].Get<string>(), dp, ip, false) as string;

			if (Page.Session[name] != null && Page.Session[name] is Node)
                ip.Add((Page.Session[name] as Node).Clone());
		}
    }
}

