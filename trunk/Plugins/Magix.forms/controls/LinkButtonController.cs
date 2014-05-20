
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
	 * contains the link button control
	 */
    public class LinkButtonController : BaseWebControlFormElementTextController
	{
		/*
		 * creates link button
		 */
		[ActiveEvent(Name = "magix.forms.controls.link-button")]
		public void magix_forms_controls_link_button(object sender, ActiveEventArgs e)
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
            if (node.Contains("value") && 
			    !string.IsNullOrEmpty(node["value"].Get<string>()))
				ret.Value = node["value"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspect(node["inspect"], @"creates a link-button type of web control

a link-button looks, and renders, like a [hyperlink], but acts like a [button]. the 
link-button is probably one of the most commonly used web control, both in magix, and 
on the web in general.  it renders as &lt;a href='...'&gt;anchor text&lt;/a&gt;.  the 
link-button is a clickable object, and logically similar to [button], though rendered 
differently.  although virtually anything can be a clickable object in magix, it is 
often more polite to use buttons and link-buttons as your clickable objects.  first of 
all, since these web controls are recognized by screen readers and such.  secondly, 
because it keeps the semantic parts of your website more correct");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["link-button"]);
            node["magix.forms.create-web-part"]["controls"]["link-button"]["value"].Value = "hello world";
            AppendInspect(node["inspect"], @"[value] is the visible text, or anchor text.  
this is the property which is changed or retrieved when you invoke the [magix.forms.set-
value] and the [magix.forms.get-value] for your link-button", true);
		}
	}
}

