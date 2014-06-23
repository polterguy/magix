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
	 * image control
	 */
    internal sealed class ImgController : BaseWebControlFormElementController
	{
		/*
		 * creates image control
		 */
		[ActiveEvent(Name = "magix.forms.controls.img")]
		private void magix_forms_controls_image(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Img ret = new Img();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Value as Node;

            if (node.ContainsValue("src"))
				ret.Src = node["src"].Get<string>();

            if (node.ContainsValue("alt"))
				ret.Alt = node["alt"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

        protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.img-dox-start].value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.img-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["img"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.img-dox-end].value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["img"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.img-sample-end]");
		}
	}
}

