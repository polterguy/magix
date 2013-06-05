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

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("has-bold") && 
			    !string.IsNullOrEmpty(node["has-bold"].Get<string>()))
				ret.HasBold = node["has-bold"].Get<bool>();

			if (node.Contains("has-italic") && 
			    !string.IsNullOrEmpty(node["has-italic"].Get<string>()))
				ret.HasItalic = node["has-italic"].Get<bool>();

			if (node.Contains("has-unorderedlist") && 
			    !string.IsNullOrEmpty(node["has-unorderedlist"].Get<string>()))
				ret.HasUnorderedList = node["has-unorderedlist"].Get<bool>();

			if (node.Contains("has-orderedlist") && 
			    !string.IsNullOrEmpty(node["has-orderedlist"].Get<string>()))
				ret.HasOrderedList = node["has-orderedlist"].Get<bool>();

			if (node.Contains("has-createlink") && 
			    !string.IsNullOrEmpty(node["has-createlink"].Get<string>()))
				ret.HasCreateLink = node["has-createlink"].Get<bool>();

			if (node.Contains("has-insertimage") && 
			    !string.IsNullOrEmpty(node["has-insertimage"].Get<string>()))
				ret.HasInsertImage = node["has-insertimage"].Get<bool>();

			if (node.Contains("has-h1") && 
			    !string.IsNullOrEmpty(node["has-h1"].Get<string>()))
				ret.HasH1 = node["has-h1"].Get<bool>();

			if (node.Contains("has-h2") && 
			    !string.IsNullOrEmpty(node["has-h2"].Get<string>()))
				ret.HasH2 = node["has-h2"].Get<bool>();

			if (node.Contains("has-h3") && 
			    !string.IsNullOrEmpty(node["has-h3"].Get<string>()))
				ret.HasH3 = node["has-h3"].Get<bool>();

			if (node.Contains("has-h4") && 
			    !string.IsNullOrEmpty(node["has-h4"].Get<string>()))
				ret.HasH4 = node["has-h4"].Get<bool>();

			if (node.Contains("has-h5") && 
			    !string.IsNullOrEmpty(node["has-h5"].Get<string>()))
				ret.HasH5 = node["has-h5"].Get<bool>();

			if (node.Contains("has-h6") && 
			    !string.IsNullOrEmpty(node["has-h6"].Get<string>()))
				ret.HasH6 = node["has-h6"].Get<bool>();

			if (node.Contains("has-forecolor") && 
			    !string.IsNullOrEmpty(node["has-forecolor"].Get<string>()))
				ret.HasForeColor = node["has-forecolor"].Get<bool>();

			if (node.Contains("has-insertspeech") && 
			    !string.IsNullOrEmpty(node["has-insertspeech"].Get<string>()))
				ret.HasInsertSpeech = node["has-insertspeech"].Get<bool>();

			if (node.Contains("has-showhtml") && 
			    !string.IsNullOrEmpty(node["has-showhtml"].Get<string>()))
				ret.HasChangeView = node["has-showhtml"].Get<bool>();

			if (node.Contains("place-holder") && 
			    !string.IsNullOrEmpty(node["place-holder"].Get<string>()))
				ret.PlaceHolder = node["place-holder"].Get<string>();

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
			node["controls"]["wysiwyg"]["place-holder"].Value = "place holder ...";
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
			base.Inspect(node["controls"]["wysiwyg"]);
		}
	}
}


















