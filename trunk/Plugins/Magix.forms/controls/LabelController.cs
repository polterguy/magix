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
            AppendInspect(node["inspect"], @"creates a label type of web control

labels are useful for displaying text, but can also be attached to a check-box 
or a radio web control.  do this by setting the [tag] property to 'label', and 
the [for] property to the id if your check-box or radio web control");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["label"]);
            node["magix.forms.create-web-part"]["controls"]["label"]["value"].Value = "hello world";
            node["magix.forms.create-web-part"]["controls"]["label"]["tag"].Value = "p|div|span|etc";
            node["magix.forms.create-web-part"]["controls"]["label"]["for"].Value = "another-control-id";
            AppendInspect(node["inspect"], @"[value] sets the visible text of 
label.  this is the property which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] for your web control

[tag] sets html tag to render panel as.  you can change this to any html tag 
you wish for the control to be rendered with, such as p, div, label, span or 
address, etc

[for] changes the associated radio or check-box web control the label is 
pointing to.  if the label is associated with a check-box or a radio, then 
the checked state of that radio or check-box web control will change if the 
user clicks the label.  remember to also set the [tag] to 'label' if you use 
this feature, since only html labels actually will be associated with radio 
or checkboxes by the browser", true);
		}
	}
}

