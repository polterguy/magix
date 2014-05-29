/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using System.Web;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.web
{
	/*
	 * browser core
	 */
	public class BrowserCore : ActiveController
	{
		/*
		 * scrolls the browser window
		 */
		[ActiveEvent(Name = "magix.browser.scroll")]
		public void magix_browser_scroll(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.browser.scroll-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.browser.scroll-sample]");
                return;
			}

            if (!string.IsNullOrEmpty(ip.Get<string>()))
            {
                string id = ip.Get<string>();
                string clientId = Magix.UX.Selector.FindControl<System.Web.UI.Control>(Page, id).ClientID;

                Node js = new Node();
                js["script"].Value = string.Format("setTimeout(function(){{MUX.$('{0}').scrollIntoView();}}, 1);",
                    clientId);

                RaiseActiveEvent(
                    "magix.viewport.execute-javascript",
                    js);
            }
            else
            {
                Node js = new Node();
                js["script"].Value = "setTimeout(function(){parent.scrollTo(0,0)}, 1);";

                RaiseActiveEvent(
                    "magix.viewport.execute-javascript",
                    js);
            }
		}

        /*
         * redirects the client/browser
         */
        [ActiveEvent(Name = "magix.browser.redirect")]
        public void magix_browser_redirect(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.browser.redirect-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.browser.redirect-sample]");
                return;
            }

            string url = ip.Get<string>();

            if (string.IsNullOrEmpty(url))
                throw new ArgumentException("need url as value for [magix.browser.redirect] to function");

            if (url.Contains("~"))
                url = url.Replace("~", GetApplicationBaseUrl());

            Magix.UX.Manager.Instance.Redirect(url);
        }
    }
}

