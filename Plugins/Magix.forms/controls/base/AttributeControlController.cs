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
    public abstract class AttributeControlController : BaseWebControlController
	{
        /*
         * fills out the attributes
         */
        protected override void FillOutParameters(Node pars, BaseControl ctrl)
        {
            base.FillOutParameters(pars, ctrl);

            Node ip = Ip(pars);
            Node node = ip["_code"].Value as Node;

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
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            base.Inspect(node);
            AppendInspect(tmp["inspect"], @"an attribute control, is a web control that 
can have generic attributes attacched to it.  you can associate new attributes with your 
control by prefixing your attribute with an '@' character.  if you do, then an additional 
html attribute will be rendered back to the browser, with the name of your attribute, 
minus the first '@' character, and the value of that node.  if you wish for your attribute 
to actually start with a @ as its attribute name, then add up two consecutive @ signs after 
each other", true);
            node["@someattribute"].Value = "whatever value";
        }
    }
}
