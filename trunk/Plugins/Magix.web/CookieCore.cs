/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Globalization;
using System.Configuration;
using Magix.Core;

namespace Magix.web
{
	/*
	 * cookie core
	 */
	internal sealed class CookieCore : ActiveController
	{
		/*
		 * sets a cookie
		 */
		[ActiveEvent(Name = "magix.web.set-cookie")]
		private void magix_web_set_cookie(object sender, ActiveEventArgs e)
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

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("no [name] given to [magix.web.set-cookie]");
            string name = Expressions.GetExpressionValue(ip["name"].Get<string>(), dp, ip, false) as string;

			string value = null;
            if (ip.ContainsValue("value"))
                value = Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as string;

            bool httpOnly = true;
            if (ip.ContainsValue("http-only"))
                httpOnly = bool.Parse(Expressions.GetExpressionValue(ip["http-only"].Get<string>(), dp, ip, false) as string);

            DateTime expires = DateTime.Now.AddYears(3);
            if (ip.Contains("expires"))
                expires = DateTime.ParseExact(
                        Expressions.GetExpressionValue(ip["expires"].Get<string>(), dp, ip, false) as string,
                        "yyyy.MM.dd HH:mm:ss",
                        CultureInfo.InvariantCulture);
            
            if (value == null)
				HttpContext.Current.Response.Cookies.Remove(name);
			else
			{
				HttpCookie cookie = new HttpCookie(name, value);
				cookie.HttpOnly = httpOnly;
				cookie.Expires = expires;
				HttpContext.Current.Response.SetCookie(cookie);
			}
		}

		/*
		 * returns an existing cookie
		 */
		[ActiveEvent(Name = "magix.web.get-cookie")]
		private void magix_web_get_cookie(object sender, ActiveEventArgs e)
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
    }
}

