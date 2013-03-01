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
		protected DynamicPanel modal;
		protected Panel backdrop;
		protected Panel mdlWrp;
		protected Label mdlHeader;
		protected LinkButton mdlClose;
		protected Panel modalFooter;
		protected Button modalClose;

		private bool _notFirstMessage;
		private int _time = 3000;

        /**
         * Shows a Message Box to the end user, which can be configured
         */
        [ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
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
			{
				messageLabel.Text = "";
				msgBoxHeader.Text = "Message from System";
			}

			messageLabel.Text += "<p>" + e.Params["message"].Get<string>() + "</p>";

			if (e.Params.Contains("code"))
			{
				Node tmp = new Node();
				Node code = e.Params["code"].Clone();
				code.Name = "";
				tmp["JSON"].Value = code;

				RaiseEvent(
					"magix.code.node-2-code",
					tmp);

				messageLabel.Text += "<pre>" + tmp["code"].Get<string>() + "</pre>";
			}

			if (e.Params.Contains("header"))
				msgBoxHeader.Text = e.Params["header"].Get<string>();

			if (e.Params.Contains("time"))
				_time = int.Parse (e.Params["time"].Get<string>());

			if (_time == -1)
			{
				modalFooter.Visible = true;
				new EffectTimeout(500)
					.ChainThese(
						new EffectFocusAndSelect(modalClose))
					.Render();
			}
			else
				modalFooter.Visible = false;

			if (e.Params.Contains("icon"))
			{
				icon.ImageUrl = e.Params["icon"].Get<string>();;
				icon.Visible = true;
			}
			else
				icon.Visible = false;
			string color = "";
			if (e.Params.Contains("color"))
				color = e.Params["color"].Get<string>();
			messageWrapper.Style[Styles.backgroundColor] = color;
			_notFirstMessage = true;
        }

		protected void modalClose_Click(object sender, EventArgs e)
		{
			messageWrapper.Visible = false;
		}

		protected override void magix_viewport_load_module(object sender, ActiveEventArgs e)
		{
			if (e.Params["container"].Get<string>() == "modal")
			{
				backdrop.Visible = true;
				mdlWrp.Visible = true;

				if (e.Params.Contains("header"))
					mdlHeader.Text = e.Params["header"].Get<string>();
				else
					mdlHeader.Text = "Info";

				mdlWrp.Style[Styles.display] = "none";

				new EffectFadeIn(backdrop, 250, 0M, 0.8M)
					.Render();
				new EffectFadeIn(mdlWrp, 250)
					.Render();
			}

			base.magix_viewport_load_module(sender, e);
		}

		protected override void magix_viewport_clear_controls(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("container") && 
			    e.Params["container"].Get<string>() == "modal")
			{
				Node tmp = new Node();
				tmp["container"].Value = "modalFtr";

				RaiseEvent(
					"magix.viewport.clear-controls",
					tmp);

				backdrop.Visible = false;
				mdlWrp.Visible = false;
			}
			base.magix_viewport_clear_controls(sender, e);
		}

		protected void CloseModal(object sender, EventArgs e)
		{
			backdrop.Visible = false;
			mdlWrp.Visible = false;

			Node tmp = new Node();
			tmp["container"].Value = "modal";

			RaiseEvent(
				"magix.viewport.clear-controls",
				tmp);
		}

		protected override void OnPreRender (EventArgs e)
		{
			if (_notFirstMessage)
			{
				if (_time == -1)
				{
					// We only render effects the FIRST time event is called ...
					new EffectFadeIn(messageWrapper, 500)
						.Render();
				}
				else
				{
					// We only render effects the FIRST time event is called ...
					new EffectFadeIn(messageWrapper, 500)
						.ChainThese(
							new EffectTimeout(_time),
							new EffectFadeOut(messageWrapper, 500))
						.Render();
				}
			}
			base.OnPreRender (e);
		}
    }
}
