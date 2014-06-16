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

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("no [name] given to [magix.web.get-parameter]");
            string name = Expressions.GetExpressionValue(ip["name"].Get<string>(), dp, ip, false) as string;

            if (HttpContext.Current.Request.Params[name] != null)
                ip["value"].Value = HttpContext.Current.Request.Params[name];
        }

        /*
         * transfer a file to client
         */
        [ActiveEvent(Name = "magix.web.transfer-file")]
        private void magix_web_transfer_file(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.transfer-file-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.transfer-file-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("no [file] given to [magix.web.transfer-file]");
            string file = Page.Server.MapPath(Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string);

            FileInfo fileInfo = new FileInfo(file);
            
            // transmitting file
            Page.Response.Clear();
            Page.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", file));
            Page.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Page.Response.TransmitFile(file);
            Page.Response.End();
        }
    }
}

