/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * contains the linkbutton control
	 */
	public class LinkButtonCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.link-button")]
		public void magix_forms_controls_link_button(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			LinkButton ret = new LinkButton();

			FillOutParameters(node, ret);

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("key") && 
			    !string.IsNullOrEmpty(node["key"].Get<string>()))
				ret.AccessKey = node["key"].Get<string>();

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a link button type of web control.&nbsp;&nbsp;
a link button looks, and renders, like a [hyperlink], but acts like a [button].&nbsp;&nbsp;
[text] is visible text, or anchor text.&nbsp;&nbsp;[key] is keyboard shortcut.&nbsp;&nbsp;
[enabled] can be true or false";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["link-button"]["text"].Value = "hello world";
			node["controls"]["link-button"]["key"].Value = "C";
			node["controls"]["link-button"]["enabled"].Value = true;
			base.Inspect(node["controls"]["link-button"]);
		}
	}
}

