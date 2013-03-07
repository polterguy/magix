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
	 */
	public class HyperLinkCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.hyperlink")]
		public void magix_forms_controls_hyperlink(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			HyperLink ret = new HyperLink();

			FillOutParameters(node, ret);

			if (node.Contains("text") && node["text"].Value != null)
				ret.Text = node["text"].Get<string>();

			if (node.Contains("url") && node["url"].Value != null)
				ret.URL = node["url"].Get<string>();

			if (node.Contains("target") && node["target"].Value != null)
				ret.Target = node["target"].Get<string>();

			if (node.Contains("key") && node["key"].Value != null)
				ret.AccessKey = node["key"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a hyperlink input type of web control.&nbsp;&nbsp;
hyperlinks points to other urls, either locally, or externally.&nbsp;&nbsp;
use [text] to change readable text, [url] to set the url of the document to link to, 
[target] to either _blank, _new, ro any id you wish to reuse.&nbsp;&nbsp;[key] is 
keyboard shortcut";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["hyperlink"]["text"].Value = "anchor text of hyperlink";
			node["controls"]["hyperlink"]["url"].Value = "http://google.com";
			node["controls"]["hyperlink"]["target"].Value = "_blank";
			node["controls"]["hyperlink"]["key"].Value = "C";
			base.Inspect(node["controls"]["hyperlink"]);
		}
	}
}

