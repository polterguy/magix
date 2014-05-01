/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using System.Web;
using Magix.Core;
using Magix.UX.Builder;

namespace Magix.execute
{
	/**
	 * browser core
	 */
	public class BrowserCore : ActiveController
	{
		/**
		 * scrolls the browser window
		 */
		[ActiveEvent(Name = "magix.viewport.scroll")]
		public void magix_browser_scroll(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will scroll the browser window 
such that is shows a specific element.&nbsp;&nbsp;if no element 
is given, it will scroll the browser window to the top.
&nbsp;&nbsp;not thread safe";
				e.Params["magix.viewport.scroll"].Value = "id-of-some-element";
				return;
			}

            if (!string.IsNullOrEmpty(Ip(e.Params).Get<string>()))
            {
                string id = Ip(e.Params).Get<string>();
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
    }
}

