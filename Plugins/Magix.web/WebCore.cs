/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Configuration;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.execute
{
	/**
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
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>plugin for loading web documents as 
files </p><p>this is useful for using as a plugin loader for the [magix.file.load] 
active event</p><p>supported protocols are all protocols supported by the WebRequest 
class in asp.net, and should at least be capable of handling https, http and ftp</p>
<p>thread safe</p>";
                e.Params["magix.file.load-from-web"]["url"].Value = "http://google.com";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("url"))
                throw new ArgumentException("you need to supply which file to load as the [url] parameter");

            string filepath = Expressions.GetExpressionValue(ip["url"].Get<string>(), dp, ip, false) as string;
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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return the given
get http parameter as [value], if existing.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.get"].Value = "some-get-parameter";
				return;
			}

            string par = Ip(e.Params).Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.get] needs a value defining which get parameter to fetch");

			if (HttpContext.Current.Request.Params[par] != null)
                Ip(e.Params)["value"].Value = HttpContext.Current.Request.Params[par];
		}

		/**
		 * sets a session object
		 */
		[ActiveEvent(Name = "magix.web.set-session")]
		public void magix_web_set_session(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
an existing session variable with name of value of [magix.web.set-session] with node 
hierarchy from [value].&nbsp;&nbsp;
if you pass in no [value], any existing session object with given key will be removed.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.set-session"].Value = "some_key";
				e.Params["magix.web.set-session"]["value"].Value = "something to store into session";
				return;
			}

            string id = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("[magix.web.set-session] needs a value to know which session object to fetch");

            if (!Ip(e.Params).Contains("value"))
			{
				// removal of existing session object
				Page.Session.Remove(id);
			}
			else
			{
				// adding or overwiting existing value
                Node value = Ip(e.Params)["value"].Clone();
				Page.Session[id] = value;
			}
		}

        /**
         * postpones execution of hyper lisp til next page load of site
         */
        /**
         * returns an existing session object
         */
		[ActiveEvent(Name = "magix.web.get-session")]
		public void magix_web_get_session(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return
an existing session variable named through the value of [magix.web.get-session] 
as node hierarchy as child nodes, if existing.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.get-session"].Value = "some_key";
				return;
			}

            string id = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("need a value for [magix.web.get-session]");

			if (Page.Session[id] != null &&
			    Page.Session[id] is Node)
                Ip(e.Params).Add(Page.Session[id] as Node);
		}

		/**
		 * redirects the client/browser
		 */
		[ActiveEvent(Name = "magix.web.redirect")]
		public void magix_web_redirect(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will redirect the browser 
to the value of [magix.web.redirect] node.&nbsp;&nbsp;
you can use the ~ to reference the base url of the website, and in 
such a way create relative redirecting urls.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.redirect"].Value = "http://google.com";
				return;
			}

            string url = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(url))
				throw new ArgumentException("need url as value for [magix.web.redirect] to function");

			if (url.Contains("~"))
			{
				url = url.Replace("~", GetApplicationBaseUrl());
			}

			Magix.UX.Manager.Instance.Redirect(url);
		}

		/**
		 * sets a cookie
		 */
		[ActiveEvent(Name = "magix.web.set-cookie")]
		public void magix_web_set_cookie(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
and existing http cookie.&nbsp;&nbsp;if no expiration date 
is used, a default of three years from now will be used.&nbsp;&nbsp;
if no [value] is passed in, any existing cookies with given name will 
be removed.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.set-cookie"].Value = "some-cookie-name";
				e.Params["magix.web.set-cookie"]["value"].Value = "something to store into cookie";
				e.Params["magix.web.set-cookie"]["expires"].Value = DateTime.Now.AddYears (3);
				return;
			}

            string par = Ip(e.Params).Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.set-cookie] needs a value to know which cookie to set");

			string value = null;

            if (Ip(e.Params).Contains("value"))
                value = Ip(e.Params)["value"].Get<string>();

			if (value == null)
			{
				HttpContext.Current.Response.Cookies.Remove(par);
			}
			else
			{
				HttpCookie cookie = new HttpCookie(par, value);
				cookie.HttpOnly = true;
				DateTime expires = DateTime.Now.AddYears(3);
                if (Ip(e.Params).Contains("expires"))
                    expires = Ip(e.Params)["expires"].Get<DateTime>();
				cookie.Expires = expires;

				HttpContext.Current.Response.SetCookie(cookie);
			}
		}

		/**
		 * returns an existing cookie
		 */
		[ActiveEvent(Name = "magix.web.get-cookie")]
		public void magix_web_get_cookie(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the given
http cookie parameter as [value] node.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.get-cookie"].Value = "some-cookie-name";
				return;
			}

            string par = Ip(e.Params).Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("[magix.web.get-cookie] nneds a value to know which cookie to fetch");

			if (HttpContext.Current.Request.Cookies.Get(par) != null)
                Ip(e.Params)["value"].Value = HttpContext.Current.Request.Cookies[par].Value;
		}
		
		/**
		 * returns a web.config setting
		 */
		[ActiveEvent(Name = "magix.web.get-config-setting")]
		public static void magix_web_get_config_setting(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the given
web.config setting as [value] node.&nbsp;&nbsp;thread safe";
				e.Params["magix.web.get-config-setting"].Value = "some-setting-name";
				return;
			}

            string par = Ip(e.Params).Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which web.config setting you wish to retrieve as value to [magix.web.get-config-setting]");

			string val = ConfigurationManager.AppSettings[par];

			if (!string.IsNullOrEmpty(val))
                Ip(e.Params)["value"].Value = val;
		}

        [ActiveEvent(Name = "magix.web.postpone-execution")]
        public void magix_web_postpone_execution(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.execute"].Value = null;
                e.Params["inspect"].Value = @"will store a block 
of hyper lisp from the [core] parameter, which it will execute upon the next page 
load of the site for the given session.&nbsp;&nbsp;not thread safe";
                e.Params["magix.web.postpone-execution"]["code"]["magix.viewport.show-message"].Value = 
                    "will not be shown before page is loaded initially over again";
                return;
            }

            if (!Ip(e.Params).Contains("code"))
                throw new ArgumentException("[magix.web.postpone-execution] needs a [code] block to execute");

            // adding or overwiting existing value
            Page.Session["magix.web.postpone-execution"] = Ip(e.Params)["code"].Clone();
        }

        /**
         * executes the postponed hyper lisp, if any
         */
        [ActiveEvent(Name = "magix.viewport.page-load")]
        public void magix_viewport_page_load(object sender, ActiveEventArgs e)
        {
            if (ShouldInspectOrHasInspected(e.Params))
            {
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

        /**
         * returns the base url of the application
         */
        [ActiveEvent(Name = "magix.web.get-base-directory")]
        public void magix_get_base_directory(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.execute"].Value = null;
                e.Params["inspect"].Value = @"returns the base directory of the web application.
&nbsp;&nbsp;thread safe";
                e.Params["magix.web.get-base-directory"].Value = null;
                return;
            }

            e.Params["value"].Value = GetApplicationBaseUrl();
        }
    }
}

