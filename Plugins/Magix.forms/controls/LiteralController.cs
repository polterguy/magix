/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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

			LiteralControl ctrl = new LiteralControl();

            Node node = ip["_code"].Get<Node>();

            string idPrefix = "";
            if (ip.ContainsValue("id-prefix"))
                idPrefix = ip["id-prefix"].Get<string>();

            if (node.ContainsValue("id"))
                ctrl.ID = idPrefix + node["id"].Get<string>();
            else if (node.Value != null)
                ctrl.ID = idPrefix + node.Get<string>();

            if (node.ContainsValue("text"))
				ctrl.Text = node["text"].Get<string>();

            ip["_ctrl"].Value = ctrl;
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

