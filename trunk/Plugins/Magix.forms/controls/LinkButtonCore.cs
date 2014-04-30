
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
	 * contains the link button control
	 */
	public class LinkButtonCore : FormElementCore
	{
		/**
		 * creates link button
		 */
		[ActiveEvent(Name = "magix.forms.controls.link-button")]
		public void magix_forms_controls_link_button(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			LinkButton ret = new LinkButton();

			FillOutParameters(node, ret);

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("key") && 
			    !string.IsNullOrEmpty(node["key"].Get<string>()))
				ret.AccessKey = node["key"].Get<string>();

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

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

            LinkButton ctrl = FindControl<LinkButton>(Ip(e.Params));

			if (ctrl != null)
			{
                ctrl.Text = Ip(e.Params)["value"].Get<string>();
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

            LinkButton ctrl = FindControl<LinkButton>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Text;
			}
		}

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a link-button type of web control.&nbsp;&nbsp;
a link-button looks, and renders, like a [hyperlink], 
but acts like a [button].&nbsp;&nbsp;the link-button is 
probably one of the most commonly used web control, both 
in magix, and on the web in general.&nbsp;&nbsp;it renders 
as &lt;a href='...'&gt;anchor text&lt;/a&gt;.&nbsp;&nbsp;
the link-button is a clickable object, and logically 
similar to [button], though rendered differently.&nbsp;&nbsp;
although virtually anything can be a clickable object in 
magix, it is often more polite to use buttons and link-buttons 
as your clickable objects.&nbsp;&nbsp;first of all, since 
these web controls are recognized by screen readers and 
such.&nbsp;&nbsp;secondly, because it keeps the semantic 
parts of your website more correct</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["link-button"]);
            node["magix.forms.create-web-part"]["controls"]["link-button"]["text"].Value = "hello world";
            node["magix.forms.create-web-part"]["controls"]["link-button"]["key"].Value = "C";
            node["magix.forms.create-web-part"]["controls"]["link-button"]["enabled"].Value = true;
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for link-button</strong></p><p>[text] 
is the visible text, or anchor text.&nbsp;&nbsp;this is the 
property which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] for 
your web control</p><p>[key] is the keyboard shortcut.
&nbsp;&nbsp;how to invoke the keyboard shortcut is different 
from system to system, but on a windows system, you normally 
invoke the keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;
if you have for instance 's' as your keyboard shortcut, then 
the end user will have to click shift+alt+s at the same time to 
invoke the keyboard shortcut for your web control</p>
<p>[enabled] enables or disables the web control.&nbsp;&nbsp;
this can be changed or retrieved after the button is created by 
invoking the [magix.forms.set-enabled] or [magix.forms.get-enabled] 
active events.&nbsp;&nbsp;legal values are true and false</p>";
		}
	}
}

