/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
                    "[magix.forms.effect-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.effect-sample]");
				return;
			}

            if (ip.ContainsValue("id"))
                CreateEffect(ip, FindControl<Control>(ip)).Render();
            else
                CreateEffect(ip, null).Render();
        }

		private Effect CreateEffect(Node pars, Control ctrl)
		{
			Effect tmp = null;
			switch (pars["type"].Get<string>())
			{
				case "fade-in":
                    tmp = CreateFadeInEffect(pars, ctrl);
                    break;
				case "fade-out":
                    tmp = CreateFadeOutEffect(pars, ctrl);
                    break;
				case "focus-and-select":
				    tmp = new EffectFocusAndSelect(ctrl);
                    break;
				case "highlight":
                    tmp = CreateHighlightEffect(pars, ctrl);
                    break;
				case "move":
                    tmp = CreateMoveEffect(pars, ctrl);
                    break;
				case "roll-up":
                    tmp = CreateRollUpEffect(pars, ctrl);
                    break;
				case "roll-down":
                    tmp = CreateRollDownEffect(pars, ctrl);
                    break;
				case "size":
                    tmp = CreateSizeEffect(pars, ctrl);
                    break;
				case "slide":
                    tmp = CreateOffsetEffect(pars, ctrl);
                    break;
				case "timeout":
                    tmp = CreateTimeoutEffect(pars);
                    break;
                default:
                    throw new ArgumentException("[type] of effect unrecognized");
			}

			if (pars.Contains("joined"))
			{
				foreach (Node idx in pars["joined"])
				{
					Effect joined = CreateEffect(idx, null);
					tmp.Joined.Add(joined);
				}
			}

			if (pars.Contains("chained"))
			{
				foreach (Node idx in pars["chained"])
				{
					Effect chained = CreateEffect(idx, FindControl<Control>(idx));
					tmp.Chained.Add(chained);
				}
			}
			return tmp;
		}

        private static Effect CreateTimeoutEffect(Node pars)
        {
            return new EffectTimeout(pars["time"].Get(-1));
        }

        private static Effect CreateOffsetEffect(Node pars, Control ctrl)
        {
            return new EffectSlide(ctrl, pars["time"].Get(-1), pars["offset"].Get(-1));
        }

        private static Effect CreateSizeEffect(Node pars, Control ctrl)
        {
            return new EffectSize(ctrl, pars["time"].Get(-1), pars["width"].Get(-1), pars["height"].Get(-1));
        }

        private static Effect CreateRollDownEffect(Node pars, Control ctrl)
        {
            return new EffectRollDown(ctrl, pars["time"].Get(-1));
        }

        private static Effect CreateRollUpEffect(Node pars, Control ctrl)
        {
            return new EffectRollUp(ctrl, pars["time"].Get(-1));
        }

        private static Effect CreateMoveEffect(Node pars, Control ctrl)
        {
            return new EffectMove(ctrl, pars["time"].Get(-1), pars["left"].Get(-1), pars["top"].Get(-1));
        }

        private static Effect CreateHighlightEffect(Node pars, Control ctrl)
        {
            return new EffectHighlight(ctrl, pars["time"].Get(-1));
        }

        private static Effect CreateFadeOutEffect(Node pars, Control ctrl)
        {
            return new EffectFadeOut(ctrl, pars["time"].Get(-1), pars["from"].Get(1.0M), pars["to"].Get(0.0M));
        }

        private static Effect CreateFadeInEffect(Node pars, Control ctrl)
        {
            return new EffectFadeIn(ctrl, pars["time"].Get(-1), pars["from"].Get(0.0M), pars["to"].Get(1.0M));
        }
	}
}

