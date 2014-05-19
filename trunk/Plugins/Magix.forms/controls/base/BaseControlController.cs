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
	/*
	 * contains the basecontrol control
	 */
	public abstract class BaseControlController : ActiveController
	{
		/*
		 * lists all widget types that exists in system
		 */
		[ActiveEvent(Name="magix.forms.list-widget-types")]
		protected static void magix_forms_list_widget_types(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				ip["inspect"].Value = @"<p>lists all widget types as [types] 
available in system</p><p>thread safe</p>";
				ip["magix.forms.list-widget-types"].Value = null;
				return;
			}

			Node controlActiveEvents = new Node();
			controlActiveEvents["begins-with"].Value = "magix.forms.controls.";
			RaiseActiveEvent(
                "magix.execute.list-events",
				controlActiveEvents);

			foreach (Node idx in controlActiveEvents["events"])
			{
				Node controlNode = new Node("widget");
				controlNode["type"].Value = idx.Name;
				controlNode["properties"]["id"].Value = "id";

				Node inspectControlNode = new Node();
				inspectControlNode["inspect"].Value = null;
				RaiseActiveEvent(
					idx.Name,
					inspectControlNode);

                if (inspectControlNode["magix.forms.create-web-part"].Contains("_no-embed"))
					controlNode["_no-embed"].Value = true;

				foreach (Node idxPropertyNode in inspectControlNode["magix.forms.create-web-part"]["controls"][0])
				{
					controlNode["properties"][idxPropertyNode.Name].Value = idxPropertyNode.Value;
					controlNode["properties"][idxPropertyNode.Name].AddRange(idxPropertyNode);
				}

                controlNode["properties"].Sort(
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

				ip["types"].Add(controlNode);
			}
		}

		/*
		 * sets visibility of control
		 */
		[ActiveEvent(Name = "magix.forms.set-visible")]
		protected static void magix_forms_set_visible(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                e.Params["inspect"].Value = @"<p>sets the visibility of the given 
[id] web control, in the [form-id] form</p><p>if the [value] node's value is true, 
then the control is shown, otherwise it will be hidden.&nbsp;&nbsp;the [id] is the 
id of the control you wish to retrieve the value from, and the [form-id] is the 
form id you created the form with.&nbsp;&nbsp;[form-id] is optional, and if not 
given, the logic will change the value of the first control that matches the [id] 
in any form</p><p>not thread safe</p>";
                e.Params["magix.forms.set-visible"]["id"].Value = "control";
                e.Params["magix.forms.set-visible"]["form-id"].Value = "webpages";
                e.Params["magix.forms.set-visible"]["value"].Value = true;
				return;
			}

            if (!ip.Contains("value"))
                throw new ArgumentException("you must supply a [value] to [magix.forms.set-visible]");

            Control ctrl = FindControl<Control>(ip);
            ctrl.Visible = ip["value"].Get<bool>();
		}

		/*
		 * retrieves visibility of control
		 */
		[ActiveEvent(Name = "magix.forms.get-visible")]
		protected static void magix_forms_get_visible(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                ip["inspect"].Value = @"<p>returns the visibility of the given 
[id] web control, in the [form-id] form</p><p>if the control is visible, the 
active event will return true in [value], otherwise false.&nbsp;&nbsp;the [id] 
is the id of the control you wish to retrieve the value from, and the [form-id] 
is the form id you created the form with.&nbsp;&nbsp;[form-id] is optional, and 
if not given, the logic will change the value of the first control that matches 
the [id] in any form</p><p>not thread safe</p>";
                ip["magix.forms.get-visible"]["id"].Value = "control";
                ip["magix.forms.get-visible"]["form-id"].Value = "webpages";
				return;
			}

            Control ctrl = FindControl<Control>(ip);
            ip["value"].Value = ctrl.Visible;
		}

        /*
         * sets value
         */
        [ActiveEvent(Name = "magix.forms.set-value")]
        public static void magix_forms_set_value(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspectOrHasInspected(ip))
            {
                ip["inspect"].Value = @"<p>sets the value property of the control</p>
<p>changes the current value of the control to whatever you give it as input in [value].
&nbsp;&nbsp;the [id] is the id of the control you wish to retrieve the value from, and 
the [form-id] is the form id you created the form with.&nbsp;&nbsp;[form-id] is optional,
and if not given, the logic will change the value of the first control that matches the 
[id] in any form</p><p>not thread safe</p>";
                ip["magix.forms.set-value"]["id"].Value = "control";
                ip["magix.forms.set-value"]["form-id"].Value = "webpages";
                ip["magix.forms.set-value"]["value"].Value = "some new value";
                return;
            }

            if (!ip.Contains("value"))
                throw new ArgumentException("[magix.forms.set-value] needs [value]");

            Control ctrl = FindControl<Control>(ip);
            IValueControl iCtrl = ctrl as IValueControl;
            if (iCtrl == null)
                throw new ArgumentException("that control doesn't support having its value changed");
            iCtrl.ControlValue = ip["value"].Get<string>();
        }

        /*
         * returns value
         */
        [ActiveEvent(Name = "magix.forms.get-value")]
        public void magix_forms_get_value(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspectOrHasInspected(ip))
            {
                ip["inspect"].Value = @"<p>returns the value of the control</p>
<p>the value will be returned as [value].&nbsp;&nbsp;the [id] is the id of the 
control you wish to retrieve the value from, and the [form-id] is the form id 
you created the form with.&nbsp;&nbsp;[form-id] is optional, and if not given, 
the logic will change the value of the first control that matches the [id] in 
any form</p><p>not thread safe</p>";
                ip["magix.forms.get-value"]["id"].Value = "control";
                ip["magix.forms.get-value"]["form-id"].Value = "webpages";
                return;
            }

            Control ctrl = FindControl<Control>(ip);
            IValueControl iCtrl = ctrl as IValueControl;
            if (iCtrl == null)
                throw new ArgumentException("that control doesn't support retrieving its value");
            ip["value"].Value = iCtrl.ControlValue;
        }

		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected virtual object GetValue(BaseControl that)
		{
			return null;
		}

		/*
		 * fills out the stuff from basecontrol
		 */
		protected virtual void FillOutParameters(Node pars, BaseControl ctrl)
		{
            Node ip = Ip(pars);
            Node controlDeclarationNode = ip["_code"].Get<Node>();

            if (controlDeclarationNode.Contains("id") && !string.IsNullOrEmpty(controlDeclarationNode["id"].Get<string>()))
				ctrl.ID = controlDeclarationNode["id"].Get<string>();
			else if (controlDeclarationNode.Value != null)
				ctrl.ID = controlDeclarationNode.Get<string>();

			if (controlDeclarationNode.Contains("visible") && controlDeclarationNode["visible"].Value != null)
				ctrl.Visible = controlDeclarationNode["visible"].Get<bool>();

			if (controlDeclarationNode.Contains("info") && !string.IsNullOrEmpty(controlDeclarationNode["info"].Get<string>()))
				ctrl.Info = controlDeclarationNode["info"].Get<string>();

			if (ShouldHandleEvent("onfirstload", controlDeclarationNode) && 
                pars.Contains("_first") && 
                pars["_first"].Get<bool>())
			{
                Node codeNode = controlDeclarationNode["onfirstload"].Clone();
				ctrl.Load += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
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
            throw new ArgumentException("couldn't find that control");
		}

        protected bool ShouldHandleEvent(string evt, Node node)
        {
            if (node.Contains(evt) && node[evt].Count > 0)
                return true;
            return false;
        }

        protected void FillOutEventInputParameters(Node node, object sender)
        {
            BaseControl that2 = sender as BaseControl;
            if (!string.IsNullOrEmpty(that2.Info))
                node["$"]["info"].Value = that2.Info;

            object val = GetValue(that2);
            if (val != null)
                node["$"]["value"].Value = val;
        }
	}
}

