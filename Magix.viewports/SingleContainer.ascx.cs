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
     * sample viewport
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
         * show a message box
         */
		[ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.viewports",
                    "Magix.viewports.hyperlisp.inspect.hl",
                    "[magix.viewports.show-message-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.viewports",
                    "Magix.viewports.hyperlisp.inspect.hl",
                    "[magix.viewports.show-message-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("message"))
				throw new ArgumentException("no [message] given to [magix.viewport.show-message]");
            string msgTxt = Expressions.GetExpressionValue(ip["message"].Get<string>(), dp, ip, false) as string;

			if (_isFirst)
                messageSmall.Value = "<p>" + msgTxt + "</p>";
			else
                messageSmall.Value += "<p>" + msgTxt + "</p>";

			if (_isFirst)
			{
				_isFirst = false;

                if (ip.Contains("color"))
                    messageSmall.Style[Styles.backgroundColor] = Expressions.GetExpressionValue(ip["color"].Get<string>(), dp, ip, false) as string;
                else
                    messageSmall.Style[Styles.backgroundColor] = "";

                int time = 3000;
                if (ip.Contains("time"))
                    time = int.Parse(Expressions.GetExpressionValue(ip["time"].Get<string>(), dp, ip, false) as string);
                if (time == -1)
                    new EffectFadeIn(messageSmall, 250)
						.Render();
				else
                    new EffectFadeIn(messageSmall, 250)
						.ChainThese(
                            new EffectTimeout(time),
                            new EffectFadeOut(messageSmall, 250))
							.Render();
			}
		}

        /*
         * returns all default content containers
         */
        protected override string[] GetAllDefaultContainers()
        {
            return new string[] { "content" };
        }
    }
}

