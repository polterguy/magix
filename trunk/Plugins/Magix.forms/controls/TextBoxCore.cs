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

			Node node = e.Params["_code"].Value as Node;

			TextBox ret = new TextBox();

			FillOutParameters(node, ret);

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

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

			if (node.Contains("max") && 
			    node["max"].Value != null)
				ret.MaxLength = node["max"].Get<int>();

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("mode") && 
				node["mode"].Value != null)
			{
				switch(node["mode"].Get<string>())
				{
				case "normal":
					ret.TextMode = TextBox.TextBoxMode.Normal;
					break;
				case "phone":
					ret.TextMode = TextBox.TextBoxMode.Phone;
					break;
				case "search":
					ret.TextMode = TextBox.TextBoxMode.Search;
					break;
				case "url":
					ret.TextMode = TextBox.TextBoxMode.Url;
					break;
				case "email":
					ret.TextMode = TextBox.TextBoxMode.Email;
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
				case "number":
					ret.TextMode = TextBox.TextBoxMode.Number;
					break;
				case "range":
					ret.TextMode = TextBox.TextBoxMode.Range;
					break;
				case "color":
					ret.TextMode = TextBox.TextBoxMode.Color;
					break;
				case "password":
					ret.TextMode = TextBox.TextBoxMode.Password;
					break;
				}
			}
			
			if (node.Contains("ontextchanged"))
			{
				Node codeNode = node["ontextchanged"].Clone();

				ret.TextChanged += delegate(object sender2, EventArgs e2)
				{
					TextBox that2 = sender2 as TextBox;
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
					TextBox that2 = sender2 as TextBox;
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

			e.Params["_ctrl"].Value = ret;
		}

		/**
		 * sets text value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

			if (!e.Params.Contains("value"))
				throw new ArgumentException("set-value needs [value]");

			TextBox ctrl = FindControl<TextBox>(e.Params);

			if (ctrl != null)
			{
				ctrl.Text = e.Params["value"].Get<string>();
			}
		}

		/**
		 * returns value
		 */
		[ActiveEvent(Name = "magix.forms.get-value")]
		public void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

			TextBox ctrl = FindControl<TextBox>(e.Params);

			if (ctrl != null)
			{
				e.Params["value"].Value = ctrl.Text;
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

			TextBox ctrl = FindControl<TextBox>(e.Params);

			if (ctrl != null)
			{
				ctrl.Select();
			}
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a text box input type of web control.&nbsp;&nbsp;
a [text-box] is a control where the end user can type in text he wish to submit 
in a form.&nbsp;&nbsp;text-box only supports lines of text, without carriage return.&nbsp;&nbsp;
use [text-area] if you need multiple lines of text.&nbsp;&nbsp;[place-holder] is shadow text, 
only visible when input area is empty.&nbsp;&nbsp;[text] is what text the control 
shall have, or currently have been changed to.&nbsp;&nbsp;[key] is keyboard shortcut 
to active, or give focus to text area.&nbsp;&nbsp;[capitalize] will automatically 
correctly capitalize entities, as if you are typing a name.&nbsp;&nbsp;[correct] will 
automatically suggest corrections for typos.&nbsp;&nbsp;[complete] will suggest 
previously typed values for you, and attempt to automatically complete the value.&nbsp;&nbsp;
[max] is maximum number of characters in text-box.&nbsp;&nbsp;";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["text-box"]["place-holder"].Value = "shadow text ...";
			node["controls"]["text-box"]["capitalize"].Value = false;
			node["controls"]["text-box"]["correct"].Value = true;
			node["controls"]["text-box"]["complete"].Value = false;
			node["controls"]["text-box"]["enabled"].Value = true;
			node["controls"]["text-box"]["max"].Value = 25;
			node["controls"]["text-box"]["text"].Value = "hello world";
			node["controls"]["text-box"]["mode"].Value = "normal|phone|search|url|email|datetime|date|month|week|time|datetimelocal|number|range|color|password";
			base.Inspect(node["controls"]["text-box"]);
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((TextBox)that).Text;
		}
	}
}

