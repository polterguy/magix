/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/*
	 * contains the form control
	 */
    public abstract class BaseWebControlFormElementController : AttributeControlController
	{
		/*
		 * set-enabled
		 */
		[ActiveEvent(Name = "magix.forms.set-enabled")]
		private static void magix_forms_set_enabled(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-enabled-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-enabled-sample]");
				return;
			}

            BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(e.Params);
            ctrl.Disabled = !ip["value"].Get<bool>(true);
		}

		/*
		 * get-enabled
		 */
		[ActiveEvent(Name = "magix.forms.get-enabled")]
		private static void magix_forms_get_enabled(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-enabled-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-enabled-sample]");
                return;
            }

            BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(e.Params);
            ip["value"].Value = !ctrl.Disabled;
		}

        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);

            Node ip = Ip(pars);
            Node node = ip["_code"].Value as Node;
            BaseWebControlFormElement that = ctrl as BaseWebControlFormElement;

            if (node.ContainsValue("accesskey"))
                that.AccessKey = node["accesskey"].Get<string>();

            if (node.ContainsValue("disabled"))
                that.Disabled = node["disabled"].Get<bool>();

            if (ShouldHandleEvent("onfocus", node))
            {
                Node codeNode = node["onfocus"].Clone();
                that.Focused += delegate(object sender, EventArgs e)
                {
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
                        "magix.execute",
                        codeNode);
                };
            }

            if (ShouldHandleEvent("onblur", node))
            {
                Node codeNode = node["onblur"].Clone();
                that.Blur += delegate(object sender, EventArgs e)
                {
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
                        "magix.execute",
                        codeNode);
                };
            }
        }

        protected override void Inspect(Node node)
        {
            base.Inspect(node);
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            AppendInspectFromResource(
                tmp["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.base-web-form-element-dox].value",
                true);
            node["accesskey"].Value = "q";
            node["disabled"].Value = "false";
            node["onfocus"].Value = "hyperlisp code";
            node["onblur"].Value = "hyperlisp code";
        }
    }
}
