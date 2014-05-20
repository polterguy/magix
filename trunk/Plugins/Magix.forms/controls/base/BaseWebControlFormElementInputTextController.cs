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

        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);
            BaseWebControlFormElementInputText ret = ctrl as BaseWebControlFormElementInputText;

            Node ip = Ip(pars);
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
        }

        protected override void Inspect(Node node)
        {
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            base.Inspect(node);
            AppendInspect(tmp["inspect"], @"[placeholder] is a piece of shadow text, that will 
only display if the web control has no text value.  this is useful for displaying additional 
information back to the user, explaining what the text-box/text-area is taking as input.  this 
can be used as an alternative to a descriptive label

[ontextchanged] is raised when the text of the web control has changed, but not before focus 
is lost somehow", true);
            node["placeholder"].Value = "shadow text ...";
            node["ontextchanged"].Value = "hyperlisp code";
        }
    }
}
