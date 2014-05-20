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
	/*
	 * literal control
	 */
	public class LiteralController : ActiveController
	{
		/*
		 * creates the literal control
		 */
		[ActiveEvent(Name = "magix.forms.controls.literal")]
		public void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			LiteralControl ret = new LiteralControl();
            Node node = ip["_code"].Get<Node>();
            if (node.Contains("text"))
				ret.Text = node["text"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected void Inspect(Node node)
		{
			AppendInspect(node["inspect"], @"creates a literal type of web control

put any html into [text] node, to render exactly the html you wish");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            node["magix.forms.create-web-part"]["controls"]["literal"].Value = null;
            node["magix.forms.create-web-part"]["controls"]["literal"]["text"].Value = "&lt;h1&gt;hello&lt;/h1&gt;";
		}
	}
}

