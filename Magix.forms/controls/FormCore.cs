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

			bool isFirst = node["_first"].Get<bool>();

			if (node.Contains("_buffer"))
			{
				form = node["_buffer"].Get<Node>();
			}
			else
			{
				form["prototype"]["type"].Value = "magix.forms.form";

				form["prototype"]["name"].Value = node.Get<string>();

				RaiseActiveEvent(
					"magix.data.load",
					form);
			}

			if (form.Contains("objects"))
			{
				if (form["objects"][0]["form"]["surface"].Contains("controls"))
				{
					foreach (Node idx in form["objects"][0]["form"]["surface"]["controls"])
					{
						string typeName = idx["type"].Get<string>();

						Node nc = new Node();

						Node idxCtrlCode = new Node(typeName.Replace("magix.forms.controls.", ""));

						foreach (Node idxProp in idx["properties"])
						{
							idxCtrlCode[idxProp.Name].Value = idxProp.Value;
							idxCtrlCode[idxProp.Name].ReplaceChildren(idxProp.Clone());
						}

						nc["_code"].Value = idxCtrlCode;
						idxCtrlCode["_first"].Value = isFirst;

						RaiseActiveEvent(
							typeName,
							nc);

						idxCtrlCode["_first"].UnTie();

						if (nc.Contains("_ctrl"))
						{
							if (nc["_ctrl"].Value != null)
								e.Params["_ctrl"].Add(new Node("_ct", nc["_ctrl"].Value));
							else
							{
								// multiple controls returned ...
								foreach (Node idxCtrl in nc["_ctrl"])
								{
									e.Params["_ctrl"].Add(new Node("_ct", idxCtrl.Value));
								}
							}
						}
						else
							throw new ArgumentException("unknown control type in your form control '" + typeName + "'");
					}
				}
			}
			node["_buffer"].Value = form.Clone();
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

