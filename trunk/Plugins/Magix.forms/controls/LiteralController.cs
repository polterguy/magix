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
    internal sealed class LiteralController : ActiveController
	{
		/*
		 * creates the literal control
		 */
		[ActiveEvent(Name = "magix.forms.controls.literal")]
		private void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			LiteralControl ret = new LiteralControl();

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("text"))
				ret.Text = node["text"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		private void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.literal-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.literal-sample-start]");
		}
	}
}

