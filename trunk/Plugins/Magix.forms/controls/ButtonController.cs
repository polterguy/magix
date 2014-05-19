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
    public class ButtonController : BaseWebControlFormElementTextController
	{
		/*
		 * creates button widget
		 */
		[ActiveEvent(Name = "magix.forms.controls.button")]
		public void magix_forms_controls_button(object sender, ActiveEventArgs e)
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
            node["inspect"].Value = @"
<p>creates a button input type of web control.&nbsp;&nbsp;
the button is probably one of the most commonly used web control, 
both in magix, and on the web in general.&nbsp;&nbsp;it renders 
as &lt;input type='button' ... /&gt;.&nbsp;&nbsp;buttons 
are clickable objects, and logically similar to [link-button], 
though rendered differently.&nbsp;&nbsp;although virtually 
anything can be a clickable object in magix, it is considered to 
be more polite to use buttons and link-buttons as your clickable 
objects.&nbsp;&nbsp;first of all, since these web controls are 
recognized by screen readers and such.&nbsp;&nbsp;secondly, because 
it keeeps the semantic parts of your website more correct</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["button"]);
            node["magix.forms.create-web-part"]["controls"]["button"]["value"].Value = "hello world";
            node["magix.forms.create-web-part"]["controls"]["button"]["key"].Value = "C";
            node["magix.forms.create-web-part"]["controls"]["button"]["disabled"].Value = false;
            node["inspect"].Value += node["inspect"].Value + @"
<p><strong>properties for button</strong></p>[value] is the visible 
text of your button.&nbsp;&nbsp;this is the property which is 
changed or retrieved when you invoke the [magix.forms.set-value] 
and the [magix.forms.get-value] for your web control</p><p>[key] 
is the keyboard shortcut.&nbsp;&nbsp;how to invoke the keyboard 
shortcut is different from system to system, but on a windows 
system, you normally invoke the keyboard shortcut with 
alt+shift+your-key.&nbsp;&nbsp;if you have for instance 's' 
as your keyboard shortcut, then the end user will have to click 
shift+alt+s at the same time to invoke the keyboard shortcut 
for your web control</p><p>[disabled] enables or disables the 
web control.&nbsp;&nbsp;this can be changed or retrieved after the 
button is created by invoking the [magix.forms.set-enabled] 
or [magix.forms.get-enabled] active events.&nbsp;&nbsp;legal 
values are true and false</p>";
		}
	}
}

