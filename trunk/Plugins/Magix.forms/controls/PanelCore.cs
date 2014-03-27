/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * panel control
	 */
	public class PanelCore : BaseWebControlCore
	{
		/**
		 * creates a panel control
		 */
		[ActiveEvent(Name = "magix.forms.controls.panel")]
		public void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

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
					{
						if (nc["_ctrl"].Value != null)
							ret.Controls.Add(nc["_ctrl"].Value as Control);
						else
						{
							// multiple controls returned ...
							foreach (Node idxCtrl in nc["_ctrl"])
							{
								ret.Controls.Add(idxCtrl.Value as Control);
							}
						}
					}
					else
						throw new ArgumentException("unknown control type in your template control '" + typeName + "'");
				}
			}

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		/**
		 * returns values
		 */
        [ActiveEvent(Name = "magix.forms.get-values")]
        public void magix_forms_get_values(object sender, ActiveEventArgs e)
        {
            if (ShouldInspectOrHasInspected(e.Params))
            {
                e.Params["inspect"].Value = @"returns the values property of all child controls
which are direct children of the panel in the [values] node, with name being id of control, 
and value being the value of the control";
                return;
            }

            Panel ctrl = FindControl<Panel>(Ip(e.Params));

            foreach (Control idx in ctrl.Controls)
            {
                object value = null;
                if (idx is Button)
                    value = (idx as Button).Text;
                if (idx is CheckBox)
                    value = (idx as CheckBox).Checked;
                if (idx is HiddenField)
                    value = (idx as HiddenField).Value;
                if (idx is HyperLink)
                    value = (idx as HyperLink).Text;
                if (idx is Image)
                    value = (idx as Image).ImageUrl;
                if (idx is Label)
                    value = (idx as Label).Text;
                if (idx is LinkButton)
                    value = (idx as LinkButton).Text;
                if (idx is LiteralControl)
                    value = (idx as LiteralControl).Text;
                if (idx is RadioButton)
                    value = (idx as RadioButton).Checked;
                if (idx is SelectList)
                    value = (idx as SelectList).SelectedItem.Value;
                if (idx is TextArea)
                    value = (idx as TextArea).Text;
                if (idx is TextBox)
                    value = (idx as TextBox).Text;
                if (idx is Uploader)
                    value = (idx as Uploader).GetFileName();
                if (idx is Wysiwyg)
                    value = (idx as Wysiwyg).Text;
                if (value != null)
                    Ip(e.Params)["values"][idx.ID].Value = value;
            }
        }

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a panel type of web control.&nbsp;&nbsp;
a panel is highly useful, since it can be a container for other controls.&nbsp;&nbsp;
you can also set the [default] to the id of a specific control, such as a button, 
which will automatically raise [onclick] on that control if carriage return
is entered into a [text-box], or something similar, within the scope of the [panel].&nbsp;&nbsp;
[tag] sets html tag to render panel as.&nbsp;&nbsp;[default] sets the defaul 
control within your panel.&nbsp;&nbsp;[controls] contains a child control collection";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["panel"]["tag"].Value = "p|div|address|etc";
			node["controls"]["panel"]["default"].Value = "default-button";
			base.Inspect(node["controls"]["panel"]);
			node["controls"]["panel"]["controls"]["button"].Value = "default-button";
			node["controls"]["panel"]["controls"]["button"]["text"].Value = "don't work";
		}
	}
}

