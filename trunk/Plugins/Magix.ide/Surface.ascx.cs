/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
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

namespace Magix.ide
{
    /**
     * ide surface module for visually building magix markup language forms
     */
    public class Surface : ActiveModule
    {
		protected Panel wrp;

		private Node DataSource
		{
			get { return ViewState["DataSource"] as Node; }
			set { ViewState["DataSource"] = value; }
		}

		private Node ClipBoard
		{
			get { return ViewState["ClipBoard"] as Node; }
			set { ViewState["ClipBoard"] = value; }
		}

		private string SelectedWidgetDna
		{
			get { return ViewState["SelectedWidgetDna"] as string; }
			set { ViewState["SelectedWidgetDna"] = value; }
		}

		public override void InitialLoading(Node node)
		{
			base.InitialLoading(node);

			if (node.Contains("form"))
			{
				Load +=
					delegate
					{
						DataSource = node["form"].Clone();
						BuildForm();
					};
			}
			else
			{
				Load +=
					delegate
					{
						DataSource = new Node("surface");
						DataSource["controls"].Value = null;
						BuildForm();
					};
			}
		}

		protected override void OnLoad (EventArgs e)
		{
			if (!FirstLoad)
				BuildForm();
			base.OnLoad(e);
		}

		private void BuildForm()
		{
			wrp.Controls.Clear();
			if (DataSource.Contains("controls"))
			{
				foreach (Node idx in DataSource["controls"])
				{
					BuildWidget(idx, wrp);
				}
			}
		}

		private void BuildWidget(Node widget, Control parent)
		{
			if (!widget.Contains("type"))
				throw new ArgumentException("need a [type] for your [widget]");

			string type = widget["type"].Get<string>();

			Node tmp = new Node();
			if (widget.Contains("properties"))
			{
				foreach (Node idx in widget["properties"])
				{
					// we don't do event handlers here, since this is 'design view' ...
					if (!idx.Name.StartsWith("on"))
						tmp[idx.Name].Value = idx.Value;
				}
			}

			// click event handler for selecting widget
			tmp["onclick"]["select-widget"]["dna"].Value = widget.Dna;

			Node ctrlRaise = new Node();
			ctrlRaise["_code"].Value = tmp;

			RaiseActiveEvent(
				type,
				ctrlRaise);

			if ((!ctrlRaise.Contains("_ctrl") || ctrlRaise["_ctrl"].Value == null) && !ctrlRaise.Contains("_tpl"))
				return;

			if (ctrlRaise.Contains("_ctrl"))
			{
				Control ctrl = ctrlRaise["_ctrl"].Value as Control;

				if (widget.Dna == SelectedWidgetDna)
				{
					if (ctrl is BaseWebControl)
					{
						(ctrl as BaseWebControl).CssClass += " selected";
					}
				}

				if (widget.Contains("controls"))
				{
					foreach (Node idx in widget["controls"])
					{
						BuildWidget(idx, ctrl);
					}
				}

				parent.Controls.Add(ctrl);
			}
			else
			{
				foreach (Node idx in ctrlRaise["_tpl"])
				{
					BuildWidget(idx, parent);
				}
			}
		}

		protected void wrp_Click(object sender, EventArgs e)
		{
			SelectedWidgetDna = null;

			BuildForm();
			wrp.ReRender();

			RaiseActiveEvent("magix.ide.surface-changed");
			RaiseActiveEvent("magix.ide.widget-selected");
		}

		/**
		 * lists widgets in form object
		 */
		[ActiveEvent(Name="magix.execute.list-widgets")]
		protected void magix_execute_list_widgets(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"lists all widgets as [widgets] in currently edited form.&nbsp;&nbsp;
not thread safe";
				e.Params["list-widgets"].Value = null;
				return;
			}

			Node retVal = new Node("widgets");

			if (DataSource.Contains("controls"))
			{
				foreach (Node idx in DataSource["controls"])
				{
					Node tp = new Node("widget");
					retVal.Add(tp);
					GetWidget(idx, tp);
				}
			}

            (e.Params["_ip"].Value as Node).Add(retVal); /*error here ... doesn't list controls of panels ...*/
		}

		private void GetWidget(Node widgetNode, Node output)
		{
			output["type"].Value = widgetNode["type"].Get<string>();
			output["dna"].Value = widgetNode.Dna;

			if (widgetNode.Contains("properties"))
			{
				foreach (Node idx in widgetNode["properties"])
				{
					output["properties"][idx.Name].Value = idx.Value;
				}
			}

			if (widgetNode.Contains("controls"))
			{
				foreach (Node idx in widgetNode["controls"])
				{
					Node tp = new Node("widget");
					output.Parent.Add(tp);
					GetWidget(idx, tp);
				}
			}
		}

		/**
		 * adds widget to surface
		 */
		[ActiveEvent(Name="magix.execute.add-widget")]
		protected void magix_execute_add_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"adds a widget to the currently viewed form.&nbsp;&nbsp;
not thread safe";
				e.Params["add-widget"]["auto-select"].Value = true;
				e.Params["add-widget"]["where"]["dna"].Value = "root";
				e.Params["add-widget"]["where"]["position"].Value = "before|after|child";
				e.Params["add-widget"]["widget"]["type"].Value = "magix.forms.controls.installed-widget";
				e.Params["add-widget"]["widget"]["properties"]["id"].Value = "myWidget";
				e.Params["add-widget"]["widget"]["properties"]["text"].Value = "hello world";
				return;
			}

            Node ip = Ip(e.Params);

			if (!ip.Contains("widget"))
				throw new ArgumentException("you need a [widget] for add-widget");

			if (!ip["widget"].Contains("type"))
				throw new ArgumentException("you need a [type] for [widget] in add-widget");

			if (!ip.Contains("where"))
				throw new ArgumentException("you need a [where] for add-widget");

			if (!ip["where"].Contains("dna"))
				throw new ArgumentException("you need a [dna] for [where] in add-widget");

			if (!ip["where"].Contains("position"))
				throw new ArgumentException("you need a [position] for [where] in add-widget");

			string dna 		= ip["where"]["dna"].Get<string>() ?? "root";
			string position = ip["where"]["position"].Get<string>();

			Node whereNode = DataSource.FindDna(dna);

			Node widgetNode = ip["widget"].Clone();

			if (!widgetNode.Contains("properties") || 
			    !widgetNode["properties"].Contains("id") || 
			    string.IsNullOrEmpty(widgetNode["properties"]["id"].Get<string>()))
				widgetNode["properties"]["id"].Value = "g" + Guid.NewGuid().ToString().Replace("-", "");

			// root can only have children, no after or before widgets
			if (dna == "root")
				position = "child";

			switch (position)
			{
			case "before":
				whereNode.AddBefore(widgetNode);
				break;
			case "after":
				whereNode.AddAfter(widgetNode);
				break;
			case "child":
			{
				if (whereNode.Name == "surface")
				{
					whereNode["controls"].Add(widgetNode);
				}
				else
				{
					Node tp = new Node();
					tp["inspect"].Value = null;

					RaiseActiveEvent(
						whereNode["type"].Get<string>(),
						tp);

					if (tp["controls"][0].Contains("controls"))
						whereNode["controls"].Add(widgetNode);
					else
						whereNode.AddAfter(widgetNode);
				}
			} break;
			default:
				throw new ArgumentException("sorry, don't know where " + ip["position"].Get<string>() + " is");
			}

			if (ip.Contains("auto-select") && ip["auto-select"].Get<bool>())
			{
				SelectedWidgetDna = widgetNode.Dna;

				BuildForm();
				wrp.ReRender();
				
				RaiseActiveEvent("magix.ide.surface-changed");

				Node tp = new Node();
				tp["dna"].Value = SelectedWidgetDna;

				RaiseActiveEvent(
					"magix.ide.widget-selected",
					tp);
			}
			else
			{
				BuildForm();
				wrp.ReRender();
				
				RaiseActiveEvent("magix.ide.surface-changed");
			}
		}

		/**
		 * removes widget from surface
		 */
		[ActiveEvent(Name="magix.execute.remove-widget")]
		protected void magix_execute_remove_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"removes the [dna] widget from the currently viewed form.&nbsp;&nbsp;
not thread safe";
				e.Params["remove-widget"]["dna"].Value = "root-0";
				return;
			}

            Node ip = Ip(e.Params);

			if (!ip.Contains("dna"))
				throw new ArgumentException("you need a [dna] for remove-widget");

			string dna = ip["dna"].Get<string>();

			Node whereNode = DataSource.FindDna(dna);

			whereNode.Parent.Remove(whereNode);

			BuildForm();
			wrp.ReRender();
			
			RaiseActiveEvent("magix.ide.surface-changed");
		}

		/**
		 * changes properties of widget
		 */
		[ActiveEvent(Name="magix.execute.change-widget")]
		protected void magix_execute_change_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"changes the [dna] widget, adding/changing [change] properties and 
removing [remove] properties.&nbsp;&nbsp;not thread safe";
				e.Params["change-widget"]["dna"].Value = "root-0-0";
				e.Params["change-widget"]["change"]["id"].Value = "myId";
				e.Params["change-widget"]["remove"]["css"].Value = null;
				return;
			}

            Node ip = Ip(e.Params);

			if (!ip.Contains("change") && !ip.Contains("remove"))
				throw new ArgumentException("you need a [change] and/or a [remove] for change-widget to signalize which properties are to be updated");

			Node widgetNode = DataSource.FindDna(ip["dna"].Get<string>());

			if (ip.Contains("change"))
			{
				foreach (Node idx in ip["change"])
				{
					// code in text format
					if ((idx.Name.StartsWith("on") && !string.IsNullOrEmpty(idx.Get<string>())) ||
					    idx.Name == "items" || idx.Name == "controls")
					{
						Node tmp = new Node();

						tmp["code"].Value = idx.Get<string>();

						RaiseActiveEvent(
							"magix.execute.code-2-node",
							tmp);

						widgetNode["properties"][idx.Name].Clear();
						widgetNode["properties"][idx.Name].AddRange(tmp["node"].Clone());
					}
					else if (idx.Name.StartsWith("on") && string.IsNullOrEmpty(idx.Get<string>()))
					{
						// code in tree format
						widgetNode["properties"][idx.Name].AddRange(idx.Clone());
					}
					else
					{
						if (idx.Value == null || idx.Get<string>() == "")
							widgetNode["properties"][idx.Name].UnTie();
						else
							widgetNode["properties"][idx.Name].Value = idx.Value;
					}
				}
			}

			if (ip.Contains("remove"))
			{
				foreach (Node idx in ip["remove"])
				{
					widgetNode["properties"][idx.Name].UnTie();
				}
			}

			BuildForm();
			wrp.ReRender();
		}

		/**
		 * moves a widget from its current position
		 */
		[ActiveEvent(Name="magix.execute.move-widget")]
		protected void magix_execute_move_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"moves the [dna] widget to the [position] of the [to] dna.&nbsp;&nbsp;
not thread safe";
				e.Params["move-widget"]["dna"].Value = "root-0-0";
				e.Params["move-widget"]["position"].Value = "before|after|child";
				e.Params["move-widget"]["to"].Value = "root-0-0";
				return;
			}

            Node ip = Ip(e.Params);

			if (!ip.Contains("dna"))
				throw new ArgumentException("you need a [dna] for move-widget");

			if (!ip.Contains("to"))
				throw new ArgumentException("you need a [to] for move-widget");

			if (!ip.Contains("position"))
				throw new ArgumentException("you need a [position] for move-widget");

			Node widgetNode = DataSource.FindDna(ip["dna"].Get<string>()).UnTie();

			switch (ip["position"].Get<string>())
			{
			case "before":
				DataSource.FindDna(ip["to"].Get<string>()).AddBefore(widgetNode);
				break;
			case "after":
				DataSource.FindDna(ip["to"].Get<string>()).AddAfter(widgetNode);
				break;
			case "child":
				DataSource.FindDna(ip["to"].Get<string>()).Add(widgetNode);
				break;
			default:
				throw new ArgumentException("sorry, don't know where " + ip["position"].Get<string>() + " is");
			}

			BuildForm();
			wrp.ReRender();
			
			RaiseActiveEvent("magix.ide.surface-changed");
		}

		/**
		 * selects widget
		 */
		[ActiveEvent(Name="magix.execute.select-widget")]
		protected void magix_execute_select_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"sets the [dna] widget as the actively selected widget.&nbsp;&nbsp;
if no [dna] is given, no widget is set as the currently selected.&nbsp;&nbsp;
raises the magix.ide.widget-selected active event when done.&nbsp;&nbsp;not thread safe";
				e.Params["select-widget"]["dna"].Value = "root-0-0";
				return;
			}

			Node ip = Ip(e.Params);

			if (!ip.Contains("dna"))
				SelectedWidgetDna = null;
			else
			{
				SelectedWidgetDna = ip["dna"].Get<string>();
			}

			BuildForm();
			wrp.ReRender();

			Node tp = new Node();
			tp["dna"].Value = SelectedWidgetDna;

			RaiseActiveEvent(
				"magix.ide.widget-selected",
				tp);
		}

		/**
		 * returns selected widget
		 */
		[ActiveEvent(Name="magix.execute.get-selected-widget")]
		protected void magix_execute_get_selected_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the currently selected widget as [value], if any.&nbsp;&nbsp;
not thread safe";
				e.Params["get-selected-widget"].Value = null;
				return;
			}

			Node ip = Ip(e.Params);

			if (!string.IsNullOrEmpty(SelectedWidgetDna))
			{
				ip["value"]["widget"].ReplaceChildren(DataSource.FindDna(SelectedWidgetDna).Clone());
				ip["value"]["dna"].Value = DataSource.FindDna(SelectedWidgetDna).Dna;
			}
		}

		/**
		 * copies widget into clipboard
		 */
		[ActiveEvent(Name="magix.execute.copy-widget")]
		protected void magix_execute_copy_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"copies the currently selected widget into the clipboard.&nbsp;&nbsp;
not thread safe";
				e.Params["copy-widget"].Value = null;
				return;
			}

			if (SelectedWidgetDna == null)
				throw new ArgumentException("no widget currently selected, you must select widget before you can copy it");

			ClipBoard = DataSource.FindDna(SelectedWidgetDna).Clone();
			ClipBoard["properties"]["id"].Value = Guid.NewGuid().ToString().Replace("-", "");
		}

		/**
		 * cuts widget into clipboard
		 */
		[ActiveEvent(Name="magix.execute.cut-widget")]
		protected void magix_execute_cut_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"cuts out the currently selected widget and puts it into the clipboard.&nbsp;&nbsp;
not thread safe";
				e.Params["cut-widget"]["dna"].Value = "root-0-0";
				return;
			}

			if (SelectedWidgetDna == null)
				throw new ArgumentException("no widget currently selected, you must select widget before you can cut it");

			RaiseActiveEvent(
				"magix.execute.copy-widget",
                Ip(e.Params));

			ClipBoard = DataSource.FindDna(SelectedWidgetDna).UnTie();
			ClipBoard["properties"]["id"].Value = Guid.NewGuid().ToString().Replace("-", "");

			SelectedWidgetDna = null;

			BuildForm();
			wrp.ReRender();
			
			RaiseActiveEvent("magix.ide.surface-changed");
		}

		/**
		 * pastes widget
		 */
		[ActiveEvent(Name="magix.execute.paste-widget")]
		protected void magix_execute_paste_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"pastes in the widget from the clipboard to the selected widget.&nbsp;&nbsp;
use [position] to signify the relationship between the node being pasted and the position being pasted into, 
default is child, meaning if control can contain children, it will be a child, otherwise it will 
be pasted into the control tree behind the currently selected widget.&nbsp;&nbsp;
not thread safe";
				return;
			}

            Node ip = Ip(e.Params);

			if (ClipBoard == null)
				throw new ArgumentException("cannot paste a widget, since there are no widgets on the clipboard");

			Node destinationNode = DataSource["controls"];

			if (!string.IsNullOrEmpty(SelectedWidgetDna))
				destinationNode = DataSource.FindDna(SelectedWidgetDna);

			string position = "child";

			if (ip.Contains("position"))
				position = ip["position"].Get<string>();

			Node toAdd = ClipBoard.Clone();

			toAdd["properties"]["id"].Value = Guid.NewGuid().ToString().Replace("-", "");

			switch (position)
			{
			case "before":
				destinationNode.AddBefore(toAdd);
				break;
			case "after":
				destinationNode.AddAfter(toAdd);
				break;
			case "child":
			{
				if (!destinationNode.Contains("type"))
				{
					// main surface ...
					destinationNode.Add(toAdd);
				}
				else
				{
					Node tp = new Node();
					tp["inspect"].Value = null;

					RaiseActiveEvent(
						destinationNode["type"].Get<string>(),
						tp);

					if (tp["controls"][0].Contains("controls"))
						destinationNode["controls"].Add(toAdd);
					else
						destinationNode.AddAfter(toAdd); // defaulting to add 'after'
				}
			} break;
			default:
				throw new ArgumentException("sorry, don't know where " + position + " is");
			}

			if (ip.Contains("auto-select") && ip["auto-select"].Get<bool>())
			{
				SelectedWidgetDna = toAdd.Dna;

				BuildForm();
				wrp.ReRender();
				
				RaiseActiveEvent("magix.ide.surface-changed");

				Node tp = new Node();
				tp["dna"].Value = SelectedWidgetDna;

				RaiseActiveEvent(
					"magix.ide.widget-selected",
					tp);
			}
			else
			{
				BuildForm();
				wrp.ReRender();
				
				RaiseActiveEvent("magix.ide.surface-changed");
			}
		}

		/**
		 * saves given form
		 */
		[ActiveEvent(Name="magix.execute.save-form")]
		protected void magix_execute_save_form(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"saves the currently loaded form as [name].&nbsp;&nbsp;
not thread safe";
				e.Params["save-form"]["name"].Value = "name-of-form";
				return;
			}

            Node ip = Ip(e.Params);

			if (!ip.Contains("name"))
				throw new ArgumentException("no [name] given to save form as");

			Node tp = new Node();

			tp["prototype"]["type"].Value = "magix.forms.form";
			tp["prototype"]["name"].Value = ip["name"].Get<string>();

			RaiseActiveEvent(
				"magix.data.load",
				tp);

			Node tmp = new Node();

			if (tp.Contains("objects") && tp["objects"].Count > 0)
			{
				tmp["id"].Value = tp["objects"][0].Name;
			}

			tmp["value"]["form"].Add(DataSource.Clone());
			tmp["value"]["type"].Value = "magix.forms.form";
			tmp["value"]["name"].Value = ip["name"].Get<string>();

			RaiseActiveEvent(
				"magix.data.save",
				tmp);
		}

		/**
		 * loads a form into the surface
		 */
		[ActiveEvent(Name="magix.execute.load-form")]
		protected void magix_execute_load_form(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"loads the given [name] form.&nbsp;&nbsp;
not thread safe";
				e.Params["load-form"]["name"].Value = "name-of-form";
				return;
			}

            Node ip = Ip(e.Params);

			if (!ip.Contains("name"))
				throw new ArgumentException("no [name] given, don't know which form to load");

			if (string.IsNullOrEmpty(ip["name"].Get<string>()))
			{
				SelectedWidgetDna = null;

				DataSource = new Node("surface");
				DataSource["controls"].Value = null;

				BuildForm();
				wrp.ReRender();
				RaiseActiveEvent("magix.ide.surface-changed");
				return;
			}

			Node tmp = new Node();

			tmp["prototype"]["name"].Value = ip["name"].Get<string>();
			tmp["prototype"]["type"].Value = "magix.forms.form";

			RaiseActiveEvent(
				"magix.data.load",
				tmp);

			if (tmp.Contains("objects") && tmp["objects"].Count > 0)
			{
				DataSource = tmp["objects"][0]["form"][0].Clone();

				SelectedWidgetDna = null;

				BuildForm();
				wrp.ReRender();
			}
			else
			{
				Node txt = new Node();
				txt["message"].Value = "no such form";

				RaiseActiveEvent(
					"magix.viewport.show-message",
					txt);
			}

			RaiseActiveEvent("magix.ide.surface-changed");
		}

		/**
		 * returns the given form
		 */
		[ActiveEvent(Name="magix.execute.get-form")]
		protected void magix_execute_get_form(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns the current form node 
back to caller as [form]";
				e.Params["get-form"].Value = null;
				return;
			}

            Node ip = Ip(e.Params);

			Node retVal = new Node();

			foreach (Node idx in DataSource["controls"])
			{
				BuildControl(retVal["_tmp"], idx);
			}

			ip["controls"].ReplaceChildren(retVal);
		}

		private void BuildControl(Node retVal, Node ctrlNode)
		{
			string type = ctrlNode["type"].Get<string>();
			type = type.Substring(type.LastIndexOf(".") + 1);
			retVal.Name = type;

			foreach (Node idx in ctrlNode["properties"])
			{
				retVal[idx.Name].Value = idx.Value;
				retVal[idx.Name].ReplaceChildren(idx.Clone());
			}

			foreach (Node idx in ctrlNode["controls"])
			{
				BuildControl(retVal["controls"]["_tmp"], idx);
			}
		}
	}
}















