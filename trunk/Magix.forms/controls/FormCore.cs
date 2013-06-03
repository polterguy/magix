/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
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
				e.Params["event:magix.forms.create-form"].Value = null;
				e.Params ["_no-embed"].Value = true;
				e.Params["inspect"].Value = @"creates a form from the given value of the node, 
the [id] node's value serves as unique id if more than
one form of the same type is injected onto the same page.&nbsp;&nbsp;
use [css] to set css class of wrapping control, and [tag] to set html tag
of wrapping control.&nbsp;&nbsp;not thread safe";
				e.Params["container"].Value = "modal";
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

				e.Params["_tpl"].AddRange(retVal);
				if (node.Contains("id") && !string.IsNullOrEmpty(node["id"].Get<string>()))
					e.Params["_tpl"]["id"].Value = node["id"].Get<string>();
				if (node.Contains("css") && !string.IsNullOrEmpty(node["css"].Get<string>()))
					e.Params["_tpl"]["css"].Value = node["css"].Get<string>();
				if (node.Contains("tag") && !string.IsNullOrEmpty(node["tag"].Get<string>()))
					e.Params["_tpl"]["tag"].Value = node["tag"].Get<string>();
			}
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

