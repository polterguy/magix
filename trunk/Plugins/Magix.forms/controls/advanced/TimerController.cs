/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
            Node node = ip["_code"].Get<Node>();

            if (node.Contains("interval") &&
                node["interval"].Value != null)
                ret.Interval = node["interval"].Get<int>();

            FillOutParameters(e.Params, ret);

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

		protected override void Inspect (Node node)
		{
            AppendInspect(node["inspect"], @"creates a timer type of control

a timer is a control which will periodically run to the server in intervalls 
of [interval] milliseconds where it will raise the [tick] event handler.  this 
control is useful for periodically checking for updates on the server, if 
you're waiting for something to occur, such as a chat client, or something 
similar");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["timer"]);
            node["magix.forms.create-web-part"]["controls"]["timer"]["interval"].Value = "1000";
            node["magix.forms.create-web-part"]["controls"]["timer"]["visible"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["timer"]["ontick"].Value = "hyperlisp code";
            AppendInspect(node["inspect"], @"[interval] is the number of 
milliseconds between each time the timer should go to the server, and raise 
the ontick event

[ontick] is the active event for that will be raised each time the [interval] 
time has passed", true);
		}
	}
}

