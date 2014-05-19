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
	/*
	 * panel control
	 */
    public class PanelController : AttributeControlController
	{
		/*
		 * creates a panel control
		 */
		[ActiveEvent(Name = "magix.forms.controls.panel")]
		public void magix_forms_controls_panel(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Panel ret = new Panel();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();
            if (node.Contains("tag") && node["tag"].Value != null)
				ret.Tag = node["tag"].Get<string>();

			if (node.Contains("default") && node["default"].Value != null)
				ret.Default = node["default"].Get<string>();

			if (node.Contains("controls"))
			{
				foreach (Node idxCtrlNode in node["controls"])
				{
					string ctrlTypeName = idxCtrlNode.Name;

					bool isControlName = true;
					foreach (char idxChar in ctrlTypeName)
					{
						if ("abcdefghijklmnopqrstuvwxyz-".IndexOf(idxChar) == -1)
						{
							isControlName = false;
							break;
						}
					}

					if (!isControlName)
						throw new ArgumentException("control '" + ctrlTypeName + "' is not understood while trying to create web controls");

					string ctrlEventName = "magix.forms.controls." + ctrlTypeName;

					Node createChildCtrlNode = new Node();
					createChildCtrlNode["_code"].Value = idxCtrlNode;

                    if (e.Params.Contains("_first"))
                        createChildCtrlNode["_first"].Value = e.Params["_first"].Value;

					RaiseActiveEvent(
						ctrlEventName,
						createChildCtrlNode);

					if (createChildCtrlNode.Contains("_ctrl"))
					{
						if (createChildCtrlNode["_ctrl"].Value != null)
							ret.Controls.Add(createChildCtrlNode["_ctrl"].Value as Control);
						else
						{
							// multiple controls returned ...
							foreach (Node idxCtrl in createChildCtrlNode["_ctrl"])
							{
								ret.Controls.Add(idxCtrl.Value as Control);
							}
						}
					}
					else
						throw new ArgumentException("unknown control type in your template control '" + ctrlTypeName + "'");
				}
			}

            ip["_ctrl"].Value = ret;
		}

		/*
		 * returns values
		 */
        [ActiveEvent(Name = "magix.forms.get-children-values")]
        public void magix_forms_get_children_values(object sender, ActiveEventArgs e)
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
                    value = (idx as Button).Value;
                if (idx is CheckBox)
                    value = (idx as CheckBox).Checked;
                if (idx is Hidden)
                    value = (idx as Hidden).Value;
                if (idx is HyperLink)
                    value = (idx as HyperLink).Value;
                if (idx is Img)
                    value = (idx as Img).Src;
                if (idx is Label)
                    value = (idx as Label).Value;
                if (idx is LinkButton)
                    value = (idx as LinkButton).Value;
                if (idx is LiteralControl)
                    value = (idx as LiteralControl).Text;
                if (idx is Radio)
                    value = (idx as Radio).Checked;
                if (idx is Select)
                    value = (idx as Select).SelectedItem.Value;
                if (idx is TextArea)
                    value = (idx as TextArea).Value;
                if (idx is TextBox)
                    value = (idx as TextBox).Value;
                if (idx is Uploader)
                    value = (idx as Uploader).GetFileName();
                if (idx is Wysiwyg)
                    value = (idx as Wysiwyg).Value;
                if (value != null)
                    Ip(e.Params)["values"][idx.ID].Value = value;
            }
        }

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a panel type of web control.&nbsp;&nbsp;
a panel is a container control for other web controls.
&nbsp;&nbsp;add up other web controls and controls 
into its [control] collection, such that your panel 
serves as a wrapper for your other controls.&nbsp;&nbsp;
you can also set the [default] to the id of a specific 
control, such as a button, which will automatically 
raise [onclick] on that control if carriage return is 
entered into a [text-box], or something similar, within 
the scope of the web control</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["panel"]);
            node["magix.forms.create-web-part"]["controls"]["panel"]["tag"].Value = "p|div|address|etc";
            node["magix.forms.create-web-part"]["controls"]["panel"]["default"].Value = "default-button";
            node["magix.forms.create-web-part"]["controls"]["panel"]["controls"]["button"].Value = "default-button";
            node["magix.forms.create-web-part"]["controls"]["panel"]["controls"]["button"]["value"].Value = "don't work";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for panel</strong><p>[tag] sets 
html tag to render panel as.&nbsp;&nbsp;you can change 
this to any html tag you wish for the control to be 
rendered within, such as p, div, label, span or address, 
etc</p><p>[default] sets the default control within your 
web control.&nbsp;&nbsp;this is the control which will 
be automatically clicked if carriage return is pressed 
while a child control of the dynamic hass focus</p>";
		}
	}
}

