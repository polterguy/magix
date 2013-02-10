/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
    /**
     */
    public class Website : Viewport
    {
        protected DynamicPanel content1;
        protected DynamicPanel content2;
        protected DynamicPanel content3;
		protected Panel messageWrapper;
		protected Label msgBoxHeader;
		protected Label messageLabel;
		protected Image icon;

		public void Page_Load (object sender, EventArgs e)
		{
			messageLabel.Text = "";
		}

        /**
         */
        [ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["message"].Value = "Message to show to End User";
				e.Params["header"].Value = "Message from system";
				e.Params["time"].Value = "1000";
				e.Params["color"].Value = "#eeaaaa";
				e.Params["inspect"].Value = @"Shows a message box to the 
end user for some seconds.";
				e.Params["icon"].Value = "media/images/magix-logo-tiny.png";
				return;
			}
			bool first = string.IsNullOrEmpty (messageLabel.Text);
			messageLabel.Text += "<p>" + e.Params["message"].Get<string>() + "</p>";
			if (e.Params.Contains ("header"))
				msgBoxHeader.Text = e.Params["header"].Get<string>();
			int time = 3000;
			if (e.Params.Contains ("time"))
				time = int.Parse (e.Params["time"].Get<string>());
			if (e.Params.Contains ("icon"))
			{
				icon.ImageUrl = e.Params["icon"].Get<string>();;
				icon.Visible = true;
			}
			else
				icon.Visible = false;
			string color = "";
			if (e.Params.Contains ("color"))
				color = e.Params["color"].Get<string>();
			messageWrapper.Style[Styles.backgroundColor] = color;
			if (first)
			{
				// We only render effects the FIRST time event is called ...
				new EffectRollDown(messageWrapper, 500)
					.JoinThese (
						new EffectFadeIn())
					.ChainThese (
						new EffectTimeout(time),
						new EffectRollUp(messageWrapper, 500)
							.JoinThese (
								new EffectFadeOut()))
					.Render ();
			}
        }
    }
}
