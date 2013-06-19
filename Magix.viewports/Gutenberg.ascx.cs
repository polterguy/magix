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
    public class Gutenberg : Viewport
    {
		protected Panel wrp;
		protected DynamicPanel header;
		protected DynamicPanel menu;
        protected DynamicPanel content1;
        protected DynamicPanel content2;
		protected DynamicPanel content3;
		protected DynamicPanel content4;
		protected DynamicPanel content5;
		protected DynamicPanel footer;
		protected DynamicPanel trace;
		protected Label message;
		protected Panel confirmWrp;
		protected Label confirmLbl;
		protected Button ok;

		/**
         * Shows a Message Box to the end user, which can be configured
         */
		[ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["message"].Value = "message to show to end user";
				e.Params["inspect"].Value = @"shows a message box to the 
end user for some seconds.&nbsp;&nbsp;not thread safe";
				e.Params["code"].Value = "code goes underneath here";
				e.Params["time"].Value = 3000;
				return;
			}


			if (!e.Params.Contains("message") || e.Params["message"].Get<string>("") == "")
				throw new ArgumentException("cannot show a message box without a [message] argument");

			message.Text = "<p>" + e.Params["message"].Get<string>() + "</p>";

			if (e.Params.Contains("code"))
			{
				Node tmp = new Node();
				Node code = e.Params["code"].Clone();
				code.Name = "";
				tmp["json"].Value = code;

				RaiseEvent(
					"magix.code.node-2-code",
					tmp);

				message.Text += "<pre style='text-align:left;margin-left:120px;'>" + tmp["code"].Get<string>().Replace("<", "&lt;").Replace(">", "&gt;") + "</pre>";
			}

			if (e.Params.Contains("time") && e.Params["time"].Get<int>() == -1)
			{
				new EffectFadeIn(message, 250)
					.Render();
			}
			else
			{
				new EffectFadeIn(message, 250)
					.ChainThese(
						new EffectTimeout(e.Params.Contains("time") ? e.Params["time"].Get<int>(3000) : 3000),
						new EffectFadeOut(message,250))
						.Render();
			}
		}

		private Node ConfirmCode
		{
			get { return ViewState["Magix.Viewport.Gutenberg.ConfirmCode"] as Node; }
			set { ViewState ["Magix.Viewport.Gutenberg.ConfirmCode"] = value; }
		}

		/**
         * asks the user for confirmation of action
         */
		[ActiveEvent(Name = "magix.viewport.confirm")]
		protected void magix_viewport_confirm(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["message"].Value = "message to show to end user";
				e.Params["inspect"].Value = @"shows a message box to the 
end user for some seconds.&nbsp;&nbsp;not thread safe";
				e.Params["code"].Value = "code to execute if user clicks ok goes here";
				e.Params["closable-only"].Value = false;
				return;
			}


			if (!e.Params.Contains("message") || e.Params["message"].Get<string>("") == "")
				throw new ArgumentException("cannot show a message box without a [message] argument");

			confirmLbl.Text = e.Params["message"].Get<string>();

			ConfirmCode = e.Params["code"].Clone();

			confirmWrp.Visible = true;

			if (!e.Params.Contains("closable-only") || e.Params["closable-only"].Get<bool>() == false)
			{
				ok.Visible = true;
			}
			else
			{
				Node tmp = new Node();
				Node code = e.Params["code"].Clone();
				code.Name = "";
				tmp["json"].Value = code;

				RaiseEvent(
					"magix.code.node-2-code",
					tmp);

				confirmLbl.Text += "<pre style='text-align:left;margin-left:120px;margin-right:120px;height:360px;overflow:auto;'>" + tmp["code"].Get<string>().Replace("<", "&lt;").Replace(">", "&gt;") + "</pre>";
				ok.Visible = false;
			}

			new EffectFadeIn(confirmWrp, 250)
				.ChainThese(
					new EffectFocusAndSelect(ok))
				.Render();
		}

		protected void CancelClick(object sender, EventArgs e)
		{
			ConfirmCode = null;
			confirmWrp.Visible = false;
			confirmWrp.Style["display"] = "none";
		}

		protected void confirmWrp_Esc(object sender, EventArgs e)
		{
			ConfirmCode = null;
			confirmWrp.Visible = false;
			confirmWrp.Style["display"] = "none";
		}

		protected void OKClick(object sender, EventArgs e)
		{
			RaiseEvent(
				"magix.execute",
				ConfirmCode);

			ConfirmCode = null;
			confirmWrp.Visible = false;
			confirmWrp.Style["display"] = "none";
		}

		/**
		 * shows the debug grid
		 */
		[ActiveEvent(Name = "magix.viewport.show-grid")]
		protected void magix_viewport_show_grid(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = "shows or hides a debugging grid according to [value] node.&nbsp;&nbsp;not thread safe";
				e.Params["value"].Value = true;
				return;
			}

			Session["magix.viewport.show-grid"] = e.Params["value"].Get<bool>();
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (Session["magix.viewport.show-grid"] != null && ((bool)Session["magix.viewport.show-grid"]) && !wrp.CssClass.Contains("showgrid"))
			{
				wrp.CssClass = wrp.CssClass + " showgrid";
				wrp.CssClass = wrp.CssClass.Trim();
			}
			else if (wrp.CssClass.Contains("showgrid") && (Session["magix.viewport.show-grid"] == null || !((bool)Session["magix.viewport.show-grid"])))
			{
				wrp.CssClass = wrp.CssClass.Replace("showgrid", "").Trim();
			}

			base.OnPreRender (e);
		}

		protected override void magix_viewport_clear_controls(object sender, ActiveEventArgs e)
		{
			base.magix_viewport_clear_controls(sender, e);

			DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
				this, 
				e.Params["container"].Get<string> ());

			if (dyn == null)
				return;

			dyn.CssClass = "";
		}

	}
}

