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
	 */
	public class HiddenFieldCore : BaseControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.hidden")]
		public void magix_forms_controls_hidden(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			HiddenField ret = new HiddenField();

			FillOutParameters(node, ret);

			if (node.Contains("value") && node["value"].Value != null)
				ret.Value = node["value"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a hidden field input type of web control.&nbsp;&nbsp;
hidden fields are invisible, but can store any data as test in their [value] node";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["hidden"]["value"].Value = "whatever you wish to put in as value";
			base.Inspect(node["controls"]["hidden"]);
		}
	}
}

