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
	internal sealed class ApplicationCore : ActiveController
	{
		/*
		 * sets an application object
		 */
		[ActiveEvent(Name = "magix.application.set")]
		private void magix_application_set(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.application.set-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.application.set-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.application.set]");
            string id = Expressions.GetFormattedExpression("id", e.Params, "");

            if (!ip.Contains("value"))
			{
				// removal of existing application object
				HttpContext.Current.Application.Remove(id);
			}
			else
			{
				// adding or overwiting existing value
                Node value = null;
                if (ip.ContainsValue("value") && ip["value"].Get<string>().StartsWith("["))
                    value = Expressions.GetExpressionValue<Node>(ip["value"].Get<string>(), dp, ip, false).Clone();
                else
                    value = ip["value"].Clone();
				HttpContext.Current.Application[id] = value;
			}
		}

        /*
         * returns an existing application object
         */
		[ActiveEvent(Name = "magix.application.get")]
		private void magix_application_get(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.application.get-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.application.get-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.application.get]");
            string id = Expressions.GetFormattedExpression("id", e.Params, "");

            if (HttpContext.Current.Application[id] != null && HttpContext.Current.Application[id] is Node)
                ip.Add((HttpContext.Current.Application[id] as Node).Clone());
		}
    }
}

