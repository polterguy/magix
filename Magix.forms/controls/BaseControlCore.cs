/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * contains the basecontrol control
	 */
	public abstract class BaseControlCore : ActiveController
	{
		/**
		 * lists widget types that exists in system
		 */
		[ActiveEvent(Name="magix.execute.list-widget-types")]
		protected static void magix_execute_list_widget_types(object sender, ActiveEventArgs e)
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

			RaiseActiveEvent(
				"magix.admin.get-active-events",
				tmp);

			Node ip = e.Params["_ip"].Get<Node>();

			foreach (Node idx in tmp["events"])
			{
				Node tp = new Node("widget");

				tp["type"].Value = idx.Get<string>();
				tp["properties"]["id"].Value = "id";

				Node tp2 = new Node();
				tp2["inspect"].Value = null;

				RaiseActiveEvent(
					idx.Get<string>(),
					tp2);

				if (tp2.Contains("_no-embed"))
					tp["_no-embed"].Value = true;

				foreach (Node idx2 in tp2["controls"][0])
				{
					tp["properties"][idx2.Name].Value = idx2.Value;
					tp["properties"][idx2.Name].AddRange(idx2);
				}

				ip["types"].Add(tp);
			}
		}

		/**
		 * fills out the stuff from basecontrol
		 */
		protected virtual void FillOutParameters(Node node, BaseControl ctrl)
		{
			if (node.Contains("id") && !string.IsNullOrEmpty(node["id"].Get<string>()))
				ctrl.ID = node["id"].Get<string>();
			else if (node.Value != null)
				ctrl.ID = node.Get<string>();

			if (node.Contains("visible") && node["visible"].Value != null)
				ctrl.Visible = node["visible"].Get<bool>();

			if (node.Contains("info") && !string.IsNullOrEmpty(node["info"].Get<string>()))
				ctrl.Info = node["info"].Get<string>();

			if (node.Contains("onfirstload") && node.Contains("_first") && node["_first"].Get<bool>())
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onfirstload"].Clone();

				ctrl.Load += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
		}

		protected virtual void Inspect(Node node)
		{
			Node tmp = node;
			while (!tmp.Contains("inspect"))
				tmp = tmp.Parent;
			tmp["inspect"].Value = tmp["inspect"].Get<string>() + @".&nbsp;&nbsp;
[visible] can be true or false, [info] is any additional textually represented 
information you like to attach with control.&nbsp;&nbsp;useful for adding in 
id strings, and such.&nbsp;&nbsp;control_id must be a unique id within your 
form.&nbsp;&nbsp;use [onfirstload] as the means to run initialization code
during first initial creation in form.&nbsp;&nbsp;[onfirstload] will only 
be raised the first time the control is created";
			node.Value = "control_id";
			node["visible"].Value = true;
			node["info"].Value = "any arbitrary additional info";
			node["onfirstload"].Value = "hyper lisp code";
		}
	}
}

