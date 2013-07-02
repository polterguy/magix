/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;
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
	public abstract class DynamicFormBase : ActiveModule
	{
		protected delegate Node FindNode(string path);
		protected bool isFirst;

		protected Node FormID
		{
			get
			{
				if (ViewState["FormID"] == null)
					ViewState["FormID"] = new Node();
				return ViewState["FormID"] as Node;
			}
			set { ViewState["FormID"] = value; }
		}

		protected Dictionary<string, Node> Methods
		{
			get
			{
				if (ViewState["Methods"] == null )
					ViewState["Methods"] = new Dictionary<string, Node>();
				return ViewState["Methods"] as Dictionary<string, Node>; }
		}

		public override void InitialLoading(Node node)
		{
			isFirst = true;
			Load +=
				delegate
				{
					FormID.Value = node["form-id"].Get<string>();
				};
			base.InitialLoading(node);
		}

		protected Control FindControl(Node pars)
		{
			if (!pars.Contains("form-id"))
				throw new ArgumentException("need a [form-id]");

			if (!pars.Contains("id"))
				throw new ArgumentException("need an [id]");

			if (FormID.Get<string>() == pars["form-id"].Get<string>() || FormID.Contains(pars["form-id"].Get<string>()))
			{
				//Node form = FormID[pars["form-id"].Get<string>()];
				// TODO: if template form, then start searching from actual template form, and
				// not from this.Parent, since that might give false positives
				Control ctrl = Selector.FindControl<Control>(this.Parent, pars["id"].Get<string>());
				return ctrl;
			}
			return null;
		}

		/**
		 * creates a new effect
         */
		[ActiveEvent(Name = "magix.forms.effect")]
		protected void magix_forms_effect(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"creates an effect on the 
[id] element in the viewport lasting for [time] milliseconds [joined] with and chaining
executing [chained] afterwards.&nbsp;&nbsp;
different effects have different properties.&nbsp;&nbsp;not thread safe";
				e.Params["type"].Value = "fade-in|fade-out|focus-and-select|highlight|move|roll-up|roll-down|size|slide|timeout";
				e.Params["id"].Value = "idOfMyWidget";
				e.Params["form-id"].Value = "form-id";
				e.Params["time"].Value = 500M;
				e.Params["joined"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["time"].Value = 500M;
				return;
			}

			if (FindControl(e.Params) != null)
			{
				Effect tmp = CreateEffect(e.Params);
				tmp.Render();
			}
		}

		private Effect CreateEffect(Node pars)
		{
			string id = null;
			Control ctrl = null;
			if (pars.Contains("id"))
			{
				id = pars["id"].Get<string>();
				ctrl = Selector.FindControl<Control>(this.Parent /*must include parent, which is viewport container itself, to support hiding of viewport containers*/, id);
			}

			Effect tmp = null;
			switch (pars["type"].Get<string>())
			{
				case "fade-in":
			{
				decimal frm = 0.0M;
				decimal to = 1.0M;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("from"))
					frm = pars["from"].Get<decimal>();
				if (pars.Contains("to"))
					to = pars["to"].Get<decimal>();
				tmp = new EffectFadeIn(ctrl, milliseconds, frm, to);
			} break;
				case "fade-out":
			{
				decimal frm = 1.0M;
				decimal to = 0.0M;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("from"))
					frm = pars["from"].Get<decimal>();
				if (pars.Contains("to"))
					to = pars["to"].Get<decimal>();
				tmp = new EffectFadeOut(ctrl, milliseconds, frm, to);
			} break;
				case "focus-and-select":
			{
				if (string.IsNullOrEmpty(id))
					throw new ArgumentException("focus-and-select effect needs an [id] for widget to select");
				tmp = new EffectFocusAndSelect(ctrl);
			} break;
				case "highlight":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectHighlight(ctrl, milliseconds);
			} break;
				case "move":
			{
				int left = -1;
				int top = -1;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("left"))
					left = pars["left"].Get<int>();
				if (pars.Contains("top"))
					top = pars["top"].Get<int>();
				tmp = new EffectMove(ctrl, milliseconds, left, top);
			} break;
				case "roll-up":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectRollUp(ctrl, milliseconds);
			} break;
				case "roll-down":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectRollDown(ctrl, milliseconds);
			} break;
				case "size":
			{
				int width = -1;
				int height = -1;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("width"))
					width = pars["width"].Get<int>();
				if (pars.Contains("height"))
					height = pars["height"].Get<int>();
				tmp = new EffectSize(ctrl, milliseconds, width, height);
			} break;
				case "slide":
			{
				int offset = -1;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("offset"))
					offset = pars["offset"].Get<int>();
				tmp = new EffectSlide(ctrl, milliseconds, offset);
			} break;
				case "timeout":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectTimeout(milliseconds);
			} break;
			}

			if (pars.Contains("joined"))
			{
				foreach (Node idx in pars["joined"])
				{
					Effect tp = CreateEffect(idx);
					tmp.Joined.Add(tp);
				}
			}

			if (pars.Contains("chained"))
			{
				foreach (Node idx in pars["chained"])
				{
					Effect tp = CreateEffect(idx);
					tmp.Chained.Add(tp);
				}
			}

			return tmp;
		}

		// TODO: create plugable event
		/**
		 */
		[ActiveEvent(Name = "magix.forms.set-class")]
		protected void magix_forms_set_class(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-class"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = "some-css-class";
				e.Params["inspect"].Value = @"sets the css class of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				string className = "";
				if (e.Params.Contains ("value"))
					className = e.Params["value"].Get<string>();
				if (ctrl is BaseWebControl)
					((BaseWebControl)ctrl).CssClass = className;
				else
					throw new ArgumentException("don't know how to set the css value of that control");
			}
		}

		/*
		 */
		[ActiveEvent(Name = "magix.forms.add-css")]
		protected void magix_forms_add_css(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.add-css"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["css"].Value = "css-class-to-add";
				e.Params["inspect"].Value = @"adds the given [css] to the css class name of the 
[id] control.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (!e.Params.Contains ("css"))
				throw new ArgumentException("Missing [css] in add-css");

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				BaseWebControl ctrl2 = ctrl as BaseWebControl;

				if (ctrl2 != null)
				{
					if (ctrl2.CssClass.IndexOf(e.Params["css"].Get<string>()) == -1)
					{
						ctrl2.CssClass = ctrl2.CssClass.Trim() + " " + e.Params["css"].Get<string>().Trim();
					}
				}
				else
					throw new ArgumentException("you cannot invoke add-css on that control since it is not a mux webcontrol");
			}
		}

		/*
		 */
		[ActiveEvent(Name = "magix.forms.remove-css")]
		protected void magix_forms_remove_css(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.remove-css"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["css"].Value = "css-class-to-remove";
				e.Params["inspect"].Value = @"removes the given [css] from the css class property of the 
[id] control.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (!e.Params.Contains ("css"))
				throw new ArgumentException("Missing [css] in remove-css");

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				BaseWebControl ctrl2 = ctrl as BaseWebControl;

				if (ctrl2 != null)
				{
					if (ctrl2.CssClass.IndexOf(e.Params["css"].Get<string>()) != -1)
					{
						ctrl2.CssClass = ctrl2.CssClass.Replace(e.Params["css"].Get<string>().Trim(), "").Replace("  ", " ").Trim();
					}
				}
				else
					throw new ArgumentException("you cannot invoke remove-css on that control since it is not a mux webcontrol");
			}
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
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"returns the value of the given 
[id] web control, in the [form-id] form, as [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				// TODO: rewrite, refactor into specific controllers ...
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
				else if (ctrl is Uploader)
					e.Params["value"].Value = 
						((Uploader)ctrl).GetFileName();
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
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = "new value";
				e.Params["inspect"].Value = @"sets the value of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				// TODO: refactor out to specific controllers ...
				string value = e.Params.Contains("value") ? e.Params["value"].Get<string>("") : "";

				if (ctrl is BaseWebControlFormElementText)
					((BaseWebControlFormElementText)ctrl).Text = value;
				else if (ctrl is Label)
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
				{
					((SelectList)ctrl).SetSelectedItemAccordingToValue(value);
					((SelectList)ctrl).ReRender();
				}
				else
					throw new ArgumentException("don't know how to set the value of that control");
			}
		}

		// TODO: create plugable event
		/**
		 */
		[ActiveEvent(Name = "magix.forms.set-visible")]
		protected void magix_forms_set_visible(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-visible"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"sets the visibility of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				bool visible = false;
				if (e.Params.Contains ("value"))
					visible = e.Params["value"].Get<bool>();

				ctrl.Visible = visible;
			}
		}

		// TODO: refactor out to specific controller
		/**
		 */
		[ActiveEvent(Name = "magix.forms.has-more-data")]
		protected void magix_forms_has_more_data(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-enabled"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"returns true in [value] if there is more data in the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				if (ctrl is Uploader)
					e.Params["value"].Value = ((Uploader)ctrl).SizeOfBatch > ((Uploader)ctrl).CurrentNo + 1;
				else
					throw new ArgumentException("don't know how to see if that control has more data");
			}
		}

		/**
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

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				bool enabled = false;
				if (e.Params.Contains ("value"))
					enabled = e.Params["value"].Get<bool>();

				if (ctrl is BaseWebControlFormElement)
					((BaseWebControlFormElement)ctrl).Enabled = enabled;
				else
					throw new ArgumentException("don't know how to set the value of that control");
			}
		}

		// TODO: create plugable event
		/**
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

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				if (ctrl is SelectList)
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
				else
					throw new ArgumentException("don't know how to set the values of that control");
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
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"sets focus to the specific 
[id] web control, in the [form-id] form.&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				if (ctrl is BaseWebControl)
					((BaseWebControl)ctrl).Focus();
				else
					throw new ArgumentException("con't know how to the focus to that control since it is not a mux webcontrol");
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
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"selects all text in the specific 
[id] textbox or textarea, in the [form-id] form.&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				if (ctrl is BaseWebControlFormElementInputText)
					((BaseWebControlFormElementInputText)ctrl).Select();
				else
					throw new ArgumentException("con't know how to select all text in that control, since it doesn't seem to be a textually based control");
			}
		}

		/**
		 * hides or shows a specific mux web control
		 */
		[ActiveEvent(Name = "magix.forms.show-control")]
		protected void magix_forms_show_control(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.show"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"shides or show the given [id] control in given [form-id], 
if [visible] is true, control is shown, otherwise hidden.&nbsp;&nbsp;not thread safe";
				return;
			}

			Control ctrl = FindControl(e.Params);

			if (ctrl != null)
			{
				if (e.Params.Contains("visible") && e.Params ["visible"].Get<bool>())
					ctrl.Visible = true;
				else
					ctrl.Visible = false;
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

		[ActiveEvent(Name = "")]
		protected void magix_null_event_handler(object sender, ActiveEventArgs e)
		{
			if (Methods.ContainsKey(e.Name))
			{
				Node tmp = Methods[e.Name].Clone();

				// cloning in the incoming parameters
				tmp["$"].AddRange(e.Params.Clone());

				RaiseEvent(
					"magix.execute",
					tmp);
			}
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
			{
				// assuming it's an event override method
				Methods[typeName] = idx.Clone();
				return;
			}

			string evtName = "magix.forms.controls." + typeName;

			Node node = new Node();

			node["_code"].Value = idx;
			idx["_first"].Value = isFirst;

			RaiseEvent(
				evtName,
				node);

			if (node.Contains("_ctrl"))
			{
				if (node["_ctrl"].Value != null)
					parent.Controls.Add(node["_ctrl"].Value as Control);
				else
				{
					// multiple controls returned ...
					foreach (Node idxCtrl in node["_ctrl"])
					{
						parent.Controls.Add(idxCtrl.Value as Control);
					}
				}
			}
			else
			{
				if (!node.Contains("_tpl"))
					throw new ArgumentException("unknown control type in your template control '" + typeName + "'");
				else
				{
					// this is a 'user control', or a 'template control', and we need to
					// individually traverse it, as if it was embedded into markup, almost
					// like copy/paste

					if (node["_tpl"].Value != null)
						FormID[node["_tpl"].Get<string>()].Value = null;

					Panel wr = new Panel();
					wr.ID = node["_tpl"]["id"].Get<string>("mumbo");
					wr.CssClass = node["_tpl"]["css"].Get<string>("");
					wr.Tag = node["_tpl"]["tag"].Get<string>("div");
					foreach (Node idxInner in node["_tpl"])
					{
						if (idxInner.Name == "id")
							continue;
						if (idxInner.Name == "css")
							continue;
						if (idxInner.Name == "tag")
							continue;
						BuildControl(
							idxInner, 
							wr);
					}
					parent.Controls.Add(wr);
				}
			}
		}

		protected abstract void ReRender();
		
		/**
		 * re renders the given [form-id]
		 */
		[ActiveEvent(Name = "magix.forms.re-render")]
		protected void magix_forms_re_render(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.re-render"].Value = null;
				e.Params["inspect"].Value = @"re renders the
form with the [form-id].&nbsp;&nbsp;not thread safe";
				e.Params["form-id"].Value = "header";
				return;
			}

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("need a [form-id] to re-render");

			if (FormID.Get<string>() == e.Params["form-id"].Get<string>() || FormID.Contains(e.Params["form-id"].Get<string>()))
			{
				ReRender();
			}
		}
	}
}

