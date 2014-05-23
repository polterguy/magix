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

namespace Magix.execute
{
	/*
	 * web helper core
	 */
	public class WebCore : ActiveController
	{
        /*
         * downloads file from web
         */
        [ActiveEvent(Name = "magix.web.get-file")]
        public static void magix_web_get_file(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-file-dox].Value");
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

            string filepath = Expressions.GetExpressionValue(ip["file"]["url"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(filepath))
                throw new ArgumentException("you need to define which file to load, as [url]");

            DownloadFile(ip, filepath);
        }

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

        /**
         * return the given http get parameter
         */
		[ActiveEvent(Name = "magix.web.get")]
		public void magix_web_get(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-sample]");
                return;
			}

            string par = ip.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.get] needs a value defining which get parameter to fetch");

			if (HttpContext.Current.Request.Params[par] != null)
                ip["value"].Value = HttpContext.Current.Request.Params[par];
		}

		/*
		 * sets a session object
		 */
		[ActiveEvent(Name = "magix.web.set-session")]
		public void magix_web_set_session(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.set-session-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.set-session-sample]");
                return;
			}

            string id = ip.Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("[magix.web.set-session] needs a value to know which session object to fetch");

            if (!ip.Contains("value"))
			{
				// removal of existing session object
				Page.Session.Remove(id);
			}
			else
			{
				// adding or overwiting existing value
                Node value = ip["value"].Clone();
				Page.Session[id] = value;
			}
		}

        /*
         * returns an existing session object
         */
		[ActiveEvent(Name = "magix.web.get-session")]
		public void magix_web_get_session(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-session-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-session-sample]");
                return;
			}

            string id = ip.Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("need a value for [magix.web.get-session]");

			if (Page.Session[id] != null &&
			    Page.Session[id] is Node)
                ip.Add(Page.Session[id] as Node);
		}

		/*
		 * redirects the client/browser
		 */
		[ActiveEvent(Name = "magix.web.redirect")]
		public void magix_web_redirect(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.redirect-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.redirect-sample]");
                return;
			}

            string url = ip.Get<string>();

			if (string.IsNullOrEmpty(url))
				throw new ArgumentException("need url as value for [magix.web.redirect] to function");

			if (url.Contains("~"))
				url = url.Replace("~", GetApplicationBaseUrl());

			Magix.UX.Manager.Instance.Redirect(url);
		}

		/*
		 * sets a cookie
		 */
		[ActiveEvent(Name = "magix.web.set-cookie")]
		public void magix_web_set_cookie(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.set-cookie-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.set-cookie-sample]");
                return;
			}

            string par = ip.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.set-cookie] needs a value to know which cookie to set");

			string value = null;

            if (ip.Contains("value"))
                value = ip["value"].Get<string>();

			if (value == null)
				HttpContext.Current.Response.Cookies.Remove(par);
			else
			{
				HttpCookie cookie = new HttpCookie(par, value);
				cookie.HttpOnly = true;
				DateTime expires = DateTime.Now.AddYears(3);
                if (ip.Contains("expires"))
                    expires = ip["expires"].Get<DateTime>();
				cookie.Expires = expires;

				HttpContext.Current.Response.SetCookie(cookie);
			}
		}

		/*
		 * returns an existing cookie
		 */
		[ActiveEvent(Name = "magix.web.get-cookie")]
		public void magix_web_get_cookie(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-cookie-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-cookie-sample]");
                return;
			}

            string par = ip.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.get-cookie] needs a value to know which cookie to fetch");

			if (HttpContext.Current.Request.Cookies.Get(par) != null)
                ip["value"].Value = HttpContext.Current.Request.Cookies[par].Value;
		}
		
		/*
		 * returns a web.config setting
		 */
		[ActiveEvent(Name = "magix.web.get-config-setting")]
		public static void magix_web_get_config_setting(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-config-setting-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.web",
                    "Magix.web.hyperlisp.inspect.hl",
                    "[magix.web.get-config-setting-sample]");
                return;
			}

            string par = ip.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which web.config setting you wish to retrieve as value to [magix.web.get-config-setting]");

			string val = ConfigurationManager.AppSettings[par];

			if (!string.IsNullOrEmpty(val))
                ip["value"].Value = val;
		}

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
            Page.Session["magix.web.postpone-execution"] = ip["code"].Clone();
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
                RaiseActiveEvent(
                    "magix.execute",
                    code);
                Page.Session.Remove("magix.web.postpone-execution");
            }
        }

        /*
         * returns the base url of the application
         */
        [ActiveEvent(Name = "magix.web.get-base-directory")]
        public void magix_get_base_directory(object sender, ActiveEventArgs e)
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

