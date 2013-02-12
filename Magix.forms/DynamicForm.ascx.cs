/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
    /**
     * Active module encapsulating a dynamically created form
     */
	[ActiveModule]
    public class DynamicForm : ActiveModule
    {
        protected Panel pnl;

		private Node DataSource
		{
			get { return ViewState["DataSource"] as Node; }
			set { ViewState["DataSource"] = value; }
		}

		private string FormID
		{
			get { return ViewState["FormID"] as string; }
			set { ViewState["FormID"] = value; }
		}

		public override void InitialLoading (Node node)
		{
			Load +=
				delegate
				{
					DataSource = node.Clone ();
					FormID = node["form-id"].Get<string>();
				};
			base.InitialLoading (node);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad (e);
			BuildControls();
		}

		void BuildControls ()
		{
			if (!DataSource.Contains ("controls"))
				throw new ArgumentException("Couldn't find any 'controls' node underneath form");
			foreach (Node idx in DataSource["controls"])
			{
				BuildControl(idx, pnl);
			}
		}

		private string GetPath (Node idxInner)
		{
			Node idxNode = idxInner;
			string retVal = "";
			while (idxNode.Parent != null)
			{
				string tmp = idxNode.Parent.IndexOf (idxNode).ToString () + "|";
				retVal = tmp + retVal;

				idxNode = idxNode.Parent;
			}
			retVal = retVal.Trim ('|');
			return retVal;
		}

		private Node GetNode (string codePath)
		{
			string[] splits = codePath.Split ('|');
			Node tmp = DataSource;
			foreach (string idx in splits)
			{
				int idxNo = int.Parse (idx);
				tmp = tmp[idxNo];
			}
			return tmp;
		}

		// TODO: Refactor!
		void BuildControl (Node idx, BaseWebControl parent)
		{
			BaseControl ctrl = null;
			switch (idx.Name)
			{
			case "Button":
				ctrl = new Button();
				break;
			case "CheckBox":
				ctrl = new CheckBox();
				break;
			case "HiddenField":
				ctrl = new HiddenField();
				break;
			case "HyperLink":
				ctrl = new HyperLink();
				break;
			case "Image":
				ctrl = new Image();
				break;
			case "Label":
				ctrl = new Label();
				break;
			case "LinkButton":
				ctrl = new LinkButton();
				break;
			case "Panel":
				ctrl = new Panel();
				break;
			case "RadioButton":
				ctrl = new RadioButton();
				break;
			case "SelectList":
				ctrl = new SelectList();
				break;
			case "TextArea":
				ctrl = new TextArea();
				break;
			case "TextBox":
				ctrl = new TextBox();
				break;
			default:
				bool isHtml = true;
				foreach (char idxC in idx.Name)
				{
					if ("abcdefghijklmnopqrstuvwxyz".IndexOf (idxC) == -1)
					{
						isHtml = false;
					}
				}
				if (isHtml)
				{
					ctrl = new Label();
					((Label)ctrl).Tag = idx.Name;
				}
				else
					throw new ArgumentException("Unknown type of tag in creating form; " + idx.Name);
				break;
			}
			if (idx.Value != null)
				ctrl.ID = idx.Get<string>();
			foreach (Node idxInner in idx)
			{
				switch (idxInner.Name)
				{
				case "Text":
					if (ctrl is Label)
						((Label)ctrl).Text = idxInner.Get<string>();
					else if (ctrl is HyperLink)
						((HyperLink)ctrl).Text = idxInner.Get<string>();
					else
						((BaseWebControlFormElementText)ctrl).Text = idxInner.Get<string>();
					break;
				case "Enabled":
					if (ctrl is BaseWebControlFormElement)
						((BaseWebControlFormElement)ctrl).Enabled = idxInner.Get<bool>();
					else
						throw new ArgumentException("Tried to set Enabled on a control that does not support it; " + ctrl.GetType ().Name);
					break;
				case "SelectedIndex":
					if (ctrl is BaseWebControlListFormElement)
						((BaseWebControlListFormElement)ctrl).SelectedIndex = idxInner.Get<int>();
					else
						throw new ArgumentException("Tried to set SelectedIndex on a control that does not support it; " + ctrl.GetType ().Name);
					break;
				case "CssClass":
					((BaseWebControl)ctrl).CssClass = idxInner.Get<string>();
					break;
				case "OnSelectedIndexChanged":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_SelectedIndexChanged"] = path;
							((SelectList)ctrl).SelectedIndexChanged +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_SelectedIndexChanged"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnEnterPressed":
				{
					string path = GetPath (idxInner);
					((TextBox)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_EnterPressed"] = path;
							((TextBox)ctrl).EnterPressed +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EnterPressed"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnTextChanged":
				{
					string path = GetPath (idxInner);
					((BaseWebControlFormElementInputText)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_TextChanged"] = path;
							((BaseWebControlFormElementInputText)ctrl).TextChanged +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_TextChanged"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnCheckedChanged":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_CheckedChanged"] = path;
							if (ctrl is CheckBox)
							{
								((CheckBox)ctrl).CheckedChanged +=
									delegate
									{
										string codePath = ViewState[((Control)sender).ClientID + "_CheckedChanged"] as string;
										Node codeNode = GetNode (codePath);
										RaiseEvent ("magix.execute", codeNode);
									};
							}
							else if (ctrl is RadioButton)
							{
								((RadioButton)ctrl).CheckedChanged +=
									delegate
									{
										string codePath = ViewState[((Control)sender).ClientID + "_CheckedChanged"] as string;
										Node codeNode = GetNode (codePath);
										RaiseEvent ("magix.execute", codeNode);
									};
							}
						else
							throw new ArgumentException("OnCheckedChanged on something that's not a RadioButton or a CheckBox ...???");
						};
				} break;
				case "OnClick":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_Click"] = path;
							((BaseWebControl)ctrl).Click +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_Click"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnDblClick":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_DblClick"] = path;
							((BaseWebControl)ctrl).DblClick +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_DblClick"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseOver":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseOver"] = path;
							((BaseWebControl)ctrl).MouseOver +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseOver"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseOut":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseOut"] = path;
							((BaseWebControl)ctrl).MouseOut +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseOut"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseDown":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseDown"] = path;
							((BaseWebControl)ctrl).MouseDown +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseDown"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnMouseUp":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_MouseUp"] = path;
							((BaseWebControl)ctrl).MouseUp +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_MouseUp"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnKeyPress":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_KeyPress"] = path;
							((BaseWebControl)ctrl).KeyPress +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_KeyPress"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnEscKey":
				{
					string path = GetPath (idxInner);
					((BaseWebControl)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_EscKey"] = path;
							((BaseWebControl)ctrl).EscKey +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EscKey"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnFocused":
				{
					string path = GetPath (idxInner);
					((BaseWebControlFormElement)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_OnFocused"] = path;
							((BaseWebControlFormElement)ctrl).Focused +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EscKey"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "OnBlur":
				{
					string path = GetPath (idxInner);
					((BaseWebControlFormElement)ctrl).Load +=
						delegate(object sender, EventArgs e)
						{
							ViewState[ctrl.ClientID + "_OnBlur"] = path;
							((BaseWebControlFormElement)ctrl).Blur +=
								delegate
								{
									string codePath = ViewState[((Control)sender).ClientID + "_EscKey"] as string;
									Node codeNode = GetNode (codePath);
									RaiseEvent ("magix.execute", codeNode);
								};
						};
				} break;
				case "ID":
					ctrl.ID = idxInner.Get<string>();
					break;
				case "Dir":
					((BaseWebControl)ctrl).Dir = idxInner.Get<string>();
					break;
				case "ToolTip":
					((BaseWebControl)ctrl).ToolTip = idxInner.Get<string>();
					break;
				case "TabIndex":
					((BaseWebControl)ctrl).TabIndex = idxInner.Get<string>();
					break;
				case "AccessKey":
					if (ctrl is BaseWebControlFormElement)
						((BaseWebControlFormElement)ctrl).AccessKey = idxInner.Get<string>();
					break;
				case "Checked":
					if (ctrl is CheckBox)
						((CheckBox)ctrl).Checked = bool.Parse (idxInner.Get<string>());
					else
						((RadioButton)ctrl).Checked = bool.Parse (idxInner.Get<string>());
					break;
				case "Tag":
					if (ctrl is Label)
						((Label)ctrl).Tag = idxInner.Get<string>();
					else
						((Panel)ctrl).Tag = idxInner.Get<string>();
					break;
				case "For":
					((Label)ctrl).For = idxInner.Get<string>();
					break;
				case "Value":
					((HiddenField)ctrl).Value = idxInner.Get<string>();
					break;
				case "URL":
					((HyperLink)ctrl).URL = idxInner.Get<string>();
					break;
				case "Target":
					((HyperLink)ctrl).Target = idxInner.Get<string>();
					break;
				case "ImageUrl":
					((Image)ctrl).ImageUrl = idxInner.Get<string>();
					break;
				case "AlternateText":
					((Image)ctrl).AlternateText = idxInner.Get<string>();
					break;
				case "Items":
					foreach (Node idxChild in idxInner)
					{
						((SelectList)ctrl).Items.Add (new ListItem(idxChild.Name, idxChild.Get<string>()));
					}
					break;
				case "PlaceHolder":
					if (ctrl is TextBox)
						((TextBox)ctrl).PlaceHolder = idxInner.Get<string>();
					else
						((TextArea)ctrl).PlaceHolder = idxInner.Get<string>();
					break;
				case "DefaultWidget":
					if (ctrl is Panel)
						((Panel)ctrl).DefaultWidget = idxInner.Get<string>();
					else
						throw new ArgumentException("Only Panels can have a Default Widget, you tried to set DefaultWidget on a " + ctrl.GetType().Name);
					break;
				case "controls":
					foreach (Node idxChild in idxInner)
					{
						BuildControl (idxChild, (BaseWebControl)ctrl);
					}
					break;
				}
			}

			parent.Controls.Add (ctrl);
		}

		/**
		 * Returns the given value of the given "id" widget, within the given
		 * "form-id" form, in the "value" node
		 */
        [ActiveEvent(Name = "magix.forms.get-value")]
		protected void magix_forms_get_value (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["id"].Value = "idOfControlInForm";
				e.Params["form-id"].Value = "formID";
				e.Params["inspect"].Value = @"Expects a ""form-id"" and an ""id""
node, which if the ""form-id"" node matches the form id of this form, used
for looking up the Value of the given web control with the given ""id"".
The value of the web control will be returned in the ""value"" node, and might
not necessarily be a string, but can also be of other types.";
				return;
			}
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
			}
		}
    }
}
