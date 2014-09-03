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
using System.Text.RegularExpressions;
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
                    "[magix.web.get-parameter-dox].value");
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
            string name = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);

            if (HttpContext.Current.Request.Params[name] != null)
                ip["value"].Value = HttpUtility.UrlDecode(HttpContext.Current.Request.Params[name]);
        }

        /*
         * transfer a file to client
         */
        [ActiveEvent(Name = "magix.web.get-url")]
        private void magix_web_get_url(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-url-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-url-sample]");
                return;
            }

            ip["url"].Value = HttpUtility.UrlDecode(HttpContext.Current.Request.Url.AbsoluteUri.ToString().ToLower().Replace("default.aspx", ""));
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
                    "[magix.web.transfer-file-dox].value");
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
            string file = Page.Server.MapPath(Expressions.GetExpressionValue<string>(ip["file"].Get<string>(), dp, ip, false));

            FileInfo fileInfo = new FileInfo(file);

            string fileAs = fileInfo.Name;
            if (ip.ContainsValue("as"))
                fileAs = Expressions.GetExpressionValue<string>(ip["as"].Get<string>(), dp, ip, false);

            // transmitting file
            Page.Response.Filter = null; // ditching magix ux filter rendering ...
            Page.Response.ClearContent();
            Page.Response.ClearHeaders();
            Page.Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", fileAs));
            Page.Response.AddHeader("Content-Length", fileInfo.Length.ToString());
            Page.Response.ContentType = GetContentType(fileInfo.Extension.ToLower());
            Page.Response.TransmitFile(file);
            Page.Response.Flush();
            Page.Response.End();
        }

        /*
         * helper for above
         */
        private string GetContentType(string extension)
        {
            switch (extension)
            {
                case ".htm":
                case ".html":
                case ".log":
                    return "text/HTML";
                case ".txt":
                    return "text/plain";
                case ".doc":
                    return "application/ms-word";
                case ".tiff":
                case ".tif":
                    return "image/tiff";
                case ".asf":
                    return "video/x-ms-asf";
                case ".avi":
                    return "video/avi";
                case ".zip":
                    return "application/zip";
                case ".xls":
                case ".csv":
                    return "application/vnd.ms-excel";
                case ".gif":
                    return "image/gif";
                case ".jpg":
                case "jpeg":
                    return "image/jpeg";
                case ".bmp":
                    return "image/bmp";
                case ".wav":
                    return "audio/wav";
                case ".mp3":
                    return "audio/mpeg3";
                case ".mpg":
                case "mpeg":
                    return "video/mpeg";
                case ".rtf":
                    return "application/rtf";
                case ".asp":
                    return "text/asp";
                case ".pdf":
                    return "application/pdf";
                case ".fdf":
                    return "application/vnd.fdf";
                case ".ppt":
                    return "application/mspowerpoint";
                case ".dwg":
                    return "image/vnd.dwg";
                case ".msg":
                    return "application/msoutlook";
                case ".xml":
                case ".sdxl":
                    return "application/xml";
                case ".xdp":
                    return "application/vnd.adobe.xdp+xml";
                default:
                    return "application/octet-stream";
            }
        }

        /*
         * strips html from node
         */
        [ActiveEvent(Name = "magix.web.strip-html")]
        private void magix_web_strip_html(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.strip-html-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.strip-html-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string html = Expressions.GetExpressionValue<string>(ip["value"].Get<string>(), dp, ip, false);
            html = Regex.Replace(html, "<.*?>", string.Empty);

            ip["result"].Value = html;
        }
    }
}

