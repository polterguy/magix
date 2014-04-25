/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
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
		 * lists all widget types that exists in system
		 */
		[ActiveEvent(Name="magix.forms.list-widget-types")]
		protected static void magix_forms_list_widget_types(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"lists all widget types as [types] available in system.&nbsp;&nbsp;
thread safe";
				e.Params["magix.forms.list-widget-types"].Value = null;
				return;
			}

			Node tmp = new Node();
			tmp["begins-with"].Value = "magix.forms.controls.";

			RaiseActiveEvent(
				"magix.admin.get-active-events",
				tmp);

            Node ip = Ip(e.Params);

			foreach (Node idx in tmp["events"])
			{
				Node tp = new Node("widget");

				tp["type"].Value = idx.Name;
				tp["properties"]["id"].Value = "id";

				Node tp2 = new Node();
				tp2["inspect"].Value = null;

				RaiseActiveEvent(
					idx.Name,
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
		 * sets visibility of control
		 */
		[ActiveEvent(Name = "magix.forms.set-visible")]
		protected static void magix_forms_set_visible(object sender, ActiveEventArgs e)
		{
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.set-visible"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"sets the visibility of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

            Control ctrl = FindControl<Control>(Ip(e.Params));

			if (ctrl != null)
			{
				bool visible = false;
                if (Ip(e.Params).Contains("value"))
                    visible = Ip(e.Params)["value"].Get<bool>();

				ctrl.Visible = visible;
			}
		}

		/**
		 * retrieves visibility of control
		 */
		[ActiveEvent(Name = "magix.forms.get-visible")]
		protected static void magix_forms_get_visible(object sender, ActiveEventArgs e)
		{
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.get-visible"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"retrieves the visibility of the given 
[id] web control, in the [form-id] form, into [value].&nbsp;&nbsp;not thread safe";
				return;
			}

            Control ctrl = FindControl<Control>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Visible;
			}
		}

		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected virtual object GetValue(BaseControl that)
		{
			return null;
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
				Node codeNode = node["onfirstload"].Clone();

				ctrl.Load += delegate(object sender, EventArgs e)
				{
					BaseControl that2 = sender as BaseControl;
					if (!string.IsNullOrEmpty(that2.Info))
						codeNode["$"]["info"].Value = that2.Info;

					object val = GetValue(that2);
					if (val != null)
						codeNode["$"]["value"].Value = val;

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

		protected static T FindControl<T>(Node pars) where T : Control
		{
			if (!pars.Contains("id"))
				throw new ArgumentException("set-value(s)/get-value(s) needs [id] parameter");

			Node ctrlNode = new Node();

			ctrlNode["id"].Value = pars["id"].Value;

			if (pars.Contains("form-id"))
				ctrlNode["form-id"].Value = pars["form-id"].Value;

			RaiseActiveEvent(
				"magix.forms._get-control",
				ctrlNode);

			if (ctrlNode.Contains("_ctrl"))
			{
				Control ctrl = ctrlNode["_ctrl"].Value as Control;
				return ctrl as T;
			}

			return default(T);
		}
	}
}
