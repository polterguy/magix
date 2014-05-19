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
	 * textarea control
	 */
    public class TextAreaController : BaseWebControlFormElementInputTextController
	{
		/*
		 * creates text area control
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-area")]
		public void magix_forms_controls_text_area(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			TextArea ret = new TextArea();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();
			if (node.Contains("rows") && 
			    node["rows"].Value != null)
				ret.Rows = node["rows"].Get<int>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a text area input type of web control.&nbsp;&nbsp;
a text area is an input control for text, where the end user 
can type in text, that can handle multiple lines of text</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-area"]);
            node["magix.forms.create-web-part"]["controls"]["text-area"]["placeholder"].Value = "shadow text ...";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["rows"].Value = 5;
            node["magix.forms.create-web-part"]["controls"]["text-area"]["value"].Value = "is there anybody out there?";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["key"].Value = "T";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["disabled"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-area"]["ontextchanged"].Value = "hyper lisp code";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for text area</strong></p><p>[placeholder] 
is shadow text, only visible when input area is empty.&nbsp;&nbsp;
use this property to display some information about your web 
control</p><p>[rows] is how many visible rows of text there shall 
be at the same time</p><p>[value] is what text the control shall 
have, or currently have been changed to.&nbsp;&nbsp;this is the 
property which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] for your 
web control</p><p>[key] is the keyboard shortcut.&nbsp;&nbsp;
how to invoke the keyboard shortcut is different from system 
to system, but on a windows system, you normally invoke the 
keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;if you 
have for instance 's' as your keyboard shortcut, then the end 
user will have to click shift+alt+s at the same time to invoke 
the keyboard shortcut for your web control</p><p>[disabled] 
enables or disables the web control.&nbsp;&nbsp;this can be 
changed or retrieved after the button is created by invoking 
the [magix.forms.set-enabled] or [magix.forms.get-enabled] 
active events.&nbsp;&nbsp;legal values are true and false</p>
<p>[ontextchanged] is raised when text is changed by user, 
and user moves focus out from the web control somehow</p>";
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((TextArea)that).Value;
		}
	}
}

