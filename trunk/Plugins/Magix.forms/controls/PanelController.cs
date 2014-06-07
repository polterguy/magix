/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/*
	 * panel control
	 */
    internal sealed class PanelController : AttributeControlController
	{
		/*
		 * creates a panel control
		 */
		[ActiveEvent(Name = "magix.forms.controls.panel")]
		private void magix_forms_controls_panel(object sender, ActiveEventArgs e)
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

            if (node.ContainsValue("tag"))
				ret.Tag = node["tag"].Get<string>();

            if (node.ContainsValue("default"))
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

        protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.panel-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.panel-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["panel"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.panel-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["panel"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.panel-sample-end]");
		}
	}
}

