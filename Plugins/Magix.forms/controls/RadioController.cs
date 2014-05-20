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
	 * radio button
	 */
    public class RadioController : BaseWebControlFormElementController
	{
		/*
		 * creates radio button control
		 */
		[ActiveEvent(Name = "magix.forms.controls.radio")]
		public void magix_forms_controls_radio(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Radio ret = new Radio();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();
            if (node.Contains("name") && node["name"].Value != null)
				ret.Name = node["name"].Get<string>();

			if (node.Contains("checked") && node["checked"].Value != null)
				ret.Checked = node["checked"].Get<bool>();

			if (ShouldHandleEvent("oncheckedchanged", node))
			{
				Node codeNode = node["oncheckedchanged"].Clone();
				ret.CheckedChanged += delegate(object sender2, EventArgs e2)
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
            AppendInspect(node["inspect"], @"creates a radio type of web control

several radio buttons can be combined together to form a multiple choice type of 
input value in combination.  to create a group of related radio web controls, make 
sure all grouped radio controls have the same [name] value.  then only one of your 
radios can be selected at any time.  meaning you can ask questions such as, 
'chicken, fish or veggies?', and have one radio web control for each of these choices");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["radio"]);
            node["magix.forms.create-web-part"]["controls"]["radio"]["name"].Value = "id_of_group";
            node["magix.forms.create-web-part"]["controls"]["radio"]["checked"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["radio"]["oncheckedchanged"].Value = "hyperlisp code";
            AppendInspect(node["inspect"], @"[name] sets group association of 
control.  only one radio belonging to the same group can at any one particular 
time be checked

[checked] sets its state to true or false.  this is the value which you set or 
retrieve with the [magix.forms.get-value] or [magix.forms.set-value]

[oncheckedchanged] is the active event raised when the checked state of your 
radio changes by the user", true);
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((Radio)that).Checked;
		}
	}
}

