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
	 * select list
	 */
    internal sealed class SelectController : BaseWebControlListFormElementController
	{
		/*
		 * creates the select list control
		 */
		[ActiveEvent(Name = "magix.forms.controls.select")]
		private void magix_forms_controls_select(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Select ret = new Select();
			FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("size"))
				ret.Size = node["size"].Get<int>();

			if (ShouldHandleEvent("onselectedindexchanged", node))
			{
				Node codeNode = node["onselectedindexchanged"].Clone();
				ret.SelectedIndexChanged += delegate(object sender2, EventArgs e2)
				{
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            ip["_ctrl"].Value = ret;
		}

		/*
		 * set-values
		 */
		[ActiveEvent(Name = "magix.forms.set-values")]
		private void magix_forms_set_values(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-values"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["values"]["item1"].Value = "new value 1";
				e.Params["values"]["item2"].Value = "new value 2";
				e.Params["inspect"].Value = @"sets the values of the given 
[id] web control, in the [form-id] form, from [values] for select widgets.&nbsp;&nbsp;
not thread safe";
				return;
			}

            Select lst = FindControl<Select>(Ip(e.Params));
			lst.Items.Clear();
            if (Ip(e.Params).Contains("values"))
			{
                foreach (Node idx in Ip(e.Params)["values"])
				{
					ListItem it = new ListItem(idx.Get<string>(), idx.Name);
					lst.Items.Add(it);
				}
			}
			lst.ReRender();
		}

		protected override void Inspect (Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.select-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.select-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["select"]);
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.select-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["select"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.select-sample-end]");
		}
	}
}

