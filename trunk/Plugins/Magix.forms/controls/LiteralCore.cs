/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * literal control
	 */
	public class LiteralCore : ActiveController
	{
		/**
		 * creates the literal control
		 */
		[ActiveEvent(Name = "magix.forms.controls.literal")]
		public void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			LiteralControl ret = new LiteralControl();
			if (node.Contains("text"))
				ret.Text = node["text"].Get<string>();

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		protected void Inspect(Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"<p>creates a literal type of web control.&nbsp;&nbsp;
put any html into [text] node, to render exactly the html you wish</p>
<p><strong>properties for literal</strong></p><p>[text] is the contents of your literal</p>";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["literal"].Value = null;
			node["controls"]["literal"]["text"].Value = "&lt;h1&gt;hello&lt;/h1&gt;";
		}
	}
}

