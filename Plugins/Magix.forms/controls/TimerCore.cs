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
	/**
	 * timer control
	 */
    public class TimerCore : BaseControlCore
	{
		/**
		 * creates timer control
		 */
		[ActiveEvent(Name = "magix.forms.controls.timer")]
		public void magix_forms_controls_timer(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			Timer ret = new Timer();

            if (node.Contains("interval") &&
                node["interval"].Value != null)
                ret.Interval = node["interval"].Get<int>();

            FillOutParameters(node, ret);

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

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a timer type of control.&nbsp;&nbsp;
a timer is a control which will periodically run 
to the server in intervalls of [interval] 
milliseconds where it will raise the [tick] 
event handler.&nbsp;&nbsp;this control is 
useful for periodically checking for updates on 
the server, if you're waiting for something to 
occur, such as a chat client, or something similar</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["timer"]);
            node["magix.forms.create-web-part"]["controls"]["timer"]["interval"].Value = "1000";
            node["magix.forms.create-web-part"]["controls"]["timer"]["ontick"].Value = "hyper lisp code";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for timer</strong></p><p>[interval] 
is the number of milliseconds between each time the timer 
should go to the server, and raise the [ontick] event</p>
<p>[ontick] is the active event for that will be raised 
each time the [interval] time hass passed</p>";
		}
	}
}

