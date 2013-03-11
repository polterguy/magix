/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 */
	public class CheckBoxCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.check")]
		public void magix_forms_controls_check(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			CheckBox ret = new CheckBox();

			FillOutParameters(node, ret);

			if (node.Contains("checked") && node["checked"].Value != null)
				ret.Checked = node["checked"].Get<bool>();

			if (node.Contains("key") && 
			    !string.IsNullOrEmpty(node["key"].Get<string>()))
				ret.AccessKey = node["key"].Get<string>();

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

			if (node.Contains("oncheckedchanged"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["oncheckedchanged"].Clone();

				ret.CheckedChanged += delegate(object sender2, EventArgs e2)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a checkbox input type of web control.&nbsp;&nbsp;
useful for representing choices like yes or no.&nbsp;&nbsp;[checked] determines its 
state, true or false.&nbsp;&nbsp;key is keyboard shortcut.&nbsp;&nbsp;[enabled] can be 
true or false, and determines if it is possible to change its state, or not.&nbsp;&nbsp;
[oncheckedchanged] is raised when checked state of control changes";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["check"]["checked"].Value = true;
			node["controls"]["check"]["key"].Value = "C";
			node["controls"]["check"]["enabled"].Value = true;
			node["controls"]["check"]["oncheckedchanged"].Value = "hyper lisp code";
			base.Inspect(node["controls"]["check"]);
		}
	}
}

