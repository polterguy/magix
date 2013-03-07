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
	 * contains the textbox control
	 */
	public class TextBoxCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-box")]
		public void magix_forms_controls_text_box(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
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

			if (node.Contains("max") && 
			    node["max"].Value != null)
				ret.MaxLength = node["max"].Get<int>();

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
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
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["text-box"]["place-holder"].Value = "shadow text ...";
			node["controls"]["text-box"]["capitalize"].Value = false;
			node["controls"]["text-box"]["correct"].Value = true;
			node["controls"]["text-box"]["complete"].Value = false;
			node["controls"]["text-box"]["max"].Value = 25;
			node["controls"]["text-box"]["text"].Value = "hello world";
			base.Inspect(node["controls"]["text-box"]);
		}
	}
}

