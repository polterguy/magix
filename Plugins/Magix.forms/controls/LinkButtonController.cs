
/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/*
	 * contains the link button control
	 */
    internal sealed class LinkButtonController : BaseWebControlFormElementTextController
	{
		/*
		 * creates link button
		 */
		[ActiveEvent(Name = "magix.forms.controls.link-button")]
		private void magix_forms_controls_link_button(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			LinkButton ret = new LinkButton();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("value"))
				ret.Value = node["value"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.link-button-dox-start].value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.link-button-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["link-button"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.link-button-dox-end].value",
                true);
            node["magix.forms.create-web-part"]["controls"]["link-button"]["onclick"].UnTie();
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["link-button"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.link-button-sample-end]");
		}
	}
}

