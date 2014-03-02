/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * hyper link
	 */
	public class HyperLinkCore : BaseWebControlCore
	{
		/**
		 * create hyper link
		 */
		[ActiveEvent(Name = "magix.forms.controls.hyperlink")]
		public void magix_forms_controls_hyperlink(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			HyperLink ret = new HyperLink();

			FillOutParameters(node, ret);

			if (node.Contains("text") && node["text"].Value != null)
				ret.Text = node["text"].Get<string>();

			if (node.Contains("url") && node["url"].Value != null)
			{
				string url = node["url"].Get<string>();

				if (url.StartsWith("~"))
					url = url.Replace("~", GetApplicationBaseUrl());

				ret.URL = url;
			}

			if (node.Contains("target") && node["target"].Value != null)
				ret.Target = node["target"].Get<string>();

			if (node.Contains("key") && node["key"].Value != null)
				ret.AccessKey = node["key"].Get<string>();

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		/**
		 * sets value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

            if (!Ip(e.Params).Contains("value"))
				throw new ArgumentException("set-value needs [value]");

            HyperLink ctrl = FindControl<HyperLink>(Ip(e.Params));

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
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

            HyperLink ctrl = FindControl<HyperLink>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Text;
			}
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a hyperlink input type of web control.&nbsp;&nbsp;
hyperlinks points to other urls, either locally, or externally.&nbsp;&nbsp;
use [text] to change readable text, [url] to set the url of the document to link to, 
[target] to either _blank, _new, ro any id you wish to reuse.&nbsp;&nbsp;[key] is 
keyboard shortcut";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["hyperlink"]["text"].Value = "anchor text of hyperlink";
			node["controls"]["hyperlink"]["url"].Value = "http://google.com";
			node["controls"]["hyperlink"]["target"].Value = "_blank";
			node["controls"]["hyperlink"]["key"].Value = "C";
			base.Inspect(node["controls"]["hyperlink"]);
		}
	}
}

