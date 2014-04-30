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

				ret.EscPressed += delegate(object sender2, EventArgs e2)
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
            node["inspect"].Value = @"
<p>creates a text area input type of web control.&nbsp;&nbsp;
a text area is an input control for text, where the end user 
can type in text, that can handle multiple lines of text</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-area"]);
            node["magix.forms.create-web-part"]["controls"]["text-area"]["place-holder"].Value = "shadow text ...";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["rows"].Value = 5;
            node["magix.forms.create-web-part"]["controls"]["text-area"]["text"].Value = "is there anybody out there?";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["key"].Value = "T";
            node["magix.forms.create-web-part"]["controls"]["text-area"]["enabled"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["text-area"]["ontextchanged"].Value = "hyper lisp code";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for text area</strong></p><p>[place-holder] 
is shadow text, only visible when input area is empty.&nbsp;&nbsp;
use this property to display some information about your web 
control</p><p>[rows] is how many visible rows of text there shall 
be at the same time</p><p>[text] is what text the control shall 
have, or currently have been changed to.&nbsp;&nbsp;this is the 
property which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] for your 
web control</p><p>[key] is the keyboard shortcut.&nbsp;&nbsp;
how to invoke the keyboard shortcut is different from system 
to system, but on a windows system, you normally invoke the 
keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;if you 
have for instance 's' as your keyboard shortcut, then the end 
user will have to click shift+alt+s at the same time to invoke 
the keyboard shortcut for your web control</p><p>[enabled] 
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
			return ((TextArea)that).Text;
		}
	}
}

