/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
		protected System.Web.UI.HtmlControls.HtmlInputText activeEvent;

		protected void Page_Load (object sender, EventArgs e)
		{
			if (this.FirstLoad)
			{
				AjaxManager.Instance.WriterAtBack.Write(activeEvent.ClientID + ".focus();");
				AjaxManager.Instance.WriterAtBack.Write(activeEvent.ClientID + ".select();");

				string evt = Page.Request["evt"];
				string code = Page.Request["code"];

				if (string.IsNullOrEmpty(evt))
					activeEvent.Value = "magix.viewport.show-message";
				else
					activeEvent.Value = evt;

				if (string.IsNullOrEmpty (code))
					txtIn.Text = "inspect";
				else
					txtIn.Text = code;

				// Including JavaScript files ...
				Node tmp = new Node();
				tmp["type"].Value = "JavaScript";
				tmp["file"].Value = "media/bootstrap/js/jQuery.js";

				RaiseEvent(
					"magix.viewport.include-client-file",
					tmp);

				tmp = new Node();
				tmp["type"].Value = "JavaScript";
				tmp["file"].Value = "media/bootstrap/js/bootstrap.min.js";

				RaiseEvent(
					"magix.viewport.include-client-file",
					tmp);

				activeEvent.DataBind();
			}
		}

		protected string GetDataSource()
		{
			Node node = new Node();

			RaiseEvent(
				"magix.admin.get-active-events", 
				node);

			string data = "";

			foreach (Node idx in node["ActiveEvents"])
			{
				data += "\"" + idx.Get<string>() + "\",";
			}

			return "[" + data.TrimEnd (',') + "]";
		}

		/**
		 * Called by Magix.Core when an event is overridden. Handled here to make
		 * sure we re-retrieve the active events in the system, and rebinds our
		 * list of active events
		 */
		[ActiveEvent(Name = "magix.execute._event-overridden")]
		public void magix_execute__event_overridden(object sender, ActiveEventArgs e)
		{
			// TODO: Make a Panel wrapper around input field, such that ReRender
			// can be called, to re-databind the events ...
			Node node = new Node();

			RaiseEvent(
				"magix.admin.get-active-events", 
				node);
		}

		/**
		 * Called by Magix.Core when an event override is removed. Handled here to make
		 * sure we re-retrieve the active events in the system, and rebinds our
		 * list of active events
		 */
		[ActiveEvent(Name = "magix.execute._event-override-removed")]
		public void magix_execute__event_override_removed(object sender, ActiveEventArgs e)
		{
			// TODO: Make a Panel wrapper around input field, such that ReRender
			// can be called, to re-databind the events ...
			Node node = new Node();

			RaiseEvent(
				"magix.admin.get-active-events", 
				node);
		}

		protected void run_Click (object sender, EventArgs e)
		{
			//AjaxManager.Instance.WriterAtBack.Write(activeEvent.ClientID + ".focus();");
			//AjaxManager.Instance.WriterAtBack.Write(activeEvent.ClientID + ".select();");
			if (txtIn.Text != "")
			{
				string wholeTxt = txtIn.Text;
				if (wholeTxt.StartsWith("Method:") || wholeTxt.StartsWith("event:"))
				{
					string method = wholeTxt.Split (':')[1];
					method = method.Substring (0, method.IndexOf ("\n"));
					activeEvent.Value = method;
					wholeTxt = wholeTxt.Substring (wholeTxt.IndexOf ("\n")).TrimStart ();
				}

				Node tmp = new Node();
				tmp["code"].Value = wholeTxt;

				RaiseEvent(
					"magix.code.code-2-node",
					tmp);

				RaiseEvent(
					activeEvent.Value, 
					tmp["JSON"].Get<Node>());

				RaiseEvent(
					"magix.code.node-2-code", 
					tmp);

				txtOut.Text = tmp["code"].Get<string>();
			}
			else
			{
				Node node = RaiseEvent(activeEvent.Value);

				Node tmp = new Node();
				tmp["JSON"].Value = node;

				RaiseEvent(
					"magix.code.node-2-code", 
					tmp);

				txtOut.Text = tmp["code"].Get<string>();
			}
		}
	}
}
