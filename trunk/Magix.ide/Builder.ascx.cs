/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
     * ide module for visually building magix markup language forms
     */
    public class Builder : ActiveModule
    {
		protected Panel wrp;
		protected TextArea console;
		protected TextArea output;

		private Node DataSource
		{
			get { return ViewState["DataSource"] as Node; }
			set { ViewState["DataSource"] = value; }
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
					};
				BuildForm();
			}
			else
			{
				Load +=
					delegate
					{
						DataSource = new Node();
					};
			}
		}

		private void BuildForm()
		{
			if (DataSource.Contains("controls"))
			{
				foreach (Node idx in DataSource["controls"])
				{
					BuildWidget(idx, wrp);
				}
			}
			wrp.ReRender();
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
					tmp[idx.Name].Value = idx.Value;
				}
			}

			Node ctrlRaise = new Node();
			ctrlRaise["_code"].Value = tmp;

			RaiseEvent(
				type,
				ctrlRaise);

			if (!ctrlRaise.Contains("_ctrl"))
				throw new ArgumentException("[type] '" + type + "' does not exist");

			Control ctrl = ctrlRaise["_ctrl"].Value as Control;

			if (widget.Contains("controls"))
			{
				foreach (Node idx in widget["controls"])
				{
					BuildWidget(idx, ctrl);
				}
			}

			parent.Controls.Add(ctrl);
		}

		protected void run_Click(object sender, EventArgs e)
		{
			if (console.Text != "")
			{
				string wholeTxt = console.Text;

				Node tmp = new Node();
				tmp["code"].Value = wholeTxt;

				RaiseEvent(
					"magix.code.code-2-node",
					tmp);

				RaiseEvent(
					"magix.execute", 
					tmp["json"].Get<Node>());

				RaiseEvent(
					"magix.code.node-2-code", 
					tmp);

				output.Text = tmp["code"].Get<string>();
			}
		}

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
					GetWidget(idx, retVal);
				}
			}

			(e.Params["_ip"].Value as Node).Add(retVal);
		}

		private void GetWidget(Node widgetNode, Node output)
		{
			Node retVal = new Node("widget");

			retVal["type"].Value = widgetNode["type"].Get<string>();
			retVal["dna"].Value = widgetNode.Dna;

			if (widgetNode.Contains("properties"))
			{
				foreach (Node idx in widgetNode["properties"])
				{
					retVal["properties"][idx.Name].Value = idx.Value;
				}
			}

			output.Add(retVal);

			if (widgetNode.Contains("controls"))
			{
				foreach (Node idx in widgetNode["controls"])
				{
					GetWidget(idx, output);
				}
			}
		}

		[ActiveEvent(Name="magix.execute.list-widget-types")]
		protected void magix_execute_list_widget_types(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"lists all widget types as [types] available in system.&nbsp;&nbsp;
not thread safe";
				e.Params["list-widget-types"].Value = null;
				return;
			}

			Node tmp = new Node();
			tmp["begins-with"].Value = "magix.forms.controls.";

			RaiseEvent(
				"magix.admin.get-active-events",
				tmp);

			Node ip = e.Params["_ip"].Get<Node>();

			foreach (Node idx in tmp["events"])
			{
				Node tp = new Node("widget");
				tp["type"].Value = idx.Get<string>();

				Node tp2 = new Node();
				tp2["inspect"].Value = null;

				RaiseEvent(
					idx.Get<string>(),
					tp2);

				foreach (Node idx2 in tp2["controls"][0])
				{
					tp["properties"][idx2.Name].Value = idx2.Value;
				}

				ip["types"].Add(tp);
			}
		}

		[ActiveEvent(Name="magix.execute.add-widget")]
		protected void magix_execute_add_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"adds a widget to the currently viewed form.&nbsp;&nbsp;
not thread safe";
				e.Params["add-widget"]["where"]["dna"].Value = "root";
				e.Params["add-widget"]["where"]["position"].Value = "before|after|child";
				e.Params["add-widget"]["widget"]["type"].Value = "magix.forms.controls.installed-widget";
				e.Params["add-widget"]["widget"]["properties"]["id"].Value = "myWidget";
				e.Params["add-widget"]["widget"]["properties"]["text"].Value = "hello world";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

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

			string dna 		= ip["where"]["dna"].Get<string>();
			string position = ip["where"]["position"].Get<string>();

			Node whereNode = DataSource.FindDna(dna);

			Node widgetNode = ip["widget"].Clone();

			switch (position)
			{
			case "before":
				whereNode.AddBefore(widgetNode);
				break;
			case "after":
				whereNode.AddAfter(widgetNode);
				break;
			case "child":
				whereNode["controls"].Add(widgetNode);
				break;
			default:
				throw new ArgumentException("sorry, don't know where " + ip["position"].Get<string>() + " is");
			}

			BuildForm();
		}

		[ActiveEvent(Name="magix.execute.remove-widget")]
		protected void magix_execute_remove_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"removes the [dna] widget from the currently viewed form.&nbsp;&nbsp;
not thread safe";
				e.Params["remove-widget"]["dna"].Value = "root-0-0";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("dna"))
				throw new ArgumentException("you need a [dna] for remove-widget");

			string dna = ip["dna"].Get<string>();

			Node whereNode = DataSource.FindDna(dna);

			whereNode.Parent.Remove(whereNode);

			BuildForm();
		}

		[ActiveEvent(Name="magix.execute.change-widget")]
		protected void magix_execute_change_widget(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"changes the [dna] widget, adding/changing [change] properties and 
removing [remove] properties.&nbsp;&nbsp;not thread safe";
				e.Params["remove-widget"]["dna"].Value = "root-0-0";
				e.Params["remove-widget"]["change"]["id"].Value = "myId";
				e.Params["remove-widget"]["remove"]["css"].Value = null;
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("change") && !ip.Contains("remove"))
				throw new ArgumentException("you need a [change] and/or a [remove] for change-widget to signalize which properties are to be updated");

			Node widgetNode = DataSource.FindDna(ip["dna"].Get<string>());

			if (ip.Contains("change"))
			{
				foreach (Node idx in ip["change"])
				{
					widgetNode["properties"][idx.Name].Value = idx.Value;
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
		}

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

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

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
		}

		[ActiveEvent(Name="magix.execute.select-widget")]
		protected void magix_execute_select_widget(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.copy-widget")]
		protected void magix_execute_copy_widget(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.cut-widget")]
		protected void magix_execute_cut_widget(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.paste-widget")]
		protected void magix_execute_paste_widget(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.load-form")]
		protected void magix_execute_load_form(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.save-form")]
		protected void magix_execute_save_form(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.export-form")]
		protected void magix_execute_export_form(object sender, ActiveEventArgs e)
		{
		}
	}
}
