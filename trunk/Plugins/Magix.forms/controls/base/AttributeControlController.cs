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
    public abstract class AttributeControlController : BaseWebControlController
	{
        /*
         * fills out the attributes
         */
        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);

            Node ip = Ip(pars);
            Node node = ip["_code"].Get<Node>();

            AttributeControl atrCtrl = ctrl as AttributeControl;
            foreach (Node idx in node)
            {
                if (idx.Name.StartsWith("@"))
                {
                    atrCtrl.Attributes.Add(new AttributeControl.Attribute(idx.Name.Substring(1), idx.Get<string>()));
                }
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
                "[magix.forms.attribute-dox].Value",
                true);
        }
    }
}
