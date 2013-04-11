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
		 * executes the given javascript
		 */
        [ActiveEvent(Name = "magix.viewport.execute-javascript")]
		protected void magix_viewport_execute_javascript(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["execute:magix.viewport.execute-javascript"]["script"].Value = "alert('{0}');";
				e.Params["execute:magix.viewport.execute-javascript"]["script"]["x"].Value = "thomas";
				e.Params["inspect"].Value = @"executes the javascript given in [script] using the values
of its children as string.format values.&nbsp;&nbsp;
not thread safe";
				return;
			}

			if (!e.Params.Contains("script"))
				throw new ArgumentException("you need a [script] value to execute javascript");

			string script = e.Params["script"].Get<string>();

			if (e.Params["script"].Count > 0)
			{
				for (int idx = 0; idx < e.Params["script"].Count; idx++)
				{
					script = script.Replace("{" + idx + "}", e.Params["script"][idx].Get<string>());
				}
			}

			AjaxManager.Instance.WriterAtBack.Write(script);
		}

		/**
		 * Changes header of Modal Form
		 */
        [ActiveEvent(Name = "magix.viewport.change-modal-header")]
		protected void magix_viewport_change_modal_header(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["execute:magix.viewport.change-modal-header"].Value = null;
				e.Params["inspect"].Value = @"changes the header of the modal viewport container, 
if it is visible.&nbsp;&nbsp;the header is set to [header].&nbsp;&nbsp;not thread safe";
				e.Params["header"].Value = "new header";
				return;
			}

			mdlHeader.Text = e.Params["header"].Get<string>();
		}

        /**
         */
        [ActiveEvent(Name = "magix.forms.effect")]
		protected void magix_forms_effect(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"creates an effect on the 
[id] element in the viewport lasting for [time] milliseconds [joined] with and chaining
executing [chained] afterwards.&nbsp;&nbsp;
different effects have different properties.&nbsp;&nbsp;not thread safe";
				e.Params["type"].Value = "fade-in|fade-out|focus-and-select|highlight|move|roll-up|roll-down|size|slide|timeout";
				e.Params["id"].Value = "idOfMyWidget";
				e.Params["time"].Value = 500M;
				e.Params["joined"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["time"].Value = 500M;
				return;
			}
			Effect tmp = CreateEffect(e.Params);

			tmp.Render();
		}

		private Effect CreateEffect (Node pars)
		{
			string id = null;
			Control ctrl = null;
			if (pars.Contains("id"))
			{
				id = pars["id"].Get<string>();
				ctrl = Selector.FindControl<Control>(this, id);
			}

			Effect tmp = null;
			switch (pars["type"].Get<string>())
			{
				case "fade-in":
				{
					decimal frm = 0.0M;
					decimal to = 1.0M;
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					if (pars.Contains("from"))
						frm = pars["from"].Get<decimal>();
					if (pars.Contains("to"))
						to = pars["to"].Get<decimal>();
					tmp = new EffectFadeIn(ctrl, milliseconds, frm, to);
				} break;
				case "fade-out":
				{
					decimal frm = 1.0M;
					decimal to = 0.0M;
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					if (pars.Contains("from"))
						frm = pars["from"].Get<decimal>();
					if (pars.Contains("to"))
						to = pars["to"].Get<decimal>();
					tmp = new EffectFadeOut(ctrl, milliseconds, frm, to);
				} break;
				case "focus-and-select":
				{
					if (string.IsNullOrEmpty(id))
						throw new ArgumentException("focus-and-select effect needs an [id] for widget to select");
					tmp = new EffectFocusAndSelect(ctrl);
				} break;
				case "highlight":
				{
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					tmp = new EffectHighlight(ctrl, milliseconds);
				} break;
				case "move":
				{
					int left = -1;
					int top = -1;
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					if (pars.Contains("left"))
						left = pars["left"].Get<int>();
					if (pars.Contains("top"))
						top = pars["top"].Get<int>();
					tmp = new EffectMove(ctrl, milliseconds, left, top);
				} break;
				case "roll-up":
				{
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					tmp = new EffectRollUp(ctrl, milliseconds);
				} break;
				case "roll-down":
				{
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					tmp = new EffectRollDown(ctrl, milliseconds);
				} break;
				case "size":
				{
					int width = -1;
					int height = -1;
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					if (pars.Contains("width"))
						width = pars["width"].Get<int>();
					if (pars.Contains("height"))
						height = pars["height"].Get<int>();
					tmp = new EffectSize(ctrl, milliseconds, width, height);
				} break;
				case "slide":
				{
					int offset = -1;
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					if (pars.Contains("offset"))
						offset = pars["offset"].Get<int>();
					tmp = new EffectSlide(ctrl, milliseconds, offset);
				} break;
				case "timeout":
				{
					int milliseconds = -1;
					if (pars.Contains("time"))
						milliseconds = pars["time"].Get<int>();
					tmp = new EffectTimeout(milliseconds);
				} break;
			}

			if (pars.Contains("joined"))
			{
				foreach (Node idx in pars["joined"])
				{
					Effect tp = CreateEffect(idx);
					tmp.Joined.Add(tp);
				}
			}

			if (pars.Contains("chained"))
			{
				foreach (Node idx in pars["chained"])
				{
					Effect tp = CreateEffect(idx);
					tmp.Chained.Add(tp);
				}
			}

			return tmp;
		}

        /**
         */
        [ActiveEvent(Name = "magix.viewport.verify")]
		protected void magix_viewport_verify(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["message"].Value = "mml message to show to end user";
				e.Params["header"].Value = "message from system";
				e.Params["button-text"].Value = "delete ...";
				e.Params["inspect"].Value = @"shows a modal confirmation box to the 
end user such that he can verify his intent.&nbsp;&nbsp;not thread safe";
				e.Params["code"].Value = "code goes underneath here";
				return;
			}

			if (!e.Params.Contains("header"))
				throw new ArgumentException("you must have a [header] for your verification box");

			if (!e.Params.Contains("code"))
				throw new ArgumentException("you must have a [code] for your verification box");

			Node tmp = new Node();

			tmp["form-id"].Value = "confirmation-box";
			tmp["container"].Value = "modal";
			tmp["css"].Value = "modal-body";
			tmp["header"].Value = e.Params["header"].Get<string>();
			tmp["html"].Value = e.Params["message"].Get<string>();

			RaiseEvent(
				"magix.forms.create-web-page",
				tmp);

			// getting code
			Node code = new Node();

			string btnText = "ok";
			if (e.Params.Contains("button-text"))
				btnText = e.Params["button-text"].Get<string>();

			Node cn = new Node();
			cn["link-button"].Value = "ok";
			cn["link-button"]["text"].Value = btnText;
			cn["link-button"]["css"].Value = "btn btn-primary";
			cn["link-button"]["onclick"].ReplaceChildren(e.Params["code"].Clone());

			code["json"].Value = cn;

			RaiseEvent(
				"magix.code.node-2-code",
				code);

			tmp = new Node();

			tmp["form-id"].Value = "confirmation-navigation";
			tmp["container"].Value = "modalFtr";
			tmp["html"].Value = string.Format(@"
<div class=""btn-group"">
{{{{
{0}
}}}}
</div>
",
				code["code"].Get<string>());
			RaiseEvent(
				"magix.forms.create-web-page",
				tmp);
		}

        /**
         * Shows a Message Box to the end user, which can be configured
         */
        [ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["message"].Value = "message to show to end user";
				e.Params["header"].Value = "message from system";
				e.Params["time"].Value = "1000";
				e.Params["color"].Value = "#eeaaaa";
				e.Params["inspect"].Value = @"shows a message box to the 
end user for some seconds.&nbsp;&nbsp;not thread safe";
				e.Params["icon"].Value = "media/images/magix-logo-tiny.png";
				e.Params["code"].Value = "code goes underneath here";
				return;
			}

			if (!_notFirstMessage)
			{
				messageLabel.Text = "";
				msgBoxHeader.Text = "message from system";
			}

			if (!e.Params.Contains("message") || e.Params["message"].Get<string>("") == "")
				throw new ArgumentException("cannot show a message box without a [message] argument");

			messageLabel.Text += "<p>" + e.Params["message"].Get<string>() + "</p>";

			if (e.Params.Contains("code"))
			{
				Node tmp = new Node();
				Node code = e.Params["code"].Clone();
				code.Name = "";
				tmp["json"].Value = code;

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
