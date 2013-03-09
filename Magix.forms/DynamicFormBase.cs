/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.Core;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * Base class, containging helpers to help dynamically build Web Controls
	 */
	public class DynamicFormBase : ActiveModule
	{
		protected delegate Node FindNode(string path);
		protected bool isFirst;

		protected string FormID
		{
			get { return ViewState["FormID"] as string; }
			set { ViewState["FormID"] = value; }
		}

		public override void InitialLoading(Node node)
		{
			isFirst = true;
			Load +=
				delegate
				{
					FormID = node["form-id"].Get<string>();
				};
			base.InitialLoading(node);
		}

		// TODO: create plugable event
		// TODO: Change id to Value instead of "id" node
		/**
		 * Returns the given value of the given "id" widget, within the given
		 * "form-id" form, in the "value" node
		 */
        [ActiveEvent(Name = "magix.forms.get-value")]
		protected void magix_forms_get_value(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.get-value"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formID";
				e.Params["inspect"].Value = @"returns the value of the given 
[id] web control, in the [form-id] form, as [value]";
				return;
			}

			if (!e.Params.Contains ("form-id"))
				throw new ArgumentException("Missing form-id in get-value");

			if (!e.Params.Contains ("id"))
				throw new ArgumentException("Missing id in get-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				Control ctrl = Selector.FindControl<Control>(this, e.Params["id"].Get<string>());
				if (ctrl is BaseWebControlFormElementText)
					e.Params["value"].Value = 
						((BaseWebControlFormElementText)ctrl).Text;
				else if (ctrl is CheckBox)
					e.Params["value"].Value = 
						((CheckBox)ctrl).Checked;
				else if (ctrl is HiddenField)
					e.Params["value"].Value = 
						((HiddenField)ctrl).Value;
				else if (ctrl is RadioButton)
					e.Params["value"].Value = 
						((RadioButton)ctrl).Checked;
				else if (ctrl is SelectList)
					e.Params["value"].Value = 
						((SelectList)ctrl).SelectedItem.Value;
				else if (ctrl is Label)
					e.Params["value"].Value = 
						((Label)ctrl).Text;
			}
		}

		// TODO: create plugable event
		/**
		 * Returns the given value of the given "id" widget, within the given
		 * "form-id" form, in the "value" node
		 */
        [ActiveEvent(Name = "magix.forms.set-value")]
		protected void magix_forms_set_value(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-value"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formid";
				e.Params["inspect"].Value = @"sets the value of the given 
[id] web control, in the [form-id] form, from [value]";
				return;
			}

			if (!e.Params.Contains ("form-id"))
				throw new ArgumentException("Missing form-id in set-value");

			if (!e.Params.Contains ("id"))
				throw new ArgumentException("Missing id in set-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				string value = e.Params.Contains("value") ? e.Params["value"].Get<string>("") : "";

				Control ctrl = Selector.FindControl<Control>(this, e.Params["id"].Get<string>());
				if (ctrl is BaseWebControlFormElementText)
					((BaseWebControlFormElementText)ctrl).Text = value;
				if (ctrl is Label)
					((Label)ctrl).Text = value;
				else if (ctrl is Button)
					((Button)ctrl).Text = value;
				else if (ctrl is LinkButton)
					((LinkButton)ctrl).Text = value;
				else if (ctrl is Image)
					((Image)ctrl).ImageUrl = value;
				else if (ctrl is HyperLink)
					((HyperLink)ctrl).URL = value;
				else if (ctrl is Button)
					((Button)ctrl).Text = value;
				else if (ctrl is CheckBox)
					((CheckBox)ctrl).Checked = bool.Parse (value);
				else if (ctrl is HiddenField)
					((HiddenField)ctrl).Value = value;
				else if (ctrl is RadioButton)
					((RadioButton)ctrl).Checked = bool.Parse (value);
				else if (ctrl is SelectList)
					((SelectList)ctrl).SetSelectedItemAccordingToValue(value);
				else
					throw new ArgumentException("Don't know how to set the value of that control");
			}
		}

		/**
		 * sets focus to a specific mux web control
		 */
        [ActiveEvent(Name = "magix.forms.set-focus")]
		protected void magix_forms_set_focus(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-focus"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formid";
				e.Params["inspect"].Value = @"sets focus to the specific 
[id] web control, in the [form-id] form";
				return;
			}

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("Missing form-id in set-value");

			if (!e.Params.Contains("id"))
				throw new ArgumentException("Missing id in set-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				BaseWebControl ctrl = Selector.FindControl<BaseWebControl>(this, e.Params["id"].Get<string>());
				ctrl.Focus();
			}
		}

		/**
		 * selects all text in a mux textbox or textarea
		 */
        [ActiveEvent(Name = "magix.forms.select-all")]
		protected void magix_forms_select_all(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.select-all"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "formid";
				e.Params["inspect"].Value = @"selects all text in the specific 
[id] textbox or textarea, in the [form-id] form";
				return;
			}

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("Missing form-id in set-value");

			if (!e.Params.Contains("id"))
				throw new ArgumentException("Missing id in set-value");

			if (e.Params["form-id"].Get<string>() == FormID)
			{
				BaseWebControlFormElementInputText ctrl = 
					Selector.FindControl<BaseWebControlFormElementInputText>(this, e.Params["id"].Get<string>());
				ctrl.Select();
			}
		}

		protected string GetPath(Node idxInner)
		{
			Node idxNode = idxInner;
			string retVal = "";
			while (idxNode.Parent != null)
			{
				string tmp = idxNode.Parent.IndexOf(idxNode).ToString() + "|";
				retVal = tmp + retVal;

				idxNode = idxNode.Parent;
			}
			retVal = retVal.Trim('|');
			return retVal;
		}

		protected Node GetNode(string codePath, Node source)
		{
			string[] splits = codePath.Split('|');
			Node tmp = source;
			foreach (string idx in splits)
			{
				int idxNo = int.Parse(idx);
				tmp = tmp[idxNo];
			}
			return tmp;
		}

		protected void BuildControl(Node idx, Control parent)
		{
			string typeName = idx.Name;

			bool isControlName = true;
			foreach (char idxC in typeName)
			{
				if ("abcdefghijklmnopqrstuvwxyz-".IndexOf(idxC) == -1)
				{
					isControlName = false;
					break;
				}
			}

			if (!isControlName)
				throw new ArgumentException("control '" + typeName + "' is not understood while trying to create web controls");

			string evtName = "magix.forms.controls." + typeName;

			Node node = new Node();

			node["_code"].Value = idx;
			idx["_first"].Value = isFirst;

			RaiseEvent(
				evtName,
				node);

			idx["_first"].UnTie();

			if (node.Contains("_ctrl"))
				parent.Controls.Add(node["_ctrl"].Value as Control);
			else
			{
				if (!node.Contains("_tpl"))
					throw new ArgumentException("unknown control type in your template control '" + typeName + "'");
				else
				{
					// this is a 'user control', or a 'template control', and we need to
					// individually traverse it, as if it was embedded into markup, almost
					// like copy/paste
					BuildControl(
						node["_tpl"][0], 
						parent);
				}
			}
		}
	}
}

