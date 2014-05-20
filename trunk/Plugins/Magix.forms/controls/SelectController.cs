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
    public class SelectController : BaseWebControlListFormElementController
	{
		/*
		 * creates the select list control
		 */
		[ActiveEvent(Name = "magix.forms.controls.select")]
		public void magix_forms_controls_select(object sender, ActiveEventArgs e)
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
            if (node.Contains("size") && node["size"].Value != null)
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
		protected void magix_forms_set_values(object sender, ActiveEventArgs e)
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

            Select ctrl = FindControl<Select>(Ip(e.Params));

			if (ctrl != null)
			{
				Select lst = ctrl as Select;
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
		}
		protected override void Inspect (Node node)
		{
            AppendInspect(node["inspect"], @"creates a select type of web control

select web controls are useful since they can be used much the same way a radio 
web control can be used, but they take less space.  a select list can have several 
items, where the name of your node underneath items becomes its value, and value 
becomes its text to show to end user.  user can then choose only one of these items 
to be set as active");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["select"]);
            node["magix.forms.create-web-part"]["controls"]["select"]["size"].Value = 5;
            node["magix.forms.create-web-part"]["controls"]["select"]["onselectedindexchanged"].Value = "hyperlisp code";
            AppendInspect(node["inspect"], @"[size] changes how many visible 
choices there shall be at the same time.  if you set this value to anything 
larger than 1, then the select will not render as a drop-down-list type of 
control, but show multiple items at the same time

[onselectedindexchanged] is raised when the selected item of the web control 
is being changed by the user", true);
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
            if (((Select)that).SelectedItem == null)
                return null;
			return ((Select)that).SelectedItem.Value;
		}
	}
}

