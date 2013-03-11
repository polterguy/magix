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
	public class ImageCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.image")]
		public void magix_forms_controls_image(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Image ret = new Image();

			FillOutParameters(node, ret);

			if (node.Contains("src") && node["src"].Value != null)
				ret.ImageUrl = node["src"].Get<string>();

			if (node.Contains("alt") && node["alt"].Value != null)
				ret.AlternateText = node["alt"].Get<string>();

			if (node.Contains("key") && node["key"].Value != null)
				ret.AccessKey = node["key"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates an image type of web control.&nbsp;&nbsp;
useful showing images, where you need dynamic behavior, such as onclick event handlers,
or changing the image src during execution.&nbsp;&nbsp;use [src] to set image url.&nbsp;&nbsp;
use [alt] to set alternating text.&nbsp;&nbsp;use [key] to set keyboard shortcut";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["image"]["src"].Value = "media/images/magix-logo.png";
			node["controls"]["image"]["alt"].Value = "alternative text";
			node["controls"]["image"]["key"].Value = "C";
			base.Inspect(node["controls"]["image"]);
		}
	}
}

