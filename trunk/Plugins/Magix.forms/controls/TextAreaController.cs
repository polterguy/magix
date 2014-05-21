/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/*
	 * textarea control
	 */
    internal sealed class TextAreaController : BaseWebControlFormElementInputTextController
	{
		/*
		 * creates text area control
		 */
		[ActiveEvent(Name = "magix.forms.controls.text-area")]
		private void magix_forms_controls_text_area(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			TextArea ret = new TextArea();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("rows"))
				ret.Rows = node["rows"].Get<int>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-area-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-area-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["text-area"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-area-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["text-area"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.text-area-sample-end]");
		}
	}
}

