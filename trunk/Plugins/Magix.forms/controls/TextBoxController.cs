/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
    internal sealed class TextBoxController : BaseWebControlFormElementInputTextController
	{
		/*
		 * creates a text-box
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-box")]
		private void magix_forms_controls_text_box(object sender, ActiveEventArgs e)
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

            if (node.ContainsValue("autocapitalize"))
				ret.AutoCapitalize = node["autocapitalize"].Get<bool>();

            if (node.ContainsValue("autocorrect"))
				ret.AutoCorrect = node["autocorrect"].Get<bool>();

            if (node.ContainsValue("autocomplete"))
                ret.AutoComplete = node["autocomplete"].Get<bool>();

            if (node.ContainsValue("maxlength"))
                ret.MaxLength = node["maxlength"].Get<int>();

            if (node.ContainsValue("type"))
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
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-box-dox-start].value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-box-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-box"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-box-dox-end].value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["text-box"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-box-sample-end]");
		}
	}
}

