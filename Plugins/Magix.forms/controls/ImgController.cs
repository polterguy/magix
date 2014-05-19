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
            node["inspect"].Value = @"
<p>creates an image type of web control.&nbsp;&nbsp;
the image web control is useful for showing images, 
where you need dynamic behavior, such as onclick 
event handlers, or the ability to change the image 
shown during execution</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["img"]);
            node["magix.forms.create-web-part"]["controls"]["img"]["src"].Value = "media/grid/start-button.png";
            node["magix.forms.create-web-part"]["controls"]["img"]["alt"].Value = "alternative text";
            node["magix.forms.create-web-part"]["controls"]["img"]["key"].Value = "C";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for hyperlink</strong></p><p>
[src] sets the image url.&nbsp;&nbsp;this is the property 
which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] 
for your web control</p><p>[alt] sets alternating text.
&nbsp;&nbsp;the alternating text will be displayed while 
image is loading, or if the image cannot be found.
&nbsp;&nbsp;in addition, the [alt] teext will also be 
the descriptive text that screen readers will read up 
loud to the user</p><p>[key] is the keyboard shortcut.
&nbsp;&nbsp;how to invoke the keyboard shortcut is 
different from system to system, but on a windows 
system, you normally invoke the keyboard shortcut with 
alt+shift+your-key.&nbsp;&nbsp;if you have for instance 
's' as your keyboard shortcut, then the end user will 
have to click shift+alt+s at the same time to invoke 
the keyboard shortcut for your web control</p>";
		}
	}
}

