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
    public abstract class BaseWebControlListFormElementController : BaseWebControlFormElementController
	{
        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);
            BaseWebControlListFormElement ret = ctrl as BaseWebControlListFormElement;
            Node ip = Ip(pars);
            Node node = ip["_code"].Get<Node>();
            if (node.Contains("items"))
            {
                foreach (Node idxItemNode in node["items"])
                {
                    if (idxItemNode.Name == null)
                        throw new ArgumentException("list item for select needs unique name of node to be used as value");
                    if (idxItemNode.Value == null)
                        throw new ArgumentException("list item for select needs value of node to be used as text to show user in item");
                    ret.Items.Add(new ListItem(idxItemNode.Get<string>(), idxItemNode.Name));
                }
            }

            if (node.ContainsValue("selected"))
                ret.SetSelectedItemAccordingToValue(node["selected"].Get<string>());
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
                "[magix.forms.base-web-list-form-element-dox].Value",
                true);
            node["items"]["item1"].Value = "this is item 1";
            node["items"]["item2"].Value = "this is item 2";
        }
    }
}
