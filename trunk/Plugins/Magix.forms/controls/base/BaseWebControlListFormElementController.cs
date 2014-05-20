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

            if (node.Contains("selected") &&
                node["selected"].Value != null)
            {
                ret.SetSelectedItemAccordingToValue(node["selected"].Get<string>());
            }
        }

        protected override void Inspect(Node node)
        {
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            base.Inspect(node);
            AppendInspect(tmp["inspect"], @"[items] is a list of key/value nodes, 
that are the items of the list form element

[selected] is the id of the item in the [items] collection that should be initially 
selected.  this is the name of the node who's value is supposed to be initially 
selected during creation.  this is also the value that is changed or retrieved when 
the [magix.forms.set-value] or [magix.forms.get-value] active events are invoked on 
the web control", true);
            node["items"]["item1"].Value = "this is item 1";
            node["items"]["item2"].Value = "this is item 2";
        }
    }
}
