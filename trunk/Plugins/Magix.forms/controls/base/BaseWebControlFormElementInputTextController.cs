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
	 * abstract helper class
	 */
    public abstract class BaseWebControlFormElementInputTextController : BaseWebControlFormElementTextController
	{
        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);
            Node ip = Ip(pars);
            BaseWebControlFormElementInputText ret = ctrl as BaseWebControlFormElementInputText;

            Node node = ip["_code"].Get<Node>();
            if (node.Contains("placeholder") &&
                !string.IsNullOrEmpty(node["placeholder"].Get<string>()))
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

            if (ShouldHandleEvent("onescpressed", node))
            {
                Node codeNode = node["onescpressed"].Clone();
                ret.Esc += delegate(object sender2, EventArgs e2)
                {
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
                        "magix.execute",
                        codeNode);
                };
            }
        }

        /*
         * selects all text
         */
        [ActiveEvent(Name = "magix.forms.select-all")]
        protected void magix_forms_select_all(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                ip["inspect"].Value = @"<p>selects all text in the specified 
[id] textbox or textarea, in the [form-id] form</p><p>not thread safe</p>";
                ip["magix.forms.select-all"]["id"].Value = "control";
                ip["magix.forms.select-all"]["form-id"].Value = "webpages";
                return;
            }

            BaseWebControlFormElementInputText ctrl = FindControl<BaseWebControlFormElementInputText>(ip);
            ctrl.Select();
        }
    }
}
