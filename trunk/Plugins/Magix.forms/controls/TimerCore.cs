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

			if (node.Contains("tick"))
			{
				Node codeNode = node["tick"].Clone();

				ret.Tick += delegate(object sender2, EventArgs e2)
				{
					Timer that2 = sender2 as Timer;
					if (!string.IsNullOrEmpty(that2.Info))
						codeNode["$"]["info"].Value = that2.Info;

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a timer type of control.&nbsp;&nbsp;
a [timer] is a control which will periodically run to the server in intervalls of 
[interval] milliseconds where it will raise the [tick] event handler";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["timer"]["interval"].Value = "1000";
			base.Inspect(node["controls"]["timer"]);
			node["controls"]["timer"]["tick"].Value = "hyper lisp code";
		}
	}
}

