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
		[ActiveEvent(Name = "magix.session.set")]
		private void magix_session_set(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.session.set-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.session.set-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.session.set]");
            string id = Expressions.GetFormattedExpression("id", e.Params, "");

            if (!ip.Contains("value"))
			{
				// removal of existing session object
				Page.Session.Remove(id);
			}
			else
			{
				// adding or overwiting existing value
                Node value = null;
                if (ip.ContainsValue("value") && ip["value"].Get<string>().StartsWith("["))
                    value = Expressions.GetExpressionValue<Node>(ip["value"].Get<string>(), dp, ip, false).Clone();
                else
                    value = ip["value"].Clone();
				Page.Session[id] = value;
			}
		}

        /*
         * returns an existing session object
         */
		[ActiveEvent(Name = "magix.session.get")]
		private void magix_session_get(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.session.get-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.session.get-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.session.get]");
            string id = Expressions.GetFormattedExpression("id", e.Params, "");

			if (Page.Session[id] != null && Page.Session[id] is Node)
                ip.Add((Page.Session[id] as Node).Clone());
		}
    }
}

