/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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

namespace Magix.admin
{
    /**
     * Active Module for Active Event Executor. Allows you to execute and traverse all
     * the Active Events you have registered in your system, and also contains a 'shell'
     * for creating parameters to said Active Events, such that you can configure your
     * system, through something almost resembling a Linux Shell. You can configure
     * it to set the system into a specific state, by supplying an "evt" and "code"
     * HTTP parameters
     */
    public class ExecutorForm : ActiveModule
    {
		protected TextArea txtIn;
		protected TextArea txtOut;
		protected Button run;
		protected TextBox activeEvent;

		protected void Page_Load (object sender, EventArgs e)
		{
			if (this.FirstLoad)
			{
				AjaxManager.Instance.WriterAtBack.Write(activeEvent.ClientID + ".focus();");
				AjaxManager.Instance.WriterAtBack.Write(activeEvent.ClientID + ".select();");

				string evt = Page.Request["evt"];
				string code = Page.Request["code"];

				if (string.IsNullOrEmpty(evt))
					activeEvent.Text = "magix.viewport.show-message";
				else
					activeEvent.Text = evt;

				if (string.IsNullOrEmpty (code))
					txtIn.Text = "inspect";
				else
					txtIn.Text = code;

				// Including JavaScript files ...
				Node tmp = new Node();

				tmp["type"].Value = "javascript";
				tmp["file"].Value = "media/bootstrap/js/jQuery.js";

				RaiseEvent(
					"magix.viewport.include-client-file",
					tmp);

				tmp = new Node();
				tmp["type"].Value = "javascript";
				tmp["file"].Value = "media/bootstrap/js/bootstrap.min.js";

				RaiseEvent(
					"magix.viewport.include-client-file",
					tmp);

				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-provide", "typeahead"));
				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-items", "42"));
				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-source", GetDataSource()));
			}
		}

		protected string GetDataSource()
		{
			Node node = new Node();

			RaiseEvent(
				"magix.admin.get-active-events", 
				node);

			string data = "";

			foreach (Node idx in node["events"])
			{
				data += "\"" + idx.Get<string>() + "\",";
			}

			return "[" + data.TrimEnd (',') + "]";
		}

		[ActiveEvent(Name = "magix.admin.set-code-event")]
		public void magix_admin_set_code_event(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.admin.set-code-event"].Value = null;
				e.Params["inspect"].Value = @"sets the active evtn to what is given in [event].
&nbsp;&nbsp;not thread safe";
				e.Params["event"].Value = "magix.execute";
				return;
			}

			if (!e.Params.Contains("event"))
				throw new ArgumentException("you must pass in an [event] to set-code-event");

			activeEvent.Text = e.Params["event"].Get<string>();
		}

		[ActiveEvent(Name = "magix.admin.set-code")]
		public void magix_admin_set_code(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.admin.set-code"].Value = null;
				e.Params["inspect"].Value = @"sets the code to what is given in [code]
in the active event executor.&nbsp;&nbsp;not thread safe";
				e.Params["code"].Value =  @"
event:magix.execute
Data=>thomas
if=>[Data].Value==thomas
  magix.viewport.show-message
    message=>hello world";
				return;
			}

			Node tmp = new Node();
			tmp["code"].Value = e.Params["code"].Get<string>();

			RaiseEvent (
				"magix.code.code-2-node",
				tmp);

			Node json = tmp["json"].Get<Node>();

			foreach (Node idx in json)
			{
				if (idx.Name.StartsWith("event:"))
				{
					activeEvent.Text = idx.Name.Substring(6);
					break;
				}
			}

			tmp = new Node();
			tmp["json"].Value = json;

			RaiseEvent (
				"magix.code.node-2-code",
				tmp);

			txtIn.Text = tmp["code"].Get<string>();
		}

		protected void move_Click(object sender, EventArgs e)
		{
			txtIn.Text = txtOut.Text;
		}

		protected void run_Click(object sender, EventArgs e)
		{
			if (txtIn.Text != "")
			{
				Node tmp = new Node();
				tmp["code"].Value = txtIn.Text;

				RaiseEvent(
					"magix.code.code-2-node",
					tmp);

				foreach (Node idx in tmp["json"].Get<Node>())
				{
					if (idx.Name.StartsWith("event:"))
					{
						activeEvent.Text = idx.Name.Substring(6);
						tmp["json"].Get<Node>().Remove(idx);
						if (tmp["json"].Get<Node>().Contains("inspect"))
							tmp["json"].Get<Node>().Remove(tmp["json"].Get<Node>()["inspect"]);
						break;
					}
				}

				RaiseEvent(
					activeEvent.Text, 
					tmp["json"].Get<Node>());

				RaiseEvent(
					"magix.code.node-2-code", 
					tmp);

				txtOut.Text = tmp["code"].Get<string>();
			}
			else
			{
				Node node = RaiseEvent(activeEvent.Text);

				Node tmp = new Node();
				tmp["json"].Value = node;

				RaiseEvent(
					"magix.code.node-2-code", 
					tmp);

				txtOut.Text = tmp["code"].Get<string>();
			}
		}
		
		protected void indent_Click(object sender, EventArgs e)
		{
			string transformed = "";
			using (StringReader reader = new StringReader(txtIn.Text))
			{
				string line = reader.ReadLine();
				while (line != null)
				{
					transformed += "  " + line + "\r\n";
					line = reader.ReadLine();
				}
			}
			txtIn.Text = transformed;
		}
		
		protected void deindent_Click(object sender, EventArgs e)
		{
			string transformed = "";
			using (StringReader reader = new StringReader(txtIn.Text))
			{
				string line = reader.ReadLine();
				while (line != null)
				{
					if (line.IndexOf("  ") == 0)
						line = line.Substring(2);
					transformed += line + "\r\n";
					line = reader.ReadLine();
				}
			}
			txtIn.Text = transformed;
		}
	}
}





























