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
	internal sealed class PostponedCore : ActiveController
	{
        /*
         * postpones execution of given hyperlisp to next http request
         */
        [ActiveEvent(Name = "magix.web.postpone-execution")]
        public void magix_web_postpone_execution(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.postpone-execution-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.postpone-execution-sample]");
                return;
            }

            if (!ip.Contains("code"))
                throw new ArgumentException("[magix.web.postpone-execution] needs a [code] block to execute");

            if (Page.Session["magix.web.postpone-execution"] == null)
                Page.Session["magix.web.postpone-execution"] = new Node();
            ((Node)Page.Session["magix.web.postpone-execution"]).Add(ip["code"].Clone());
        }

        /*
         * executes the postponed hyperlisp, if any
         */
        [ActiveEvent(Name = "magix.viewport.page-load")]
        public void magix_viewport_page_load(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.viewport.page-load-dox].Value");
                return;
            }

            if (Page.Session["magix.web.postpone-execution"] != null)
            {
                Node code = Page.Session["magix.web.postpone-execution"] as Node;
                Page.Session.Remove("magix.web.postpone-execution");
                foreach (Node idxNode in code)
                {
                    RaiseActiveEvent(
                        "magix.execute",
                        idxNode);
                }
            }
        }
    }
}

