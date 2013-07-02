/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 */
	public class RadioButtonCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.radio")]
		public void magix_forms_controls_radio(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			RadioButton ret = new RadioButton();

			FillOutParameters(node, ret);

			if (node.Contains("group") && node["group"].Value != null)
				ret.GroupName = node["group"].Get<string>();

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
			node["inspect"].Value = @"creates a radio input type of web control.&nbsp;&nbsp;
several radio controls can be combined together to form a multiple
choice type of input value in combination.&nbsp;&nbsp; to create a group
of related [radio] controls, make sure all grouped radio controls have the 
same [group] value.&nbsp;&nbsp;then only one of your radio controls can be 
selected at the same time.&nbsp;&nbsp;meaning you can ask questions such as, 
'chicken, fish or veggies?'.&nbsp;&nbsp;[groups] sets group association of control.&nbsp;&nbsp;
[checked] sets its state to true or false.&nbsp;&nbsp;[key] changes keyboard shortcut.&nbsp;&nbsp;
[enabled] changes the enabled state of your control to true or false.&nbsp;&nbsp;
[oncheckedchanged] is raised when checked state of control changes";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["radio"]["group"].Value = "id_of_group";
			node["controls"]["radio"]["checked"].Value = true;
			node["controls"]["radio"]["key"].Value = "C";
			node["controls"]["radio"]["enabled"].Value = true;
			base.Inspect(node["controls"]["radio"]);
			node["controls"]["radio"]["oncheckedchanged"].Value = "hyper lisp code";
		}
	}
}

