/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/*
	 * timer control
	 */
    internal sealed class TimerController : BaseControlController
	{
		/*
		 * creates timer control
		 */
		[ActiveEvent(Name = "magix.forms.controls.timer")]
		private void magix_forms_controls_timer(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

            Timer ret = new Timer();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("interval"))
                ret.Interval = node["interval"].Get<int>();

			if (ShouldHandleEvent("ontick", node))
			{
				Node codeNode = node["ontick"].Clone();
				ret.Tick += delegate(object sender2, EventArgs e2)
				{
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            ip["_ctrl"].Value = ret;
		}

        /*
         * disables or enables timer
         */
        [ActiveEvent(Name = "magix.forms.timer.enable")]
        private static void magix_forms_timer_enable(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.timer.enable-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.timer.enable-sample]");
                return;
            }

            Timer ctrl = FindControl<Timer>(e.Params);
            ctrl.Disabled = !ip.GetValue("value", true);
        }

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.timer-dox-start].value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.timer-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["timer"]);
            node["magix.forms.create-web-part"]["controls"]["timer"]["visible"].UnTie(); // makes no sense
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.timer-dox-end].value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["timer"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.timer-sample-end]");
		}
	}
}

