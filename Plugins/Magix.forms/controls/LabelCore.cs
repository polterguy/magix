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
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a label type of web control.&nbsp;&nbsp;
labels are useful for displaying text, but can also be attached to
[check] and [radio] controls, by setting [tag] to label, and [for] to id if your [check] 
or [radio] control.&nbsp;&nbsp;use [text] to change visible text of label, [tag] to 
change html tag to render control, [for] in combination with [tag] being label, to 
point to a [radio] or [check] control, to associate clicking of the label with 
changing state of radio or check control";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["label"]["text"].Value = "hello world";
			node["controls"]["label"]["tag"].Value = "p|div|span|etc";
			node["controls"]["label"]["for"].Value = "another-control-id";
			base.Inspect(node["controls"]["label"]);
		}
	}
}

