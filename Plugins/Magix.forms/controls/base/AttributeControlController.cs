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
		 * creates an attribute
		 */
        [ActiveEvent(Name = "magix.forms.add-attribute")]
        private static void magix_forms_add_attribute(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.add-attribute-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.add-attribute-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string name = Expressions.GetExpressionValue<string>(ip["name"].Get<string>(), dp, ip, false);
            string value = Expressions.GetExpressionValue<string>(ip["value"].Get<string>(), dp, ip, false);

            AttributeControl ctrl = FindControl<AttributeControl>(e.Params);
            ctrl.Attributes.Add(new AttributeControl.Attribute(name, value));
        }

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
                "[magix.forms.attribute-dox].value",
                true);
        }
    }
}
