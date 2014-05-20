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
	 * contains the button control
	 */
    internal sealed class ButtonController : BaseWebControlFormElementTextController
	{
		/*
		 * creates button widget
		 */
		[ActiveEvent(Name = "magix.forms.controls.button")]
		private void magix_forms_controls_button(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Button ret = new Button();
            FillOutParameters(e.Params, ret);
            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.button-dox].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.button-sample]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["button"]);
            node["magix.forms.create-web-part"]["controls"]["button"]["onclick"].UnTie();
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["button"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.button-sample-end]");
        }
	}
}

