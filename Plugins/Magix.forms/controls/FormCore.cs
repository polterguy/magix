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
				e.Params["controls"]["form"].Value = "name-of-your-form";
				e.Params["controls"]["id"].Value = "unique-id";
				e.Params["controls"]["css"].Value = "css-of-div-wrapper";
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			bool isFirst = node["_first"].Get<bool>();

			if (!node.Contains("_buffer"))
			{
				Node form = new Node();

				form["prototype"]["type"].Value = "magix.forms.form";

				form["prototype"]["name"].Value = node.Get<string>();

				RaiseActiveEvent(
					"magix.data.load",
					form);

				Node buffer = new Node();

				if (form.Contains("objects"))
				{
                    Ip(e.Params)["_ctrl"].Value = null;

					if (form["objects"][0]["form"]["surface"].Contains("controls"))
					{
						foreach (Node idx in form["objects"][0]["form"]["surface"])
						{
							BuildTemplateControl(idx, buffer);
						}
					}
				}
				node["_buffer"].Value = buffer;
			}

			Node ctrlNode = node["_buffer"].Value as Node;

			foreach (Node idx in ctrlNode)
			{
				// this is a control
				string evtName = "magix.forms.controls." + idx.Name;

				Node nodeCtrl = new Node(idx.Name);

				nodeCtrl["_code"].Value = idx;
				idx["_first"].Value = isFirst;

				RaiseActiveEvent(
					evtName,
					nodeCtrl);

				idx["_first"].UnTie();

				if (nodeCtrl.Contains("_ctrl"))
				{
					if (nodeCtrl["_ctrl"].Value != null)
                        Ip(e.Params)["_ctrl"].Add(new Node("_x", nodeCtrl["_ctrl"].Value));
					else
					{
						// multiple controls returned ...
						foreach (Node idxCtrl in nodeCtrl["_ctrl"])
						{
                            Ip(e.Params)["_ctrl"].Add(new Node("_x", idxCtrl.Value));
						}
					}
				}
				else
				{
					throw new ArgumentException("unknown control type in your template control '" + idx.Name + "'");
				}
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
					if (idxProp.Name == "id")
						continue;

					ctrl[idxProp.Name].Value = idxProp.Value;
					foreach (Node idxInner in idxProp)
					{
						ctrl[idxProp.Name].Add(idxInner.Clone());
					}
				}

				if (idx.Contains("controls") && idx["controls"].Count > 0)
				{
					BuildTemplateControl(idx["controls"], ctrl["controls"]);
				}
				results.Add(ctrl);
			}
		}
	}
}

