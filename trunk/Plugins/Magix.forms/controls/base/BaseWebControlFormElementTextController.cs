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
    public abstract class BaseWebControlFormElementTextController : BaseWebControlFormElementController
	{
        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);

            Node ip = Ip(pars);
            Node codeNode = ip["_code"].Value as Node;
            BaseWebControlFormElementText that = ctrl as BaseWebControlFormElementText;

            if (codeNode.Contains("value") &&
                !string.IsNullOrEmpty(codeNode["value"].Get<string>()))
                that.Value = codeNode["value"].Get<string>();
        }

        protected override void Inspect(Node node)
        {
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            base.Inspect(node);
            AppendInspect(tmp["inspect"], @"[value] is the visible text of your button.  
this is the property which is changed or retrieved when you invoke the [magix.forms.set-
value] and the [magix.forms.get-value] for your web control", true);
            node["value"].Value = "text value";
        }
    }
}
