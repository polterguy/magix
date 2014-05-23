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

namespace Magix.forms
{
	/*
	 * dynamic control
	 */
    internal sealed class DynamicController : AttributeControlController
	{
		/*
		 * creates a dynamic control
		 */
		[ActiveEvent(Name = "magix.forms.controls.dynamic")]
		private void magix_forms_controls_dynamic(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			DynamicPanel ret = new DynamicPanel();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("tag"))
                ret.Tag = node["tag"].Get<string>();

            if (node.ContainsValue("default"))
                ret.Default = node["default"].Get<string>();

            ret.Reload +=
                delegate(object sender2, DynamicPanel.ReloadEventArgs e2)
                {
                    DynamicPanel dynamic = sender2 as DynamicPanel;
                    Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(e2.Key);
                    if (e2.FirstReload)
                    {
                        // Since this is the Initial Loading of our module
                        // We'll need to make sure our Initial Loading procedure is being
                        // called, if Module is of type ActiveModule
                        Node nn = e2.Extra as Node;
                        ctrl.Init +=
                            delegate
                            {
                                ActiveModule module = ctrl as ActiveModule;
                                if (module != null)
                                {
                                    module.InitialLoading(nn);
                                }
                            };
                    }
                    dynamic.Controls.Add(ctrl);
                };

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.dynamic-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.dynamic-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["dynamic"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.dynamic-dox-end].Value",
                true);
            node["magix.forms.create-web-part"]["controls"]["dynamic"]["onfirstload"].UnTie();
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["dynamic"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.dynamic-sample-end]");
		}
	}
}

