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

		/*
         * shows a message box
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
				throw new ArgumentException("no [message] parameter given to [magix.viewport.show-message]");
            string msgTxt = Expressions.GetExpressionValue<string>(ip["message"].Get<string>(), dp, ip, false);
            if (ip["message"].Count > 0)
                msgTxt = Expressions.FormatString(dp, ip, ip["message"], msgTxt);

            Label whichMsg = message;
            if (!ip.Contains("code"))
                whichMsg = messageSmall;

			if (_isFirst)
                whichMsg.Value = "<p>" + msgTxt + "</p>";
			else
                whichMsg.Value += "<p>" + msgTxt + "</p>";

            if (ip.Contains("code"))
			{
                Node code = null;
                if (string.IsNullOrEmpty(ip["code"].Get<string>()))
                    code = ip["code"].Clone();
                else
                    code = (Expressions.GetExpressionValue<Node>(ip["code"].Get<string>(), dp, ip, false) ?? new Node()).Clone();
				code.Name = "";
                
                Node tmp = new Node();
                tmp["node"].Value = code;

				RaiseActiveEvent(
					"magix.execute.node-2-code",
					tmp);

                whichMsg.Value += "<pre style='text-align:left;margin-left:120px;overflow:auto;max-height:300px;'>" + 
                    tmp["code"].Get<string>().Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;") + "</pre>";
			}

			if (_isFirst)
			{
				_isFirst = false;

                if (ip.Contains("color"))
                    whichMsg.Style[Styles.backgroundColor] = Expressions.GetExpressionValue<string>(ip["color"].Get<string>(), dp, ip, false);
                else
                    whichMsg.Style[Styles.backgroundColor] = "";

                int time = 3000;
                if (ip.Contains("time"))
                    time = Expressions.GetExpressionValue<int>(ip["time"].Get<string>(), dp, ip, false);
                if (time == -1)
                    new EffectFadeIn(whichMsg, 250)
						.Render();
				else
                    new EffectFadeIn(whichMsg, 250)
						.ChainThese(
                            new EffectTimeout(time),
                            new EffectFadeOut(whichMsg, 250))
							.Render();
			}
		}

        /*
         * contains code to execute upon confirmation
         */
		private Node ConfirmCode
		{
			get { return ViewState["Magix.Viewport.Gutenberg.ConfirmCode"] as Node; }
			set { ViewState ["Magix.Viewport.Gutenberg.ConfirmCode"] = value; }
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
                    "[magix.viewports.confirm-dox].Value");
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
            string message = Expressions.GetExpressionValue<string>(ip["message"].Get<string>(), dp, ip, false);
            if (ip["message"].Count > 0)
                message = Expressions.FormatString(dp, ip, ip["message"], message);

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
         * locks given container
         */
        [ActiveEvent(Name = "magix.viewport.pin-container")]
        protected void magix_viewport_pin_container(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.viewports",
                    "Magix.viewports.hyperlisp.inspect.hl",
                    "[magix.viewports.pin-container-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.viewports",
                    "Magix.viewports.hyperlisp.inspect.hl",
                    "[magix.viewports.pin-container-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("container"))
                throw new ArgumentException("no [container] given to [magix.viewport.pin-container]");
            string container = Expressions.GetExpressionValue<string>(ip["container"].Get<string>(), dp, ip, false);

            if (ViewState["magix.viewport.pin-container"] == null)
                ViewState["magix.viewport.pin-container"] = new Node();

            Node viewStateNode = ViewState["magix.viewport.pin-container"] as Node;
            if (!viewStateNode.Contains(container))
                viewStateNode.Add(new Node(container));
        }

        /*
         * returns all the default content containers
         */
        protected override string[] GetAllDefaultContainers()
        {
            List<string> retVal = new List<string>(
                new string[] { 
                    "content1", 
                    "content2", 
                    "content3", 
                    "content4", 
                    "content5", 
                    "content6", 
                    "content7" });
            Node viewStateNode = ViewState["magix.viewport.pin-container"] as Node;
            if (viewStateNode != null)
            {
                foreach (Node idx in viewStateNode)
                {
                    retVal.Remove(idx.Name);
                }
            }
            return retVal.ToArray();
        }
    }
}

