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

			switch (type)
			{
			case "html-panel":
				break;
			case "html-label":
				break;
			default:
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

				break;
			}
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
				e.Params["add-widget"]["widget"]["type"].Value = "html-panel|html-label|magix.forms.controls.installed-widget";
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
			}

			BuildForm();
		}

		[ActiveEvent(Name="magix.execute.remove-widget")]
		protected void magix_execute_remove_widget(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.change-widget")]
		protected void magix_execute_change_widget(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.move-widget")]
		protected void magix_execute_move_widget(object sender, ActiveEventArgs e)
		{
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

		[ActiveEvent(Name="magix.execute.list-widgets")]
		protected void magix_execute_list_widgets(object sender, ActiveEventArgs e)
		{
		}

		[ActiveEvent(Name="magix.execute.list-widget-types")]
		protected void magix_execute_list_widget_types(object sender, ActiveEventArgs e)
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
