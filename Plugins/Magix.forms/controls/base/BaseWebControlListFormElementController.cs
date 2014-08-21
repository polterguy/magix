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
                    ListItem item = new ListItem(idxItemNode.Get<string>(), idxItemNode.Name);
                    if (!idxItemNode.GetValue("enabled", true))
                        item.Enabled = false;
                    ret.Items.Add(item);
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
                "[magix.forms.base-web-list-form-element-dox].value",
                true);
            node["items"]["item1"].Value = "this is item 1";
            node["items"]["item2"].Value = "this is item 2";
            node["items"]["item2"]["enabled"].Value = false;
            node["items"]["item3"].Value = "this is item 3";
        }
    }
}
