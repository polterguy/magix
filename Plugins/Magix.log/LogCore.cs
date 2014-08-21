/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using log4net;
using Magix.Core;

namespace Magix.log
{
	/*
	 * contains the logging functionality of magix
	 */
	public class LogCore : ActiveController
	{
        private static readonly ILog log = LogManager.GetLogger("magix");

        /*
         * initializes log4net
         */
        [ActiveEvent(Name = "magix.core.application-startup")]
        public static void magix_core_application_startup(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.log",
                    "Magix.log.hyperlisp.inspect.hl",
                    "[magix.log.application-startup-dox].value");
                return;
            }

            log4net.Config.XmlConfigurator.Configure(new FileInfo(HttpContext.Current.Server.MapPath("~/Web.config"))); 
        }

		/*
		 * will log the given header and body
		 */
		[ActiveEvent(Name = "magix.log.append")]
		public static void magix_log_append(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.log",
                    "Magix.log.hyperlisp.inspect.hl",
                    "[magix.log.append-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.log",
                    "Magix.log.hyperlisp.inspect.hl",
                    "[magix.log.append-sample]");
                return;
			}

            if (!ip.ContainsValue("header"))
				throw new ArgumentException("no [header] given to [magix.log.append]");

            if (!ip.ContainsValue("body"))
				throw new ArgumentException("no [body] given to [magix.log.append]");

            Node dp = Dp(e.Params);

            string header = Expressions.GetFormattedExpression("header", e.Params, "");
            string body = Expressions.GetFormattedExpression("body", e.Params, "");
            bool error = ip.ContainsValue("error") && ip["error"].Get<bool>();

            if (error)
                log.Error(string.Format("{0} - {1}", header, body));
            else
                log.Debug(string.Format("{0} - {1}", header, body));
		}
	}
}

