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
            node["inspect"].Value = @"
<p>creates a radio button type of web control.&nbsp;&nbsp;
several radio buttons can be combined together to form a 
multiple choice type of input value in combination.
&nbsp;&nbsp; to create a group of related [radio] controls, 
make sure all grouped radio controls have the same [group] 
value.&nbsp;&nbsp;then only one of your radio controls can 
be selected at the same time.&nbsp;&nbsp;meaning you can 
ask questions such as, 'chicken, fish or veggies?'</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["radio"]);
            node["magix.forms.create-web-part"]["controls"]["radio"]["group"].Value = "id_of_group";
            node["magix.forms.create-web-part"]["controls"]["radio"]["checked"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["radio"]["key"].Value = "C";
            node["magix.forms.create-web-part"]["controls"]["radio"]["disabled"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["radio"]["oncheckedchanged"].Value = "hyper lisp code";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for radio button</strong></p>[group] 
sets group association of control.&nbsp;&nbsp;only one radio 
button belonging to the same group can at any one particular 
time be checked</p><p>[checked] sets its state to true or 
false</p><p>[key] is the keyboard shortcut.&nbsp;&nbsp;
how to invoke the keyboard shortcut is different from system 
to system, but on a windows system, you normally invoke the 
keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;if you 
have for instance 's' as your keyboard shortcut, then the 
end user will have to click shift+alt+s at the same time to 
invoke the keyboard shortcut for your web control</p><p>
[disabled] enables or disables the web control.&nbsp;&nbsp;
this can be changed or retrieved after the button is 
created by invoking the [magix.forms.set-enabled] or 
[magix.forms.get-enabled] active events.&nbsp;&nbsp;
legal values are true and false</p>";
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

