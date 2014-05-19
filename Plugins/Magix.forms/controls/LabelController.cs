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
	 * contains the label control
	 */
    public class LabelController : AttributeControlController
	{
		/*
		 * creates label control
		 */
		[ActiveEvent(Name = "magix.forms.controls.label")]
		public void magix_forms_controls_label(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Label ret = new Label();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Value as Node;
            if (node.Contains("value") && 
			    !string.IsNullOrEmpty(node["value"].Get<string>()))
				ret.Value = node["value"].Get<string>();

			if (node.Contains("tag") && 
			    !string.IsNullOrEmpty(node["tag"].Get<string>()))
				ret.Tag = node["tag"].Get<string>();

			if (node.Contains("for") && 
			    !string.IsNullOrEmpty(node["for"].Get<string>()))
				ret.For = node["for"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a label type of web control.&nbsp;&nbsp;
labels are useful for displaying text, but can also 
be attached to checkboxes and radiobuttons, by setting 
the [tag] property to 'label', and the [for] property 
to the id if your checkbox or radiobutton</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["label"]);
            node["magix.forms.create-web-part"]["controls"]["label"]["value"].Value = "hello world";
            node["magix.forms.create-web-part"]["controls"]["label"]["tag"].Value = "p|div|span|etc";
            node["magix.forms.create-web-part"]["controls"]["label"]["for"].Value = "another-control-id";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for label</strong></p><p>[value] 
sets the visible text of label.&nbsp;&nbsp;this is the 
property which is changed or retrieved when you invoke 
the [magix.forms.set-value] and the [magix.forms.get-value] 
for your web control</p><p>[tag] sets html tag to render 
panel as.&nbsp;&nbsp;you can change this to any html tag 
you wish for the control to be rendered within, such as 
p, div, label, span or address, etc</p><p>[for] changes 
the associated radiobutton or checkbox the label is 
pointing to.&nbsp;&nbsp;if the label is associated with 
a checkbox or a radiobutton, then the checked state of 
that radiobutton or checkbox will change if the user clicks 
the label.&nbsp;&nbsp;remember to also set the [tag] to 
'label' if you use this feature, since only html labels 
will actually be associated with radiobuttons or checkboxes 
by the browser</p>";
		}
	}
}

