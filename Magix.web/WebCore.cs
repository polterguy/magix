/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using System.Web;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.execute
{
	/**
	 * Contains helpers for 'web-stuff', such as getting GET/POST parameters, cookie handling,
	 * etc...
	 */
	public class Helper : ActiveController
	{
		/**
		 * Returns the given Value HTTP GET or POST parameter as "value"
		 */
		[ActiveEvent(Name = "magix.web.get")]
		public void magix_web_get(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return the given
get http parameter as [value].&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.get"].Value = "some-get-parameter";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("You must tell me which GET parameter you wish to extract");

			if (HttpContext.Current.Request.Params[par] != null)
				e.Params["value"].Value = HttpContext.Current.Request.Params[par];
		}

		[ActiveEvent(Name = "magix.web.set-session")]
		public void magix_web_set_session(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
an existing session variable named [key] with node hierarchy from [value].&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.set-session"]["key"].Value = "some_key";
				e.Params["magix.web.set-session"]["value"].Value = "something to store into session";
				return;
			}

			if (!e.Params.Contains("key"))
				throw new ArgumentException("need [key]");

			if (!e.Params.Contains("value"))
				Page.Session.Remove(e.Params["key"].Get<string>());
			else
			{
				Node value = e.Params["value"].Clone();
				Page.Session[e.Params["key"].Get<string>()] = value;
			}
		}

		[ActiveEvent(Name = "magix.web.get-session")]
		public void magix_web_get_session(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return
an existing session variable named [key] as node hierarchy into [value].&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.get-session"]["key"].Value = "some_key";
				return;
			}

			if (!e.Params.Contains("key"))
				throw new ArgumentException("need [key]");

			if (Page.Session[e.Params["key"].Get<string>()] != null &&
			    Page.Session[e.Params["key"].Get<string>()] is Node)
				e.Params.Add(Page.Session[e.Params["key"].Get<string>()] as Node);
		}


		[ActiveEvent(Name = "magix.web.redirect")]
		public void magix_web_redirect(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will redirect the browser 
to the value of the [url].&nbsp;&nbsp;
not thread safe";
				e.Params["magix.web.redirect"]["url"].Value = "http://google.com";
				return;
			}

			if (!e.Params.Contains("url"))
				throw new ArgumentException("need [url]");

			string url = e.Params["url"].Get<string>();

			if (url.StartsWith("~"))
			{
				url = url.Replace("~", GetApplicationBaseUrl());
			}

			Magix.UX.AjaxManager.Instance.Redirect(url);
		}

		/**
		 * Sets the given Value HTTP cookie persistent from "value"
		 */
		[ActiveEvent(Name = "magix.web.set-cookie")]
		public void magix_web_set_cookie(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
and existing http cookie.&nbsp;&nbsp;if no expiration date 
is used, a default of three years from now will be used.&nbsp;&nbsp;not thread safe";
				e.Params["magix.web.set-cookie"].Value = "some-cookie-name";
				e.Params["magix.web.set-cookie"]["value"].Value = "something to store into cookie";
				e.Params["magix.web.set-cookie"]["expires"].Value = DateTime.Now.AddYears (3);
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which cookie you wish to set");

			string value = null;

			if (e.Params.Contains("value"))
				value = e.Params["value"].Get<string>();

			if (value == null)
			{
				HttpContext.Current.Response.Cookies.Remove(par);
			}
			else
			{
				HttpCookie cookie = new HttpCookie(par, value);
				cookie.HttpOnly = true;
				DateTime expires = DateTime.Now.AddYears(3);
				if (e.Params.Contains("expires"))
					expires = e.Params["expires"].Get<DateTime>();
				cookie.Expires = expires;

				HttpContext.Current.Response.SetCookie(cookie);
			}
		}

		/**
		 * Returns the given Value HTTP cookie as "value"
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

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which cookie you wish to extract");

			if (HttpContext.Current.Request.Cookies.Get(par) != null)
				e.Params["value"].Value = HttpContext.Current.Request.Cookies[par].Value;
		}
		
		/**
		 * Returns the given Value HTTP cookie as "value"
		 */
		[ActiveEvent(Name = "magix.web.get-config-setting")]
		public static void magix_web_get_config_setting(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the given
web.config setting as [value] node.&nbsp;&nbsp;thread safe";
				e.Params["magix.web.get-config-setting"].Value = "some-cookie-name";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which setting you wish to retrieve");

			string val = ConfigurationManager.AppSettings[par];

			if (!string.IsNullOrEmpty(val))
				e.Params["value"].Value = val;
		}
	}
}

