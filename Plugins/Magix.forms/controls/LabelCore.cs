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
	 * contains the label control
	 */
	public class LabelCore : BaseWebControlCore
	{
		/**
		 * creates label control
		 */
		[ActiveEvent(Name = "magix.forms.controls.label")]
		public void magix_forms_controls_label(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			Label ret = new Label();

			FillOutParameters(node, ret);

			if (node.Contains("text") && 
			    !string.IsNullOrEmpty(node["text"].Get<string>()))
				ret.Text = node["text"].Get<string>();

			if (node.Contains("tag") && 
			    !string.IsNullOrEmpty(node["tag"].Get<string>()))
				ret.Tag = node["tag"].Get<string>();

			if (node.Contains("for") && 
			    !string.IsNullOrEmpty(node["for"].Get<string>()))
				ret.For = node["for"].Get<string>();

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

            Label ctrl = FindControl<Label>(Ip(e.Params));

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

            Label ctrl = FindControl<Label>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Text;
			}
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
            node["magix.forms.create-web-part"]["controls"]["label"]["text"].Value = "hello world";
            node["magix.forms.create-web-part"]["controls"]["label"]["tag"].Value = "p|div|span|etc";
            node["magix.forms.create-web-part"]["controls"]["label"]["for"].Value = "another-control-id";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for label</strong></p><p>[text] 
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

