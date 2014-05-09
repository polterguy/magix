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
	 * contains the textbox control
	 */
	public class TextBoxCore : FormElementCore
	{
		/**
		 * creates a text-box
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-box")]
		public void magix_forms_controls_text_box(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			TextBox ret = new TextBox();

            FillOutParameters(e.Params, ret);

			if (node.Contains("place-holder") && 
			    !string.IsNullOrEmpty(node["place-holder"].Get<string>()))
				ret.PlaceHolder = node["place-holder"].Get<string>();

			if (node.Contains("capitalize") && 
			    node["capitalize"].Value != null)
				ret.AutoCapitalize = node["capitalize"].Get<bool>();

			if (node.Contains("correct") && 
			    node["correct"].Value != null)
				ret.AutoCorrect = node["correct"].Get<bool>();

			if (node.Contains("complete") && 
			    node["complete"].Value != null)
				ret.AutoComplete = node["complete"].Get<bool>();

			if (node.Contains("disabled") &&
                node["disabled"].Value != null)
                ret.Disabled = node["disabled"].Get<bool>();

			if (node.Contains("max") && 
			    node["max"].Value != null)
				ret.MaxLength = node["max"].Get<int>();

			if (node.Contains("value") && 
			    !string.IsNullOrEmpty(node["value"].Get<string>()))
				ret.Value = node["value"].Get<string>();

			if (node.Contains("mode") && 
				node["mode"].Value != null)
			{
				switch(node["mode"].Get<string>())
				{
				case "normal":
					ret.TextMode = TextBox.TextBoxMode.Text;
					break;
                case "password":
                    ret.TextMode = TextBox.TextBoxMode.Password;
                    break;
                case "email":
                    ret.TextMode = TextBox.TextBoxMode.Email;
                    break;
                case "phone":
					ret.TextMode = TextBox.TextBoxMode.Tel;
					break;
                case "number":
                    ret.TextMode = TextBox.TextBoxMode.Number;
                    break;
                case "search":
					ret.TextMode = TextBox.TextBoxMode.Search;
					break;
				case "url":
					ret.TextMode = TextBox.TextBoxMode.Url;
					break;
				case "datetime":
					ret.TextMode = TextBox.TextBoxMode.DateTime;
					break;
				case "date":
					ret.TextMode = TextBox.TextBoxMode.Date;
					break;
				case "month":
					ret.TextMode = TextBox.TextBoxMode.Month;
					break;
				case "week":
					ret.TextMode = TextBox.TextBoxMode.Week;
					break;
				case "time":
					ret.TextMode = TextBox.TextBoxMode.Time;
					break;
				case "datetimelocal":
					ret.TextMode = TextBox.TextBoxMode.DateTimeLocal;
					break;
				case "range":
					ret.TextMode = TextBox.TextBoxMode.Range;
					break;
				case "color":
					ret.TextMode = TextBox.TextBoxMode.Color;
					break;
				}
			}
			
			if (ShouldHandleEvent("ontextchanged", node))
			{
				Node codeNode = node["ontextchanged"].Clone();
				ret.TextChanged += delegate(object sender2, EventArgs e2)
				{
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onescpressed", node))
			{
				Node codeNode = node["onescpressed"].Clone();
				ret.EscKey += delegate(object sender2, EventArgs e2)
				{
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            if (ShouldHandleEvent("onenterpressed", node))
            {
                Node codeNode = node["onenterpressed"].Clone();
                ret.EnterPressed += delegate(object sender2, EventArgs e2)
                {
                    FillOutEventInputParameters(codeNode, sender2);
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

            TextBox ctrl = FindControl<TextBox>(Ip(e.Params));

			if (ctrl != null)
			{
                ctrl.Value = Ip(e.Params)["value"].Get<string>();
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

            TextBox ctrl = FindControl<TextBox>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Value;
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

            TextBox ctrl = FindControl<TextBox>(Ip(e.Params));

			if (ctrl != null)
			{
				ctrl.Select();
			}
		}

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a text box input type of web control.&nbsp;&nbsp;
a text box is a web control where the end user can type in 
text to submit in a form.&nbsp;&nbsp;text-box only supports 
one line of text, without carriage return.&nbsp;&nbsp;use 
[text-area] if you need multiple lines of text</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-box"]);
            node["magix.forms.create-web-part"]["controls"]["text-box"]["place-holder"].Value = "shadow text ...";
            node["magix.forms.create-web-part"]["controls"]["text-box"]["capitalize"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["correct"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["complete"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["disabled"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["max"].Value = 25;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["value"].Value = "hello world";
            node["magix.forms.create-web-part"]["controls"]["text-box"]["mode"].Value = "normal|phone|search|url|email|datetime|date|month|week|time|datetimelocal|number|range|color|password";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for text box</strong></p><p>[place-holder] 
is shadow text, only visible when input area is empty</p><p>
[value] is what text the control shall have, or currently have 
been changed to.&nbsp;&nbsp;this is the property which is changed 
or retrieved when you invoke the [magix.forms.set-value] and 
the [magix.forms.get-value] for your web control</p><p>[key] 
is the keyboard shortcut.&nbsp;&nbsp;how to invoke the keyboard 
shortcut is different from system to system, but on a windows 
system, you normally invoke the keyboard shortcut with 
alt+shift+your-key.&nbsp;&nbsp;if you have for instance 's' as 
your keyboard shortcut, then the end user will have to click 
shift+alt+s at the same time to invoke the keyboard shortcut 
for your web control</p><p>[disabled] enables or disables the 
web control.&nbsp;&nbsp;this can be changed or retrieved after 
the button is created by invoking the [magix.forms.set-enabled] 
or [magix.forms.get-enabled] active events.&nbsp;&nbsp;legal 
values are true and false</p><p>[capitalize] will automatically 
correctly capitalize entities, as if you are typing text</p>
<p>[correct] will automatically suggest corrections for typos</p>
[complete] will suggest previously typed values for you, and 
attempt to automatically complete the value</p><p>[max] is 
maximum number of characters in web control</p><p>[mode] 
determines the mode of the text box.&nbsp;&nbsp;this depends 
upon the type of input you wish that your text box shall be 
taking.&nbsp;&nbsp;if you want the user to type in an email 
address for instance, then use 'email' as the value of this 
property</p>";
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((TextBox)that).Value;
		}
	}
}

