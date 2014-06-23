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
	 * plugin for loading files from web
	 */
	internal sealed class FilePluginCore : ActiveController
	{
        /*
         * downloads file from web
         */
        [ActiveEvent(Name = "magix.web.get-file")]
        private static void magix_web_get_file(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-file-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-file-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("you need to supply a [file] parameter");

            if (!ip["file"].ContainsValue("url"))
                throw new ArgumentException("you need to supply which file to load as the [url] parameter");

            string filepath = Expressions.GetExpressionValue<string>(ip["file"]["url"].Get<string>(), dp, ip, false);
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentException("you need to define which file to load, as [url]");

            DownloadFile(ip, filepath);
        }

        /*
         * helper for above
         */
        private static void DownloadFile(Node ip, string filepath)
        {
            WebRequest request = WebRequest.Create(filepath) as HttpWebRequest;
            using (WebResponse response = request.GetResponse())
            {
                using (TextReader reader = new StreamReader(response.GetResponseStream()))
                {
                    ip["value"].Value = reader.ReadToEnd();
                }
            }
        }
    }
}

