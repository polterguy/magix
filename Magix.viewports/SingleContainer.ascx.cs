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
        protected Panel confirmWrp;
		protected Label confirmLbl;
        protected Button ok;
        protected Button cancel;
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
                    "[magix.viewports.show-message-dox].value");
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

            string msgTxt = Expressions.GetFormattedExpression("message", e.Params, "");

			if (_isFirst)
                messageSmall.Value = "<p>" + msgTxt + "</p>";
			else
                messageSmall.Value += "<p>" + msgTxt + "</p>";

			if (_isFirst)
			{
				_isFirst = false;

                if (ip.Contains("color"))
                    messageSmall.Style[Styles.backgroundColor] = Expressions.GetExpressionValue<string>(ip["color"].Get<string>(), dp, ip, false);
                else
                    messageSmall.Style[Styles.backgroundColor] = "";

                int time = 3000;
                if (ip.Contains("time"))
                    time = Expressions.GetExpressionValue<int>(ip["time"].Get<string>(), dp, ip, false);
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
         * contains code to execute upon confirmation
         */
        private Node ConfirmCode
        {
            get { return ViewState["Magix.Viewport.Gutenberg.ConfirmCode"] as Node; }
            set { ViewState["Magix.Viewport.Gutenberg.ConfirmCode"] = value; }
        }

        /*
         * asks the user for confirming action
         */
        [ActiveEvent(Name = "magix.viewport.confirm")]
        protected void magix_viewport_confirm(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.viewports",
                    "Magix.viewports.hyperlisp.inspect.hl",
                    "[magix.viewports.confirm-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.viewports",
                    "Magix.viewports.hyperlisp.inspect.hl",
                    "[magix.viewports.confirm-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("message"))
                throw new ArgumentException("no [message] given to [magix.viewport.confirm]");
            string message = Expressions.GetFormattedExpression("message", e.Params, "");

            confirmLbl.Value = message;

            if (ip.Contains("code"))
                ConfirmCode = ip["code"].Clone();
            else
                ConfirmCode = null;
            confirmWrp.Visible = true;

            if (!ip.Contains("closable-only") || ip["closable-only"].Get<bool>() == false)
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
                    Node code = ip["code"].Clone();
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

        /*
         * closes confirm message box
         */
        protected void CancelClick(object sender, EventArgs e)
        {
            ConfirmCode = null;
            CloseMessageBox();
        }

        /*
         * closes confirm message box
         */
        protected void confirmWrp_Esc(object sender, EventArgs e)
        {
            ConfirmCode = null;
            CloseMessageBox();
        }

        /*
         * closes confirm message box
         */
        private void CloseMessageBox()
        {
            confirmWrp.Visible = false;
            confirmWrp.Style["display"] = "none";
        }

        /*
         * closes confirm message box and executes hyperlisp given
         */
        protected void OKClick(object sender, EventArgs e)
        {
            CloseMessageBox();
            RaiseActiveEvent(
                "magix.execute",
                ConfirmCode);
            ConfirmCode = null;
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

