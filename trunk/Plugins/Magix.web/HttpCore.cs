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
	 * web helper core
	 */
	internal sealed class HttpCore : ActiveController
	{
        /*
         * return the given http get parameter
         */
        [ActiveEvent(Name = "magix.web.get-parameter")]
		private void magix_web_get_parameter(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-parameter-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-parameter-sample]");
                return;
			}

            string par = ip.Get<string>();
			if (string.IsNullOrEmpty(par))
                throw new ArgumentException("[magix.web.get-parameter] needs a value defining which get parameter to fetch");

			if (HttpContext.Current.Request.Params[par] != null)
                ip["value"].Value = HttpContext.Current.Request.Params[par];
		}
    }
}

