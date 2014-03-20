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
	 * textarea control
	 */
	public class TextAreaCore : FormElementCore
	{
		/**
		 * creates text area control
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-area")]
		public void magix_forms_controls_text_area(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

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
				Node codeNode = node["ontextchanged"].Clone();

				ret.TextChanged += delegate(object sender2, EventArgs e2)
				{
					TextArea that2 = sender2 as TextArea;
					if (!string.IsNullOrEmpty(that2.Info))
						codeNode["$"]["info"].Value = that2.Info;

					object val = GetValue(that2);
					if (val != null)
						codeNode["$"]["value"].Value = val;

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onescpressed"))
			{
				Node codeNode = node["onescpressed"].Clone();

				ret.TextChanged += delegate(object sender2, EventArgs e2)
				{
					TextArea that2 = sender2 as TextArea;
					if (!string.IsNullOrEmpty(that2.Info))
						codeNode["$"]["info"].Value = that2.Info;

					object val = GetValue(that2);
					if (val != null)
						codeNode["$"]["value"].Value = val;

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		/**
		 * sets text value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
            if (ShouldInspectOrHasInspected(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

            if (!Ip(e.Params).Contains("value"))
				throw new ArgumentException("set-value needs [value]");

            TextArea ctrl = FindControl<TextArea>(Ip(e.Params));

			if (ctrl != null)
			{
                ctrl.Text = Ip(e.Params)["value"].Get<string>();
			}
		}

		/**
		 * returns value
		 */
		[ActiveEvent(Name = "magix.forms.get-value")]
		public void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
            if (ShouldInspectOrHasInspected(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

            TextArea ctrl = FindControl<TextArea>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Text;
			}
		}

		/**
		 * selects all text
		 */
		[ActiveEvent(Name = "magix.forms.select-all")]
		protected void magix_forms_select_all(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.select-all"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"selects all text in the specific 
[id] textbox or textarea, in the [form-id] form.&nbsp;&nbsp;not thread safe";
				return;
			}

            TextArea ctrl = FindControl<TextArea>(Ip(e.Params));

			if (ctrl != null)
			{
				ctrl.Select();
			}
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
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
			node["controls"]["text-area"]["enabled"].Value = true;
			base.Inspect(node["controls"]["text-area"]);
			node["controls"]["text-area"]["ontextchanged"].Value = "hyper lisp code";
			node["controls"]["text-area"]["onescpressed"].Value = "hyper lisp code";
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((TextArea)that).Text;
		}
	}
}

