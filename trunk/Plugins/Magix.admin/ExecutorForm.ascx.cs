/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Drawing;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
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
				Manager.Instance.JavaScriptWriter.Write(activeEvent.ClientID + ".focus();");
				Manager.Instance.JavaScriptWriter.Write(activeEvent.ClientID + ".select();");

				string evt = Page.Request["evt"];
				string code = Page.Request["code"];

				if (string.IsNullOrEmpty(evt))
					activeEvent.Value = "magix.execute";
				else
					activeEvent.Value = evt;

				if (string.IsNullOrEmpty(code))
					txtIn.Value = @"magix.viewport.show-message
  message=>howdy world!";
				else
					txtIn.Value = code;

				// Including JavaScript files ...
				Node tmp = new Node();

				tmp["type"].Value = "javascript";
				tmp["file"].Value = "media/bootstrap/js/jQuery.js";

				RaiseActiveEvent(
					"magix.viewport.include-client-file",
					tmp);

				tmp = new Node();
				tmp["type"].Value = "javascript";
				tmp["file"].Value = "media/bootstrap/js/bootstrap.min.js";

				RaiseActiveEvent(
					"magix.viewport.include-client-file",
					tmp);

				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-provide", "typeahead"));
				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-items", "12"));
				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-source", GetDataSource()));
			}
		}

		protected string GetDataSource()
		{
			Node node = new Node();

			RaiseActiveEvent(
                "magix.execute.list-events", 
				node);

			string data = "";

			foreach (Node idx in node["events"])
			{
				data += "\"" + idx.Name + "\",";
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
				e.Params["inspect"].Value = @"sets the active event to what is given in [event].
&nbsp;&nbsp;not thread safe";
				e.Params["event"].Value = "magix.execute";
				return;
			}

            if (!Ip(e.Params).Contains("event"))
				throw new ArgumentException("you must pass in an [event] to set-code-event");

            activeEvent.Value = Ip(e.Params)["event"].Get<string>();
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
            tmp["code"].Value = Ip(e.Params)["code"].Get<string>();

			RaiseActiveEvent (
				"magix.execute.code-2-node",
				tmp);

			Node json = tmp["node"].Clone();

            bool foundEvent = false;
			foreach (Node idx in json)
			{
				if (idx.Name.StartsWith("event:"))
				{
					activeEvent.Value = idx.Name.Substring(6);
                    idx.UnTie();
                    foundEvent = true;
					break;
				}
			}

            if (!foundEvent)
                activeEvent.Value = "magix.execute";

			tmp = new Node();
			tmp["node"].Value = json;

			RaiseActiveEvent (
				"magix.execute.node-2-code",
				tmp);

			txtIn.Value = tmp["code"].Get<string>();
		}

		protected void move_Click(object sender, EventArgs e)
		{
            // Removing "inspect" node
            Node tmp = new Node();
            tmp["code"].Value = txtOut.Value;
            RaiseActiveEvent(
                "magix.execute.code-2-node",
                tmp);
            Node node = tmp["node"].Clone();
            node["inspect"].UnTie();
            foreach (Node idx in node)
            {
                if (idx.Name.StartsWith("event:"))
                {
                    activeEvent.Value = idx.Name.Replace("event:", "");
                    idx.UnTie();
                    break;
                }
            }
            RaiseActiveEvent(
                "magix.execute.node-2-code",
                tmp);
			txtIn.Value = tmp["code"].Get<string>();
		}

		protected void run_Click(object sender, EventArgs e)
		{
			if (txtIn.Value != "")
			{
				Node tmp = new Node();
				tmp["code"].Value = txtIn.Value;

				RaiseActiveEvent(
					"magix.execute.code-2-node",
					tmp);

                Node node = tmp["node"].Clone();
                node.Name = activeEvent.Value;

				foreach (Node idx in node)
				{
					if (idx.Name.StartsWith("event:"))
					{
						activeEvent.Value = idx.Name.Substring(6);
						node.Remove(idx);
						if (node.Contains("inspect"))
							node.Remove(node["inspect"]);
						break;
					}
				}

				RaiseActiveEvent(
					activeEvent.Value, 
					node);

                tmp = new Node();
                tmp["node"].AddRange(node);

				RaiseActiveEvent(
					"magix.execute.node-2-code", 
					tmp);

				txtOut.Value = tmp["code"].Get<string>();
			}
			else
			{
				Node node = RaiseActiveEvent(activeEvent.Value);

				Node tmp = new Node();
				tmp["node"].Value = node;

				RaiseActiveEvent(
					"magix.execute.node-2-code", 
					tmp);

				txtOut.Value = tmp["code"].Get<string>();
			}

            // showing a visual clue to end user that code is done executing
            new EffectHighlight(txtOut, 500)
                .Render();
		}
		
		protected void indent_Click(object sender, EventArgs e)
		{
			string transformed = "";
			using (StringReader reader = new StringReader(txtIn.Value))
			{
				string line = reader.ReadLine();
				while (line != null)
				{
					transformed += "  " + line + "\r\n";
					line = reader.ReadLine();
				}
			}
			txtIn.Value = transformed;
		}
		
		protected void deindent_Click(object sender, EventArgs e)
		{
			string transformed = "";
			using (StringReader reader = new StringReader(txtIn.Value))
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
			txtIn.Value = transformed;
		}
	}
}

