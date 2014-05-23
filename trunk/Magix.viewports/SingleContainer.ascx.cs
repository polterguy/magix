/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.viewports
{
    /*
     * Sample Viewport, nothing fancy, three empty containers
     */
    public class SingleContainer : Viewport
    {
        protected DynamicPanel content;
        protected Label messageSmall;
		private bool _isFirst = true;

        protected override string GetDefaultContainer()
        {
            return "content";
        }

		/*
         * Shows a Message Box to the end user, which can be configured
         */
		[ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"<p>shows a message box to the 
end user for some seconds.&nbsp;&nbsp;all arguments can either be a constant 
or an expression.&nbsp;&nbsp;[time] is optional number of milliseconds to 
display the message box.&nbsp;&nbsp;[color] is optional background-color to 
use for the message box</p><p>if you create multiple message boxes within the 
same request, then all messages will show, but only the first invocation will 
decide the other parameters, such as how long time to show the message box, 
what background-color to use, and so on</p><p>not thread safe</p>";
                e.Params["message"].Value = "message to show to end user";
				e.Params["time"].Value = 3000;
				e.Params["color"].Value = "LightGreen";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

            if (!ip.Contains("message") || string.IsNullOrEmpty(ip["message"].Get<string>()))
				throw new ArgumentException("cannot show a message box without a [message] argument");

            string msgTxt = Expressions.GetExpressionValue(ip["message"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(msgTxt))
                throw new ArgumentException("you must supply a [message] for your message box");

			if (_isFirst)
			{
                messageSmall.Value = "<p>" + msgTxt + "</p>";
			}
			else
			{
                messageSmall.Value += "<p>" + msgTxt + "</p>";
			}

            if (ip.Contains("color"))
                messageSmall.Style[Styles.backgroundColor] = Expressions.GetExpressionValue(ip["color"].Get<string>(), dp, ip, false) as string;
            else
                messageSmall.Style[Styles.backgroundColor] = "";

			if (_isFirst)
			{
				_isFirst = false;
                int time = 3000;
                if (ip.Contains("time"))
                    time = int.Parse(Expressions.GetExpressionValue(ip["time"].Get<string>(), dp, ip, false) as string);
                if (time == -1)
				{
                    new EffectFadeIn(messageSmall, 250)
						.Render();
				}
				else
				{
                    new EffectFadeIn(messageSmall, 250)
						.ChainThese(
                            new EffectTimeout(time),
                            new EffectFadeOut(messageSmall, 250))
							.Render();
				}
			}
		}
	}
}

