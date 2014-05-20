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
	/*
	 * effect controller
	 */
    internal sealed class EffectController : BaseControlController
	{
		/*
		 * creates a new effect
         */
		[ActiveEvent(Name = "magix.forms.effect")]
		private void magix_forms_effect(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.effect-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.effect-sample]");
				return;
			}

            CreateEffect(ip, FindControl<Control>(ip)).Render();
		}

		private Effect CreateEffect(Node pars, Control ctrl)
		{
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
                default:
                    throw new ArgumentException("[type] of effect unrecognized");
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

