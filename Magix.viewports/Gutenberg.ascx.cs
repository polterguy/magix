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
		protected DynamicPanel content6;
        protected DynamicPanel content7;
        protected DynamicPanel help;
        protected DynamicPanel footer;
		protected DynamicPanel trace;
        protected Label message;
        protected Label messageSmall;
        protected Panel confirmWrp;
		protected Label confirmLbl;
		protected Button ok;
		protected Button cancel;
		private bool _isFirst = true;

        protected override void OnLoad(EventArgs e)
        {
            if (IsPostBack && !Manager.Instance.IsAjaxCallback || !IsPostBack)
            {
                Node node = new Node();
                node["type"].Value = "css";
                node["file"].Value = "media/grid/main.css";
                RaiseActiveEvent(
                    "magix.viewport.include-client-file",
                    node);
            }
            base.OnLoad(e);
        }

        protected override string GetDefaultContainer()
        {
            return "content1";
        }

		/**
         * Shows a Message Box to the end user, which can be configured
         */
		[ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"<p>shows a message box to the 
end user for some seconds.&nbsp;&nbsp;all arguments can either be a constant 
or an expression.&nbsp;&nbsp[code] is optional code to show to end user.&nbsp;
&nbsp;[time] is optional number of milliseconds to display the message box.
&nbsp;&nbsp;[color] is optional background-color to use for the message box</p>
<p>if you create multiple message boxes within the same request, then all 
messages will show, but only the first invocation will decide the other 
parameters, such as how long time to show the message box, what background-color 
to use, and so on</p><p>not thread safe</p>";
                e.Params["message"].Value = "message to show to end user";
                e.Params["code"].Value = "code goes underneath here";
				e.Params["time"].Value = 3000;
				e.Params["color"].Value = "LightGreen";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

            if (!ip.Contains("message") || string.IsNullOrEmpty(ip["message"].Get<string>()))
				throw new ArgumentException("cannot show a message box without a [message] argument");

            string msgTxt = Expressions.GetExpressionValue(ip["message"].Get<string>(), dp, ip, false).ToString();
            if (string.IsNullOrEmpty(msgTxt))
                throw new ArgumentException("you must supply a [message] for your message box");

            Label whichMsg = message;
            if (!ip.Contains("code"))
            {
                whichMsg = messageSmall;
            }

			if (_isFirst)
			{
                whichMsg.Value = "<p>" + msgTxt + "</p>";
			}
			else
			{
                whichMsg.Value += "<p>" + msgTxt + "</p>";
			}

            if (ip.Contains("color"))
                whichMsg.Style[Styles.backgroundColor] = Expressions.GetExpressionValue(ip["color"].Get<string>(), dp, ip, false) as string;
            else
                whichMsg.Style[Styles.backgroundColor] = "";

            if (ip.Contains("code"))
			{
                Node code = null;
                if (string.IsNullOrEmpty(ip["code"].Get<string>()))
                    code = ip["code"].Clone();
                else
                    code = (Expressions.GetExpressionValue(ip["code"].Get<string>(), dp, ip, false) as Node ?? new Node()).Clone();
				code.Name = "";
                
                Node tmp = new Node();
                tmp["node"].Value = code;

				RaiseActiveEvent(
					"magix.execute.node-2-code",
					tmp);

                whichMsg.Value += "<pre style='text-align:left;margin-left:120px;overflow:auto;max-height:300px;'>" + 
                    tmp["code"].Get<string>().Replace("<", "&lt;").Replace(">", "&gt;") + "</pre>";
			}

			if (_isFirst)
			{
				_isFirst = false;
                int time = 3000;
                if (ip.Contains("time"))
                    time = int.Parse(Expressions.GetExpressionValue(ip["time"].Get<string>(), dp, ip, false) as string);
                if (time == -1)
				{
                    new EffectFadeIn(whichMsg, 250)
						.Render();
				}
				else
				{
                    new EffectFadeIn(whichMsg, 250)
						.ChainThese(
                            new EffectTimeout(time),
                            new EffectFadeOut(whichMsg, 250))
							.Render();
				}
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


            if (!Ip(e.Params).Contains("message") || Ip(e.Params)["message"].Get<string>("") == "")
				throw new ArgumentException("cannot show a confirm message box without a [message] argument");

            confirmLbl.Value = Ip(e.Params)["message"].Get<string>();

            if (Ip(e.Params).Contains("code"))
                ConfirmCode = Ip(e.Params)["code"].Clone();
			else
				ConfirmCode = null;

			confirmWrp.Visible = true;

            if (!Ip(e.Params).Contains("closable-only") || Ip(e.Params)["closable-only"].Get<bool>() == false)
			{
				ok.Visible = true;
				cancel.Value = "cancel";
			}
			else
			{
				cancel.Value = "close";
				Node tmp = new Node();

				if (ConfirmCode != null)
				{
                    Node code = Ip(e.Params)["code"].Clone();
					code.Name = "";
					tmp["node"].Value = code;

					RaiseActiveEvent(
						"magix.execute.node-2-code",
						tmp);

					confirmLbl.Value += "<pre style='text-align:left;margin-left:120px;overflow:auto;max-height:300px;'>" + tmp["code"].Get<string>().Replace("<", "&lt;").Replace(">", "&gt;") + "</pre>";
				}
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
            confirmWrp.Visible = false;
            confirmWrp.Style["display"] = "none";
            
            RaiseActiveEvent(
				"magix.execute",
				ConfirmCode);

			ConfirmCode = null;
		}

		protected override void OnPreRender(EventArgs e)
		{
			if (Session["magix.viewport.show-grid"] != null && ((Node)Session["magix.viewport.show-grid"]).Get<bool>() && !wrp.Class.Contains("showgrid"))
			{
				wrp.Class = wrp.Class + " showgrid";
				wrp.Class = wrp.Class.Trim();
			}
			else if (wrp.Class.Contains("showgrid") && (Session["magix.viewport.show-grid"] == null || !((Node)Session["magix.viewport.show-grid"]).Get<bool>()))
			{
				wrp.Class = wrp.Class.Replace("showgrid", "").Trim().Replace("  ", " ");
			}

			base.OnPreRender (e);
		}

		protected override void magix_viewport_clear_controls(object sender, ActiveEventArgs e)
		{
            if (Ip(e.Params).Contains("inspect") && Ip(e.Params)["inspect"].Value == null)
			{
				base.magix_viewport_clear_controls(sender, e);
				return;
			}

			base.magix_viewport_clear_controls(sender, e);
			DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
				this,
                Ip(e.Params)["container"].Get<string>());

			if (dyn == null)
				return;

			dyn.Class = "";
		}
	}
}

