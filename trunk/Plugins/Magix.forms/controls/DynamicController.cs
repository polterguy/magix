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
	 * dynamic control
	 */
    public class DynamicController : AttributeControlController
	{
		/*
		 * creates a dynamic control
		 */
		[ActiveEvent(Name = "magix.forms.controls.dynamic")]
		public void magix_forms_controls_dynamic(object sender, ActiveEventArgs e)
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
            if (node.Contains("tag") && node["tag"].Value != null)
                ret.Tag = node["tag"].Get<string>();

            if (node.Contains("default") && node["default"].Value != null)
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
            node["inspect"].Value = @"
<p>creates a dynamic panel type of web control.&nbsp;&nbsp;
a dynamic panel is a viewport container, which you can load 
with controls the same way you can load controls into a regular 
viewport, using for instance [magix.forms.create-web-part].
&nbsp;&nbsp;you can also set the [default] to the id of a 
specific control, such as a button, which will automatically 
raise [onclick] on that control if carriage returnis entered 
into a [text-box], or something similar, within the scope of 
the web control.&nbsp;&nbsp; a dynamic, is basically a [panel], 
except it can dynamically load controls into itself</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["dynamic"]);
            node["magix.forms.create-web-part"]["controls"]["dynamic"]["tag"].Value = "p|div|address|etc";
            node["magix.forms.create-web-part"]["controls"]["dynamic"]["default"].Value = "default-button";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for dynamic</strong><p>[tag] sets html 
tag to render panel as.&nbsp;&nbsp;you can change this to any 
html tag you wish for the control to be rendered within, such 
as p, div, label, span or address, etc</p><p>[default] sets 
the default control within your web control.&nbsp;&nbsp;this 
is the control which will be automatically clicked if carriage 
return is pressed while a child control of the dynamic hass focus</p>";
		}
	}
}

