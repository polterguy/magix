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
	 * check box
	 */
    public class CheckBoxController : BaseWebControlFormElementController
	{
		/*
		 * creates check-box
		 */
		[ActiveEvent(Name = "magix.forms.controls.check-box")]
		public void magix_forms_controls_check_box(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			CheckBox ret = new CheckBox();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Value as Node;
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
            AppendInspect(node["inspect"], @"creates a check-box input type of web control

the check-box web control is useful for representing to the user that he needs 
to choose one from two options.  the check-box web control renders like &lt;input 
type='checkbox' .../&gt;");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["check-box"]);
            node["magix.forms.create-web-part"]["controls"]["check-box"]["checked"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["check-box"]["key"].Value = "C";
            node["magix.forms.create-web-part"]["controls"]["check-box"]["disabled"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["check-box"]["oncheckedchanged"].Value = "hyperlisp code";
            AppendInspect(node["inspect"], @"[checked] determines the state 
of the check-box.  if checked is true, then the check-box is checked, and 
has a visual clue that informs the user of that is is selected.  if checked 
is false, then is is not selected.  checked is the property which is changed 
or retrieved when you invoke the [magix.forms.set-value] and the [magix.
forms.get-value] for your check-box

[oncheckedchanged] is raised when the checked state of your check-box has 
been changed by the user", true);
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((CheckBox)that).Checked;
		}
	}
}

