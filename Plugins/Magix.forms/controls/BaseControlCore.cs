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
                "magix.execute.list-events",
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

                if (tp2["magix.forms.create-web-part"].Contains("_no-embed"))
					tp["_no-embed"].Value = true;

				foreach (Node idx2 in tp2["magix.forms.create-web-part"]["controls"][0])
				{
					tp["properties"][idx2.Name].Value = idx2.Value;
					tp["properties"][idx2.Name].AddRange(idx2);
				}

                tp["properties"].Sort(
                    delegate(Node left, Node right)
                    {
                        if (left.Name == "id")
                            return -1;
                        if (right.Name == "id")
                            return 1;
                        if (left.Name.StartsWith("on"))
                            return 1;
                        if (right.Name.StartsWith("on"))
                            return -1;
                        return left.Name.CompareTo(right.Name);
                    });

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

			if (node.Contains("onfirstload") && node.Contains("_first") && node["_first"].Get<bool>() && node["onfirstload"].Count > 0)
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
            tmp["inspect"].Value = tmp["inspect"].Get<string>() + @"<p>all active event handlers, will have 
the [info] value, and whatever value the control 
has, automatically added to the [$] collection when any active events are raised.&nbsp;&nbsp;to retreieve 
these values, use either [$][info] or [$][value] in your expressions, from within your active event 
handlers.&nbsp;&nbsp;active event handlers are recognized by the fact of that they start with the 
text 'on' as their property name</p>
<p><strong>properties inherited from control</strong></p>
<p>'control_id', or the value 
of the node, must be a unique id within your 
form, and will become the control's unique identification within your form.&nbsp;&nbsp;this is the 
id you use to change or retrieve a control's state after it has been created, in for example the 
[magix.forms.get-value] active event</p><p>
[visible] can be true or false, and determines if the control is visible or not.&nbsp;&nbsp;
you can change or retrieve a controls's visibility with the [magix.forms.set-visible] or [magix.forms.get-visible] active events</p>
<p>[info] is any additional textually represented 
piece of information you'd like to attach with your control.&nbsp;&nbsp;[info] is useful for attaching
meta data with your control, which shouldn't be visible, but still coupled with your 
control somehow.&nbsp;&nbsp;[info] will be passed in automatically into your control's 
event handlers for you, together with your control's value through the [$] collection</p>
<p>[onfirstload] can be used as the means to run initialization code
during the initial creation of your control in your form.&nbsp;&nbsp;[onfirstload] will only 
be raised the first time the control is created.&nbsp;&nbsp;this active event is useful for initializing 
properties for your control, or run code which is only supposed to run once, during the initial initialization 
of your control</p>";
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

