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
	 * contains the label control
	 */
    internal sealed class LabelController : AttributeControlController
	{
		/*
		 * creates label control
		 */
		[ActiveEvent(Name = "magix.forms.controls.label")]
		private void magix_forms_controls_label(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Label ret = new Label();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Value as Node;
            if (node.Contains("value") && 
			    !string.IsNullOrEmpty(node["value"].Get<string>()))
				ret.Value = node["value"].Get<string>();

			if (node.Contains("tag") && 
			    !string.IsNullOrEmpty(node["tag"].Get<string>()))
				ret.Tag = node["tag"].Get<string>();

			if (node.Contains("for") && 
			    !string.IsNullOrEmpty(node["for"].Get<string>()))
				ret.For = node["for"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.label-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.label-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["label"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.label-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["label"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.label-sample-end]");
		}
	}
}

