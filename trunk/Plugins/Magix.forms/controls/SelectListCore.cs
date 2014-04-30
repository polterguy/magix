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
	/**
	 * select list
	 */
	public class SelectListCore : FormElementCore
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

            Node node = Ip(e.Params)["_code"].Value as Node;

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
				Node codeNode = node["onselectedindexchanged"].Clone();

				ret.SelectedIndexChanged += delegate(object sender2, EventArgs e2)
				{
					SelectList that2 = sender2 as SelectList;
					if (!string.IsNullOrEmpty(that2.Info))
						codeNode["$"]["info"].Value = that2.Info;

					object val = GetValue(that2);
					if (val != null)
						codeNode["$"]["value"].Value = val;

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            Ip(e.Params)["_ctrl"].Value = ret;
		}

		/**
		 * sets selected value
		 */
		[ActiveEvent(Name = "magix.forms.set-value")]
		public void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
            if (ShouldInspectOrHasInspected(e.Params))
			{
				e.Params["inspect"].Value = "sets the value property of the control";
				return;
			}

            if (!Ip(e.Params).Contains("value"))
				throw new ArgumentException("set-value needs [value]");

            SelectList ctrl = FindControl<SelectList>(Ip(e.Params));

			if (ctrl != null)
			{
                ctrl.SetSelectedItemAccordingToValue(Ip(e.Params)["value"].Get<string>());
			}
		}

		/**
		 * returns value
		 */
		[ActiveEvent(Name = "magix.forms.get-value")]
		public void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
            if (ShouldInspectOrHasInspected(e.Params))
			{
				e.Params["inspect"].Value = "returns the value property of the control";
				return;
			}

            SelectList ctrl = FindControl<SelectList>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.SelectedItem.Value;
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

            SelectList ctrl = FindControl<SelectList>(Ip(e.Params));

			if (ctrl != null)
			{
				SelectList lst = ctrl as SelectList;
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
            node["inspect"].Value = @"
<p>creates a select list input type of web control.&nbsp;&nbsp;
select lists are useful since they can be used much the same 
way a radio buttons can be used, but they take less space.
&nbsp;&nbsp;a select list can have several [items], where the 
name of your node underneath [items] becomes its value, and 
value becomes its text to show to end user.&nbsp;&nbsp;user 
can then choose only one of these items to be set as active</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["select"]);
            node["magix.forms.create-web-part"]["controls"]["select"]["size"].Value = 5;
            node["magix.forms.create-web-part"]["controls"]["select"]["key"].Value = "C";
            node["magix.forms.create-web-part"]["controls"]["select"]["enabled"].Value = true;
            node["magix.forms.create-web-part"]["controls"]["select"]["selected"].Value = "item3";
            node["magix.forms.create-web-part"]["controls"]["select"]["onselectedindexchanged"].Value = "hyper lisp code";
            node["magix.forms.create-web-part"]["controls"]["select"]["items"]["item1"].Value = "Item 1";
            node["magix.forms.create-web-part"]["controls"]["select"]["items"]["item2"].Value = "Item 2";
            node["magix.forms.create-web-part"]["controls"]["select"]["items"]["item3"].Value = "Item 3";
            node["magix.forms.create-web-part"]["controls"]["select"]["items"]["item4"].Value = "Item 4";
            node["magix.forms.create-web-part"]["controls"]["select"]["items"]["item5"].Value = "Item 5";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for select list</strong></p><p>[size] 
changes how many visible choices there shall be at the same 
time</p><p>[key] is the keyboard shortcut.&nbsp;&nbsp;how to 
invoke the keyboard shortcut is different from system to 
system, but on a windows system, you normally invoke the 
keyboard shortcut with alt+shift+your-key.&nbsp;&nbsp;if 
you have for instance 's' as your keyboard shortcut, then 
the end user will have to click shift+alt+s at the same 
time to invoke the keyboard shortcut for your web control</p>
<p>[enabled] enables or disables the web control.&nbsp;&nbsp;
this can be changed or retrieved after the button is created by 
invoking the [magix.forms.set-enabled] or [magix.forms.get-enabled] 
active events.&nbsp;&nbsp;legal values are true and false</p>
<p>[items] contains a collection of value/text pairs, which is 
used as the different options to the select list</p><p>[selected] 
can be set to the value of any of its items, to set an initially 
selected choice</p><p>[onselectedindexchanged] is raised when 
selected item state of control changes</p>";
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((SelectList)that).SelectedItem.Value;
		}
	}
}

