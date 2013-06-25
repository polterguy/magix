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
	 * contains the textarea control
	 */
	public class TextAreaCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-area")]
		public void magix_forms_controls_text_area(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			TextArea ret = new TextArea();

			FillOutParameters(node, ret);

			if (node.Contains("place-holder") && 
			    !string.IsNullOrEmpty(node["place-holder"].Get<string>()))
				ret.PlaceHolder = node["place-holder"].Get<string>();

			if (node.Contains("rows") && 
			    node["rows"].Value != null)
				ret.Rows = node["rows"].Get<int>();

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("key") && 
			    !string.IsNullOrEmpty(node["key"].Get<string>()))
				ret.AccessKey = node["key"].Get<string>();

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

			if (node.Contains("ontextchanged"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["ontextchanged"].Clone();

				ret.TextChanged += delegate(object sender2, EventArgs e2)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onescpressed"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onescpressed"].Clone();

				ret.TextChanged += delegate(object sender2, EventArgs e2)
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
			node["inspect"].Value = @"creates a text area input type of web control.&nbsp;&nbsp;
a [text-area] is an input control for text, where the end user can type in 
text, that can handle multiple lines of text.&nbsp;&nbsp;[place-holder] is shadow text, 
only visible when input area is empty.&nbsp;&nbsp;[rows] is how many visible rows 
of text there shall be at the same time.&nbsp;&nbsp;[text] is what text the control 
shall have, or currently have been changed to.&nbsp;&nbsp;[key] is keyboard shortcut 
to active, or give focus to text area.&nbsp;&nbsp;[ontextchanged] is raised when 
[text] is changed by user.&nbsp;&nbsp;[onescpressed] is raised when escape key
is pressed, while control has focus";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["text-area"]["place-holder"].Value = "shadow text ...";
			node["controls"]["text-area"]["rows"].Value = 5;
			node["controls"]["text-area"]["text"].Value = "is there anybody out there?";
			node["controls"]["text-area"]["key"].Value = "T";
			base.Inspect(node["controls"]["text-area"]);
			node["controls"]["text-area"]["ontextchanged"].Value = "hyper lisp code";
			node["controls"]["text-area"]["onescpressed"].Value = "hyper lisp code";
		}
	}
}

