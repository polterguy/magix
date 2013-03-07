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
	 * contains the label control
	 */
	public class LabelCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.label")]
		public void magix_forms_controls_label(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Label ret = new Label();

			FillOutParameters(node, ret);

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("tag") && 
			    !string.IsNullOrEmpty(node["tag"].Get<string>()))
				ret.Tag = node["tag"].Get<string>();

			if (node.Contains("for") && 
			    !string.IsNullOrEmpty(node["for"].Get<string>()))
				ret.For = node["for"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a label type of web control.&nbsp;&nbsp;
labels are useful for displaying text, but can also be attached to
[check] and [radio] controls, by setting [tag] to label, and [for] to id if your [check] 
or [radio] control.&nbsp;&nbsp;use [text] to change visible text of label, [tag] to 
change html tag to render control, [for] in combination with [tag] being label, to 
point to a [radio] or [check] control, to associate clicking of the label with 
changing state of radio or check control";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["label"]["text"].Value = "hello world";
			node["controls"]["label"]["tag"].Value = "p";
			node["controls"]["label"]["for"].Value = null;
			base.Inspect(node["controls"]["label"]);
		}
	}
}

