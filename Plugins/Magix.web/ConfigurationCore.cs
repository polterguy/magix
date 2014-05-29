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
	 * configuration core
	 */
	internal sealed class ConfigurationCore : ActiveController
	{
		/*
		 * returns a web.config setting
		 */
		[ActiveEvent(Name = "magix.configuration.get-setting")]
		public static void magix_configuration_get_setting(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-setting-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-setting-sample]");
                return;
			}

			if (!ip.ContainsValue("name"))
				throw new ArgumentException("missing [name] in [magix.configuration.get-setting]");

            Node dp = Dp(e.Params);

            string val = ConfigurationManager.AppSettings[Expressions.GetExpressionValue(ip["name"].Get<string>(), dp, ip, false) as string];
			if (!string.IsNullOrEmpty(val))
                ip["value"].Value = val;
		}

        /*
         * returns the base url of the application
         */
        [ActiveEvent(Name = "magix.configuration.get-base-directory")]
        public void magix_configuration_get_base_directory(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.get-base-directory-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.get-base-directory-sample]");
                return;
            }

            ip["value"].Value = GetApplicationBaseUrl();
        }
    }
}

