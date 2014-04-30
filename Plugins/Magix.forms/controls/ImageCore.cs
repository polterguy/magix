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
	/**
	 * image control
	 */
	public class ImageCore : BaseWebControlCore
	{
		/**
		 * creates image control
		 */
		[ActiveEvent(Name = "magix.forms.controls.image")]
		public void magix_forms_controls_image(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			Image ret = new Image();

			FillOutParameters(node, ret);

			if (node.Contains("src") && node["src"].Value != null)
				ret.ImageUrl = node["src"].Get<string>();

			if (node.Contains("alt") && node["alt"].Value != null)
				ret.AlternateText = node["alt"].Get<string>();

			if (node.Contains("key") && node["key"].Value != null)
				ret.AccessKey = node["key"].Get<string>();

            Ip(e.Params)["_ctrl"].Value = ret;
		}

        /**
         * sets value
         */
        [ActiveEvent(Name = "magix.forms.set-value")]
        public void magix_forms_set_value(object sender, ActiveEventArgs e)
        {
            if (ShouldInspectOrHasInspected(e.Params))
            {
                e.Params["inspect"].Value = "sets the value property of the control";
                return;
            }

            if (!Ip(e.Params).Contains("value"))
                throw new ArgumentException("set-value needs [value]");

            Image ctrl = FindControl<Image>(Ip(e.Params));

            if (ctrl != null)
            {
                ctrl.ImageUrl = Ip(e.Params)["value"].Get<string>();
            }
        }

        /**
         * returns value
         */
        [ActiveEvent(Name = "magix.forms.get-value")]
        public void magix_forms_get_value(object sender, ActiveEventArgs e)
        {
            if (ShouldInspectOrHasInspected(e.Params))
            {
                e.Params["inspect"].Value = "returns the value property of the control";
                return;
            }

            Image ctrl = FindControl<Image>(Ip(e.Params));

            if (ctrl != null)
            {
                Ip(e.Params)["value"].Value = ctrl.ImageUrl;
            }
        }

        protected override void Inspect(Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
            node["inspect"].Value = node["inspect"].Get<string>("") + @"<p>creates an image type of web control.&nbsp;&nbsp;
the image web control is useful for showing images, where you need dynamic behavior, such as onclick event handlers,
or the ability to change the image shown during execution</p>";
            base.Inspect(node["controls"]["image"]);
            node["inspect"].Value = node["inspect"].Get<string>("") + @"<p><strong>properties for hyperlink</strong></p>
<p>[src] sets the image url.&nbsp;&nbsp;this is the property which is changed or retrieved 
when you invoke the [magix.forms.set-value] and the [magix.forms.get-value] for your web control</p>
<p>[alt] sets alternating text.&nbsp;&nbsp;the alternating text will be displayed while image is loading, or 
if the image cannot be found.&nbsp;&nbsp;in addition, the [alt] teext will also be the descriptive text 
that screen readers will read up loud to the user</p><p>[key] is the keyboard shortcut.&nbsp;&nbsp;
how to invoke the keyboard shortcut is different from system to system, but on a windows system, you normally 
invoke the keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;if you have for instance 's' as your keyboard 
shortcut, then the end user will have to click shift+alt+s at the same time to invoke the keyboard shortcut 
for your web control</p>";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["image"]["src"].Value = "media/images/magix-logo.png";
			node["controls"]["image"]["alt"].Value = "alternative text";
			node["controls"]["image"]["key"].Value = "C";
		}
	}
}

