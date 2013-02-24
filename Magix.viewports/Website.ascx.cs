/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
     * Sample Viewport, nothing fancy, three empty containers
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

		private bool _notFirstMessage;
		private int _time = 3000;

        /**
         * Shows a Message Box to the end user, which can be configured
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
				e.Params["code"].Value = "List of nodes that's to be formatted and shown in pre block";
				return;
			}
			if (!_notFirstMessage)
				messageLabel.Text = "";

			messageLabel.Text += "<p>" + e.Params["message"].Get<string>() + "</p>";

			if (e.Params.Contains ("code"))
			{
				Node tmp = new Node();
				Node code = e.Params["code"].Clone ();
				code.Name = "";
				tmp["JSON"].Value = code;
				RaiseEvent (
					"magix.admin._transform-node-2-code",
					tmp);

				messageLabel.Text += "<pre>" + tmp["code"].Get<string>() + "</pre>";
			}

			if (e.Params.Contains ("header"))
				msgBoxHeader.Text = e.Params["header"].Get<string>();
			if (e.Params.Contains ("time"))
				_time = int.Parse (e.Params["time"].Get<string>());
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
			_notFirstMessage = true;
        }

		protected override void OnPreRender (EventArgs e)
		{
			if (_notFirstMessage)
			{
				// We only render effects the FIRST time event is called ...
				new EffectRollDown(messageWrapper, 500)
					.JoinThese (
						new EffectFadeIn())
					.ChainThese (
						new EffectTimeout(_time),
						new EffectRollUp(messageWrapper, 500)
							.JoinThese (
								new EffectFadeOut()))
					.Render ();
			}
			base.OnPreRender (e);
		}
    }
}
