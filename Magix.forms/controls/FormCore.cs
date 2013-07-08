/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * contains the button control
	 */
	public class FormCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.form")]
		public void magix_forms_controls_form(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.create-web-part"].Value = null;
				e.Params ["_no-embed"].Value = true;
				e.Params["inspect"].Value = @"creates a form from the given value of the node, 
the [id] node's value serves as unique id if more than
one form of the same type is injected onto the same page.&nbsp;&nbsp;
use [css] to set css class of wrapping control, and [tag] to set html tag
of wrapping control.&nbsp;&nbsp;not thread safe";
				e.Params["container"].Value = "content5";
				e.Params["form-id"].Value = "sample-form";
				e.Params["controls"]["form"].Value = "name-of-my-form";
				e.Params["controls"]["id"].Value = "unique-id";
				e.Params["controls"]["css"].Value = "css-of-div-wrapper";
				e.Params["controls"]["tag"].Value = "div";
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Node form = new Node();

			form["prototype"]["type"].Value = "magix.forms.form";

			form["prototype"]["name"].Value = node.Get<string>();

			RaiseActiveEvent(
				"magix.data.load",
				form);

			if (form.Contains("objects"))
			{
				Node retVal = new Node("widgets");

				BuildTemplateControl(form["objects"][0]["form"]["surface"]["controls"], retVal);

				e.Params["_ctrl"].AddRange(retVal);
			}
		}
		
		[ActiveEvent(Name = "magix.forms.show-form")]
		public void magix_forms_show_form(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.show-form"].Value = null;
				e.Params["container"].Value = "content1";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["form"].Value = "hyper lisp code";
				e.Params["css"].Value = "css class(es) of your form";
				e.Params["inspect"].Value = @"creates a dynamic form
from database format, meaning the same format 
being used to save and load from database, and 
not the [create-form] syntax.&nbsp;&nbsp;use [container] 
and [form-id] as you would in [create-form].&nbsp;&nbsp;
not thread safe";
			}

			if (!e.Params.Contains("container"))
				throw new ArgumentException("you need a [container] for your show-form");

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("you need a [form-id] for your show-form");

			Node tmp = new Node();

			tmp["form-id"].Value = e.Params["form-id"].Value;

			if (e.Params.Contains("css"))
				tmp["css"].Value = e.Params["css"].Value;

			BuildTemplateControl(e.Params["form"]["form"]["controls"], tmp["controls"]);

 			LoadModule(
				"Magix.forms.DynamicForm", 
				e.Params["container"].Get<string>(), 
				tmp);
		}

		private void BuildTemplateControl(Node path, Node results)
		{
			foreach (Node idx in path)
			{
				// traversing all "widgets" in form's surface
				Node ctrl = new Node(idx["type"].Get<string>().Replace("magix.forms.controls.", ""), idx["properties"]["id"].Value);

				foreach (Node idxProp in idx["properties"])
				{
					if (idx.Name == "id")
						continue;

					ctrl[idxProp.Name].Value = idxProp.Value;
					foreach (Node idxInner in idxProp)
					{
						ctrl[idxProp.Name].Add(idxInner.Clone());
					}
				}

				if (idx.Contains("controls"))
				{
					BuildTemplateControl(idx["controls"], ctrl["controls"]);
				}
				results.Add(ctrl);
			}
		}
	}
}

