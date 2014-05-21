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
	/*
	 * hyper link
	 */
    internal sealed class HyperLinkController : BaseWebControlFormElementController
	{
		/*
		 * creates a hyper link
		 */
		[ActiveEvent(Name = "magix.forms.controls.hyperlink")]
		private void magix_forms_controls_hyperlink(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			HyperLink ret = new HyperLink();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("value"))
				ret.Value = node["value"].Get<string>();

            if (node.ContainsValue("href"))
				ret.Href = node["href"].Get<string>().Replace("~", GetApplicationBaseUrl());

            if (node.ContainsValue("target"))
				ret.Target = node["target"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.hyperlink-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.hyperlink-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["hyperlink"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.hyperlink-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["hyperlink"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.hyperlink-sample-end]");
		}
	}
}

