/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * select list
	 */
	public class SelectListCore : BaseWebControlCore
	{
		/**
		 * creates the select list control
		 */
		[ActiveEvent(Name = "magix.forms.controls.select")]
		public void magix_forms_controls_select(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			SelectList ret = new SelectList();

			FillOutParameters(node, ret);

			if (node.Contains("size") && node["size"].Value != null)
				ret.Size = node["size"].Get<int>();

			if (node.Contains("key") && 
			    !string.IsNullOrEmpty(node["key"].Get<string>()))
				ret.AccessKey = node["key"].Get<string>();

			if (node.Contains("enabled") && 
			    node["enabled"].Value != null)
				ret.Enabled = node["enabled"].Get<bool>();

			if (node.Contains("items"))
			{
				foreach (Node idxI in node["items"])
				{
					if (idxI.Name == null)
						throw new ArgumentException("list item for select needs unique name of node to be used as value");
					if (idxI.Value == null)
						throw new ArgumentException("list item for select needs value of node to be used as text to show user in item");
					ret.Items.Add(new ListItem(idxI.Get<string>(), idxI.Name));
				}
			}

			if (node.Contains("selected") && 
			    node["selected"].Value != null)
			{
				ret.SetSelectedItemAccordingToValue(node["selected"].Get<string>());
			}

			if (node.Contains("onselectedindexchanged"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onselectedindexchanged"].Clone();

				ret.SelectedIndexChanged += delegate(object sender2, EventArgs e2)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			e.Params["_ctrl"].Value = ret;
		}

		/**
		 * sets selected value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

			if (!e.Params.Contains("value"))
				throw new ArgumentException("set-value needs [value]");

			SelectList ctrl = FindControl<SelectList>(e.Params);

			if (ctrl != null)
			{
				ctrl.SetSelectedItemAccordingToValue(e.Params["value"].Get<string>());
			}
		}

		/**
		 * returns value
		 */
		[ActiveEvent(Name = "magix.forms.get-value")]
		public void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

			SelectList ctrl = FindControl<SelectList>(e.Params);

			if (ctrl != null)
			{
				e.Params["value"].Value = ctrl.SelectedItem.Value;
			}
		}

		/**
		 * set-enabled
		 */
		[ActiveEvent(Name = "magix.forms.set-enabled")]
		protected void magix_forms_set_enabled(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-enabled"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"sets the enabled property of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			SelectList ctrl = FindControl<SelectList>(e.Params);

			if (ctrl != null)
			{
				bool enabled = false;
				if (e.Params.Contains("value"))
					enabled = e.Params["value"].Get<bool>();

				ctrl.Enabled = enabled;
			}
		}

		/**
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

			SelectList ctrl = FindControl<SelectList>(e.Params);

			if (ctrl != null)
			{
				SelectList lst = ctrl as SelectList;
				lst.Items.Clear();
				if (e.Params.Contains("values"))
				{
					foreach (Node idx in e.Params["values"])
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
			node["event:magix.forms.create-web-part"].Value = null;
			node["inspect"].Value = @"creates a select list input type of web control.&nbsp;&nbsp;
select lists are useful since they can be used much the same way a [radio] control can be 
used, but they take less space.&nbsp;&nbsp;a select list can have several [items], where 
the name of your node underneath [items] becomes its value, and value becomes its 
text to show to end user.&nbsp;&nbsp;user can then choose only one of these items to 
be set as active.&nbsp;&nbsp;[size] changes how many visible choices there shall be at 
the same time.&nbsp;&nbsp;[key] is keyboard shortcut.&nbsp;&nbsp;[enabled] sets the 
control to disabled or enabled.&nbsp;&nbsp;[items] contains a collection of 
value/text pairs, which is used as the different options to the select list.&nbsp;&nbsp;
[selected] can be set to the value of any of its items, to set an initially selected
choice.&nbsp;&nbsp;[onselectedindexchanged] is raised when selected item state of control changes";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["select"]["size"].Value = 5;
			node["controls"]["select"]["key"].Value = "C";
			node["controls"]["select"]["enabled"].Value = true;
			node["controls"]["select"]["selected"].Value = "item3";
			// TODO: implement design support for items ...
			base.Inspect(node["controls"]["select"]);
			node["controls"]["select"]["items"]["item1"].Value = "Item 1";
			node["controls"]["select"]["items"]["item2"].Value = "Item 2";
			node["controls"]["select"]["items"]["item3"].Value = "Item 3";
			node["controls"]["select"]["items"]["item4"].Value = "Item 4";
			node["controls"]["select"]["items"]["item5"].Value = "Item 5";
			node["controls"]["check"]["onselectedindexchanged"].Value = "hyper lisp code";
		}
	}
}

