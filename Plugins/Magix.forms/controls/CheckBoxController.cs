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
	 * check box
	 */
    internal sealed class CheckBoxController : BaseWebControlFormElementController
	{
		/*
		 * creates check-box
		 */
		[ActiveEvent(Name = "magix.forms.controls.check-box")]
		private void magix_forms_controls_check_box(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			CheckBox ret = new CheckBox();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Value as Node;

            if (node.ContainsValue("checked"))
				ret.Checked = node["checked"].Get<bool>();

			if (ShouldHandleEvent("oncheckedchanged", node))
			{
				Node codeNode = node["oncheckedchanged"].Clone();
				ret.CheckedChanged += delegate(object sender2, EventArgs e2)
				{
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.check-box-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.check-box-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["check-box"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.check-box-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["check-box"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.check-box-sample-end]");
		}
	}
}

