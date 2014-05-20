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
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-enabled"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"sets the enabled property of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

            BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(Ip(e.Params));

			if (ctrl != null)
			{
				bool disabled = false;
                if (Ip(e.Params).Contains("value"))
                    disabled = Ip(e.Params)["value"].Get<bool>();

				ctrl.Disabled = !disabled;
			}
		}

		/*
		 * get-enabled
		 */
		[ActiveEvent(Name = "magix.forms.get-enabled")]
		private static void magix_forms_get_enabled(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.get-enabled"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"retrieves the enabled property of the given 
[id] web control, in the [form-id] form, into [value].&nbsp;&nbsp;not thread safe";
				return;
			}

            BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = !ctrl.Disabled;
			}
		}

        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);

            Node ip = Ip(pars);
            Node codeNode = ip["_code"].Value as Node;
            BaseWebControlFormElement that = ctrl as BaseWebControlFormElement;

            if (codeNode.Contains("accesskey") &&
                !string.IsNullOrEmpty(codeNode["accesskey"].Get<string>()))
                that.AccessKey = codeNode["accesskey"].Get<string>();

            if (codeNode.Contains("disabled") &&
                codeNode["disabled"].Value != null)
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
