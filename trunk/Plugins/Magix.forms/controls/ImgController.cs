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
	 * image control
	 */
    public class ImgController : BaseWebControlFormElementController
	{
		/*
		 * creates image control
		 */
		[ActiveEvent(Name = "magix.forms.controls.img")]
		public void magix_forms_controls_image(object sender, ActiveEventArgs e)
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
            if (node.Contains("src") && node["src"].Value != null)
				ret.Src = node["src"].Get<string>();

			if (node.Contains("alt") && node["alt"].Value != null)
				ret.Alt = node["alt"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

        protected override void Inspect(Node node)
		{
            AppendInspect(node["inspect"], @"creates an image type of web control

the image web control is useful for showing images, where you need dynamic behavior, 
such as onclick event handlers, or the ability to change the image during execution");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["img"]);
            node["magix.forms.create-web-part"]["controls"]["img"]["src"].Value = "media/images/icons/start-button.png";
            node["magix.forms.create-web-part"]["controls"]["img"]["alt"].Value = "alternative text";
            node["magix.forms.create-web-part"]["controls"]["img"]["key"].Value = "C";
            AppendInspect(node["inspect"], @"[src] sets the image url.  this is the 
property which is changed or retrieved when you invoke the [magix.forms.set-value] 
and the [magix.forms.get-value] for your image

[alt] sets alternating text.  the alternating text will be displayed while the image 
is loading, or if the image cannot be found.  in addition, the alt text will also be 
the descriptive text that screen readers will read up loud to the end user", true);
		}
	}
}

