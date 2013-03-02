/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
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
		public static void magix_web_get(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return the given
get http parameter as [value]";
				e.Params["magix.web.get"].Value = "some-get-parameter";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("You must tell me which GET parameter you wish to extract");

			e.Params["value"].Value = HttpContext.Current.Request.Params[par];
		}

		/**
		 * Returns the given Value HTTP cookie as "value"
		 */
		[ActiveEvent(Name = "magix.web.get-cookie")]
		public static void magix_web_get_cookie(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the given
http cookie parameter as [value] node";
				e.Params["magix.web.get-cookie"].Value = "some-cookie-name";
				return;
			}

			string par = e.Params.Get<string>();
			if (string.IsNullOrEmpty(par))
				throw new ArgumentException("you must tell me which cookie you wish to extract");

			e.Params["value"].Value = HttpContext.Current.Request.Cookies[par].Value;
		}

		/**
		 * Sets the given Value HTTP cookie persistent from "value"
		 */
		[ActiveEvent(Name = "magix.web.set-cookie")]
		public static void magix_web_set_cookie(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will create or overwrite
and existing http cookie.&nbsp;&nbsp;if no expiration date 
is used, a default of three years from now will be used";
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
	}
}

