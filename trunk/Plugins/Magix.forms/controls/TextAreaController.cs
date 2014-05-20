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
            AppendInspect(node["inspect"], @"creates a text-area type of web control

a text-area is an input control for text, where the end user can type in text.  the 
text-area can handle multiple lines of input in its text portions");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-area"]);
            node["magix.forms.create-web-part"]["controls"]["text-area"]["placeholder"].Value = "shadow text ...";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["rows"].Value = 5;
            node["magix.forms.create-web-part"]["controls"]["text-area"]["value"].Value = "is there anybody out there?";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["key"].Value = "T";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["disabled"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-area"]["ontextchanged"].Value = "hyperlisp code";
            AppendInspect(node["inspect"], @"[rows] is how many visible rows of 
text there shall be at the same time.  this property defines the default height 
of the text-area, though some browsers have the possibility of letting the user 
increase or decrease the size of text-areas", true);
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

