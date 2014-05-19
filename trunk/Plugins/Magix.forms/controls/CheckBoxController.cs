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
            node["inspect"].Value = @"
<p>creates a checkbox input type of web control.&nbsp;&nbsp;
the checkbox web control is useful for representing choices 
like yes or no.&nbsp;&nbsp;the checkbox web control renders 
like &lt;input type='checkbox' .../&gt;.&nbsp;&nbsp;if you wish 
to execute hyper lisp code when the checked state is changed, 
then handle the [oncheckedchanged] active event</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["check-box"]);
            node["magix.forms.create-web-part"]["controls"]["check-box"]["checked"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["check-box"]["key"].Value = "C";
            node["magix.forms.create-web-part"]["controls"]["check-box"]["disabled"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["check-box"]["oncheckedchanged"].Value = "hyper lisp code";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for checkbox</strong></p><p>[checked] 
determines its state.&nbsp;&nbsp; if [checked] is true, then 
the checkbox is checked, and has a visual clue that informs 
the user of that is is selected.&nbsp;&nbsp;if [checked] is 
false, then is is not selected.&nbsp;&nbsp;this is the 
property which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] for 
your web control</p><p>[key] is the keyboard shortcut.
&nbsp;&nbsp;how to invoke the keyboard shortcut is different 
from system to system, but on a windows system, you normally 
invoke the keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;
if you have for instance 's' as your keyboard shortcut, then 
the end user will have to click shift+alt+s at the same time 
to invoke the keyboard shortcut for your web control</p>
<p>[disabled] enables or disables the web control.&nbsp;&nbsp;
this can be changed or retrieved after the button is created 
by invoking the [magix.forms.set-enabled] or the 
[magix.forms.get-enabled] active events.&nbsp;&nbsp;legal 
values are true and false</p><p>[oncheckedchanged] is raised 
when checked state of control changes</p>";
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

