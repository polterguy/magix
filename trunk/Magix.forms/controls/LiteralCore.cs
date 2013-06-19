/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI.WebControls;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 */
	public class LiteralCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.literal")]
		public void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Literal ret = new Literal();
			if (node.Contains("text"))
				ret.Text = node["text"].Get<string>();

			e.Params["_ctrl"].Value = ret;
		}

		protected void Inspect(Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a literal type of web control.&nbsp;&nbsp;
put any html into [text] node, to render exact html as web control";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["literal"].Value = null;
			node["controls"]["literal"]["text"].Value = "&lt;h1&gt;hello&lt;/h1&gt;";
		}
	}
}

