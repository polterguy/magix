/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * effect controller
	 */
	public class EffectCore : BaseControlCore
	{
		/**
		 * creates a new effect
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
				e.Params["form-id"].Value = "form-id";
				e.Params["time"].Value = 500M;
				e.Params["joined"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["time"].Value = 500M;
				return;
			}

            Control ctrl = FindControl<Control>(Ip(e.Params));

			if (ctrl == null)
				throw new ArgumentException("couldn't find control to run effect for");

            Effect tmp = CreateEffect(Ip(e.Params), ctrl);
			tmp.Render();
		}

		private Effect CreateEffect(Node pars, Control ctrl)
		{
			string id = null;

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
					Effect tp = CreateEffect(idx, null);
					tmp.Joined.Add(tp);
				}
			}

			if (pars.Contains("chained"))
			{
				foreach (Node idx in pars["chained"])
				{
					Effect tp = CreateEffect(idx, FindControl<Control>(idx));
					tmp.Chained.Add(tp);
				}
			}

			return tmp;
		}
	}
}

