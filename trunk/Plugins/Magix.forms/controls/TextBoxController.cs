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
	 * contains the textbox control
	 */
    public class TextBoxController : BaseWebControlFormElementInputTextController
	{
		/*
		 * creates a text-box
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-box")]
		public void magix_forms_controls_text_box(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			TextBox ret = new TextBox();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Value as Node;
			if (node.Contains("autocapitalize") && 
			    node["autocapitalize"].Value != null)
				ret.AutoCapitalize = node["autocapitalize"].Get<bool>();

			if (node.Contains("autocorrect") && 
			    node["autocorrect"].Value != null)
				ret.AutoCorrect = node["autocorrect"].Get<bool>();

			if (node.Contains("autocomplete") &&
                node["autocomplete"].Value != null)
                ret.AutoComplete = node["autocomplete"].Get<bool>();

			if (node.Contains("maxlength") &&
                node["maxlength"].Value != null)
                ret.MaxLength = node["maxlength"].Get<int>();

			if (node.Contains("type") && 
				node["type"].Value != null)
			{
				switch(node["type"].Get<string>())
				{
				case "normal":
					ret.Type = TextBox.TextBoxType.Text;
					break;
                case "password":
                    ret.Type = TextBox.TextBoxType.Password;
                    break;
                case "email":
                    ret.Type = TextBox.TextBoxType.Email;
                    break;
                case "phone":
					ret.Type = TextBox.TextBoxType.Tel;
					break;
                case "number":
                    ret.Type = TextBox.TextBoxType.Number;
                    break;
                case "search":
					ret.Type = TextBox.TextBoxType.Search;
					break;
				case "url":
					ret.Type = TextBox.TextBoxType.Url;
					break;
				case "datetime":
					ret.Type = TextBox.TextBoxType.DateTime;
					break;
				case "date":
					ret.Type = TextBox.TextBoxType.Date;
					break;
				case "month":
					ret.Type = TextBox.TextBoxType.Month;
					break;
				case "week":
					ret.Type = TextBox.TextBoxType.Week;
					break;
				case "time":
					ret.Type = TextBox.TextBoxType.Time;
					break;
				case "datetimelocal":
					ret.Type = TextBox.TextBoxType.DateTimeLocal;
					break;
				case "range":
					ret.Type = TextBox.TextBoxType.Range;
					break;
				case "color":
					ret.Type = TextBox.TextBoxType.Color;
					break;
				}
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

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspect(node["inspect"], @"creates a text-box type of web control

a text-box is a web control where the end user can type in text to submit in a form.  
text-box only supports single lines of input text, without carriage return.  use 
text-area if you need multiple lines of text");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-box"]);
            node["magix.forms.create-web-part"]["controls"]["text-box"]["autocapitalize"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["autocorrect"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["autocomplete"].Value = false;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["maxlength"].Value = 25;
            node["magix.forms.create-web-part"]["controls"]["text-box"]["type"].Value = "normal";
            AppendInspect(node["inspect"], @"[autocapitalize] will automatically 
capitalize the first letter, as you are typing into the tex-box

[autocorrect] will automatically suggest corrections for typing errors, as you are 
typing into the text-box

[autocomplete] will suggest previously typed values for you, and attempt to 
automatically complete the value on the end user's behalf

[maxlength] is the maximum number of characters in the text-box

[type] determines the mode of the text-box.  this depends upon the type of 
input you wish that your text box shall be accepting.  if you want the user 
to type in an email address for instance, then use 'email' as the value of this 
property.  legal values for type is normal, phone, search, url, email, datetime, 
date, month, week, time, datetimelocal, number, range, color and password", true);
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

