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

            string val = ConfigurationManager.AppSettings[Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false)];
			if (!string.IsNullOrEmpty(val))
                ip["value"].Value = val;
		}

        /*
         * returns all web.config connection strings
         */
        [ActiveEvent(Name = "magix.configuration.get-connection-strings")]
        public static void magix_configuration_get_connection_strings(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-connection-strings-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-connection-strings-sample]");
                return;
            }

            foreach (ConnectionStringSettings idx in System.Web.Configuration.WebConfigurationManager.ConnectionStrings)
            {
                ip["result"].Add(new Node("", idx.Name));
            }
        }

        /*
         * returns named web.config connection string
         */
        [ActiveEvent(Name = "magix.configuration.get-connection-string")]
        public static void magix_configuration_get_connection_string(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-connection-string-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-connection-string-sample]");
                return;
            }

            Node dp = Dp(e.Params);
            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.configuration.get-connection-string]");
            string id = Expressions.GetExpressionValue<string>(ip["id"].Get<string>(), dp, ip, false);

            ip["value"].Value = System.Web.Configuration.WebConfigurationManager.ConnectionStrings[id].ConnectionString;
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
                    "[magix.configuration.get-base-directory-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.configuration.get-base-directory-sample]");
                return;
            }

            ip["value"].Value = GetApplicationBaseUrl();
        }
    }
}

