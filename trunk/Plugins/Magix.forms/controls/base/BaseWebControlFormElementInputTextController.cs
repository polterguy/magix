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
	 * abstract helper class
	 */
    public abstract class BaseWebControlFormElementInputTextController : BaseWebControlFormElementTextController
	{
        /*
         * selects all text
         */
        [ActiveEvent(Name = "magix.forms.select-all")]
        private static void magix_forms_select_all(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.select-all-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.select-all-sample]");
                return;
            }

            BaseWebControlFormElementInputText ctrl = FindControl<BaseWebControlFormElementInputText>(e.Params);
            ctrl.Select();
        }

        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);
            BaseWebControlFormElementInputText ret = ctrl as BaseWebControlFormElementInputText;

            Node ip = Ip(pars);
            Node node = ip["_code"].Get<Node>();
            if (node.ContainsValue("placeholder"))
                ret.PlaceHolder = node["placeholder"].Get<string>();

            if (ShouldHandleEvent("ontextchanged", node))
            {
                Node codeNode = node["ontextchanged"].Clone();
                ret.TextChanged += delegate(object sender2, EventArgs e2)
                {
                    FillOutEventInputParameters(codeNode, sender2);
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
                "[magix.forms.base-web-form-element-text-input-dox].value",
                true);
            node["placeholder"].Value = "shadow text ...";
            node["ontextchanged"].Value = "hyperlisp code";
        }
    }
}
