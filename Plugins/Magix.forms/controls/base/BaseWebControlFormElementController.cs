/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
                    "[magix.forms.set-enabled-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-enabled-sample]");
				return;
			}

            BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(ip);
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
                    "[magix.forms.get-enabled-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-enabled-sample]");
                return;
            }

            BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(ip);
            ip["value"].Value = !ctrl.Disabled;
		}

        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);

            Node ip = Ip(pars);
            Node codeNode = ip["_code"].Value as Node;
            BaseWebControlFormElement that = ctrl as BaseWebControlFormElement;

            if (codeNode.ContainsValue("accesskey"))
                that.AccessKey = codeNode["accesskey"].Get<string>();

            if (codeNode.ContainsValue("disabled"))
                that.Disabled = codeNode["disabled"].Get<bool>();
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
                "[magix.forms.base-web-form-element-dox].Value",
                true);
            node["accesskey"].Value = "q";
            node["disabled"].Value = "false";
        }
    }
}
