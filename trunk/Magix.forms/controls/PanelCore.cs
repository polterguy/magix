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
	 */
	public class PanelCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.panel")]
		public void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Panel ret = new Panel();

			FillOutParameters(node, ret);

			if (node.Contains("tag") && node["tag"].Value != null)
				ret.Tag = node["tag"].Get<string>();

			if (node.Contains("default") && node["default"].Value != null)
				ret.DefaultWidget = node["default"].Get<string>();

			if (node.Contains("controls"))
			{
				foreach (Node idx in node["controls"])
				{
					string typeName = idx.Name;

					bool isControlName = true;
					foreach (char idxC in typeName)
					{
						if ("abcdefghijklmnopqrstuvwxyz-".IndexOf(idxC) == -1)
						{
							isControlName = false;
							break;
						}
					}

					if (!isControlName)
						throw new ArgumentException("control '" + typeName + "' is not understood while trying to create web controls");

					string evtName = "magix.forms.controls." + typeName;

					Node nc = new Node();
					nc["_code"].Value = idx;

					RaiseActiveEvent(
						evtName,
						nc);

					if (nc.Contains("_ctrl"))
						ret.Controls.Add(nc["_ctrl"].Value as System.Web.UI.Control);
					else
					{
						if (!nc.Contains("_tpl"))
							throw new ArgumentException("unknown control type in your template control '" + typeName + "'");
						else
						{
							// this is a 'user control', or a 'template control', and we need to
							// individually traverse it, as if it was embedded into markup, almost
							// like copy/paste
							nc = new Node();
							nc["_code"].Value = idx;

							RaiseActiveEvent(
								"magix.forms.controls." + node["_tpl"][0].Name,
								nc);
							if (nc.Contains("_ctrl"))
								ret.Controls.Add(nc["_ctrl"].Value as System.Web.UI.Control);
							else
								throw new ArgumentException("unknown control type in your template control '" + typeName + "'");
						}
					}
				}
			}

			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a panel type of web control.&nbsp;&nbsp;
a panel is highly useful, since it can be a container for other controls.&nbsp;&nbsp;
you can also set the [default] to the id of a specific control, such as a button, 
which will automatically raise [onclick] on that control if carriage return
is entered into a [text-box], or something similar, within the scope of the [panel].&nbsp;&nbsp;
[tag] sets html tag to render panel as.&nbsp;&nbsp;[default] sets the defaul 
control within your panel.&nbsp;&nbsp;[controls] contains a child control collection";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["panel"]["tag"].Value = "p";
			node["controls"]["panel"]["default"].Value = "run";
			node["controls"]["panel"]["controls"]["button"].Value = "run";
			node["controls"]["panel"]["controls"]["button"]["text"].Value = "don't work";
			base.Inspect(node["controls"]["panel"]);
		}
	}
}

