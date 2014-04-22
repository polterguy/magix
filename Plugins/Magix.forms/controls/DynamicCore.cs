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
	 * dynamic control
	 */
	public class DynamicCore : BaseWebControlCore
	{
		/**
		 * creates a dynamic control
		 */
		[ActiveEvent(Name = "magix.forms.controls.dynamic")]
		public void magix_forms_controls_dynamic(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

            Node node = Ip(e.Params)["_code"].Value as Node;

			DynamicPanel ret = new DynamicPanel();

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

			FillOutParameters(node, ret);

            if (node.Contains("tag") && node["tag"].Value != null)
                ret.Tag = node["tag"].Get<string>();

            if (node.Contains("default") && node["default"].Value != null)
                ret.DefaultWidget = node["default"].Get<string>();

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a dynamic panel type of web control.&nbsp;&nbsp;
a dynamic panel is basically a viewport container, which you can load with controls 
the same way you can load contrrols into a regular viewport, using for instance 
[magix.forms.create-web-part].&nbsp;&nbsp;
you can also set the [default] to the id of a specific control, such as a button, 
which will automatically raise [onclick] on that control if carriage return
is entered into a [text-box], or something similar, within the scope of the [panel].&nbsp;&nbsp;
[tag] sets html tag to render panel as.&nbsp;&nbsp;[default] sets the defaul 
control within your panel.&nbsp;&nbsp;";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["dynamic"]["tag"].Value = "p|div|address|etc";
            node["controls"]["dynamic"]["default"].Value = "default-button";
            base.Inspect(node["controls"]["dynamic"]);
		}
	}
}

