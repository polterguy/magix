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
	 * contains the wysiwyg control
	 */
	public class WysiwygCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.wysiwyg")]
		public void magix_forms_controls_wysiwyg(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Wysiwyg ret = new Wysiwyg();

			FillOutParameters(node, ret);

			if (node.Contains ("text") && 
			    !string.IsNullOrEmpty (node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains ("editor-css-file") && 
			    !string.IsNullOrEmpty (node["editor-css-file"].Get<string>()))
				ret.EditorCssFile = node["editor-css-file"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a wysiwyg input type of web control.&nbsp;&nbsp;
[text] is the visible text.&nbsp;&nbso;not thread safe";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["wysiwyg"]["text"].Value = "hello world";
			node["controls"]["wysiwyg"]["has-bold"].Value = true;
			node["controls"]["wysiwyg"]["has-italic"].Value = true;
			node["controls"]["wysiwyg"]["has-unorderedlist"].Value = true;
			node["controls"]["wysiwyg"]["has-orderedlist"].Value = true;
			node["controls"]["wysiwyg"]["has-createlink"].Value = true;
			node["controls"]["wysiwyg"]["has-insertimage"].Value = true;
			node["controls"]["wysiwyg"]["has-h1"].Value = true;
			node["controls"]["wysiwyg"]["has-h2"].Value = true;
			node["controls"]["wysiwyg"]["has-h3"].Value = false;
			node["controls"]["wysiwyg"]["has-h4"].Value = false;
			node["controls"]["wysiwyg"]["has-h5"].Value = false;
			node["controls"]["wysiwyg"]["has-h6"].Value = false;
			node["controls"]["wysiwyg"]["has-forecolor"].Value = true;
			node["controls"]["wysiwyg"]["has-insertspeech"].Value = true;
			node["controls"]["wysiwyg"]["has-showhtml"].Value = true;
			node["controls"]["wysiwyg"]["editor-css-file"].Value = "media/grid/main.css";
			base.Inspect(node["controls"]["button"]);
		}
	}
}


















