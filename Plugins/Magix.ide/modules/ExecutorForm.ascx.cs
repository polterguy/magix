/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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

namespace Magix.ide.modules
{
    /*
     * hyperlisp executor
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

                tmp = new Node();
                tmp["type"].Value = "css";
                tmp["file"].Value = "media/back-end/event-executor.css";

                RaiseActiveEvent(
                    "magix.viewport.include-client-file",
                    tmp);

                tmp = new Node();
                tmp["type"].Value = "css";
                tmp["file"].Value = "media/back-end/typeahead.css";

                RaiseActiveEvent(
                    "magix.viewport.include-client-file",
                    tmp);

                activeEvent.Attributes.Add(new AttributeControl.Attribute("data-provide", "typeahead"));
				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-items", "12"));
				activeEvent.Attributes.Add(new AttributeControl.Attribute("data-source", GetDataSource()));
			}
		}

        /*
         * datasource for active event autocompleter
         */
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

        /*
         * sets code for executor
         */
        [ActiveEvent(Name = "magix.executor.set-code")]
        public void magix_executor_set_code(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.executor.set-code-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.executor.set-code-sample]");
                return;
			}
            if (!ip.ContainsValue("code"))
                throw new ArgumentException("no [code] given to [magix.executor.set-code]");
            Node dp = Dp(e.Params);
            txtIn.Value = Expressions.GetExpressionValue(ip["code"].Get<string>(), dp, ip, false) as string;
		}

		protected void move_Click(object sender, EventArgs e)
		{
            // Removing "inspect" node
            Node toNode = new Node();
            toNode["code"].Value = txtOut.Value;
            RaiseActiveEvent(
                "magix.execute.code-2-node",
                toNode);

            toNode["node"]["inspect"].UnTie();

            Node toCode = new Node();
            toCode["node"].Value = toNode["node"];
            RaiseActiveEvent(
                "magix.execute.node-2-code",
                toCode);

			txtIn.Value = toCode["code"].Get<string>();
		}

		protected void run_Click(object sender, EventArgs e)
		{
            // showing a visual clue to end user that code is done executing
            new EffectHighlight(this.Parent, 500)
                .Render();
            
            if (txtIn.Value != "")
			{
				Node toNode = new Node();
				toNode["code"].Value = txtIn.Value;
				RaiseActiveEvent(
					"magix.execute.code-2-node",
					toNode);

                Node codeNode = toNode["node"].Clone();
                codeNode.Name = activeEvent.Value;
				RaiseActiveEvent(
					activeEvent.Value, 
					codeNode);

                Node toCode = new Node();
                toCode["node"].AddRange(codeNode);
				RaiseActiveEvent(
					"magix.execute.node-2-code",
                    toCode);

                txtOut.Value = toCode["code"].Get<string>();
			}
			else
			{
				Node node = RaiseActiveEvent(activeEvent.Value);

				Node toCode = new Node();
				toCode["node"].Value = node;
				RaiseActiveEvent(
					"magix.execute.node-2-code", 
					toCode);

				txtOut.Value = toCode["code"].Get<string>();
			}
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
