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
		[ActiveEvent(Name="magix.forms.list-control-types")]
		private static void magix_forms_list_control_types(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.list-control-types-dox].Value");
				ip["magix.forms.list-control-types"].Value = null;
				return;
			}

			Node ctrlActiveEvents = new Node();
			ctrlActiveEvents["begins-with"].Value = "magix.forms.controls.";
			RaiseActiveEvent(
                "magix.execute.list-events",
				ctrlActiveEvents);

			foreach (Node idx in ctrlActiveEvents["events"])
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
		private static void magix_forms_set_visible(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-visible-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-visible-sample]");
				return;
			}

            Control ctrl = FindControl<Control>(ip);
            ctrl.Visible = ip["value"].Get<bool>(false);
		}

		/*
		 * retrieves visibility of control
		 */
		[ActiveEvent(Name = "magix.forms.get-visible")]
		private static void magix_forms_get_visible(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-visible-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-visible-sample]");
				return;
			}

            Control ctrl = FindControl<Control>(ip);
            ip["value"].Value = ctrl.Visible;
		}

        /*
         * sets value
         */
        [ActiveEvent(Name = "magix.forms.set-value")]
        private static void magix_forms_set_value(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-value-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-value-sample]");
                return;
            }

            IValueControl ctrl = FindControl<IValueControl>(ip);
            ctrl.ControlValue = ip["value"].Get<string>("");
        }

        /*
         * returns value
         */
        [ActiveEvent(Name = "magix.forms.get-value")]
        private static void magix_forms_get_value(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-value-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-value-sample]");
                return;
            }

            IValueControl ctrl = FindControl<IValueControl>(ip);
            ip["value"].Value = ctrl.ControlValue;
        }

		/*
		 * fills out the stuff from basecontrol
		 */
		protected virtual void FillOutParameters(Node pars, BaseControl ctrl)
		{
            Node ip = Ip(pars);
            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("id"))
				ctrl.ID = node["id"].Get<string>();
			else if (node.Value != null)
				ctrl.ID = node.Get<string>();

            if (node.ContainsValue("visible"))
				ctrl.Visible = node["visible"].Get<bool>();

            if (node.ContainsValue("info"))
				ctrl.Info = node["info"].Get<string>();

			if (ShouldHandleEvent("onfirstload", node) && pars["_first"].Get<bool>(false))
			{
                Node codeNode = node["onfirstload"].Clone();
				ctrl.Load += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
		}

		protected static T FindControl<T>(Node pars)
		{
			if (!pars.Contains("id"))
				throw new ArgumentException("this active event needs an [id] parameter");

			Node ctrlNode = new Node();
			ctrlNode["id"].Value = pars["id"].Value;
			if (pars.Contains("form-id"))
				ctrlNode["form-id"].Value = pars["form-id"].Value;

			RaiseActiveEvent(
				"magix.forms._get-control",
				ctrlNode);

			if (ctrlNode.Contains("_ctrl"))
                return ctrlNode["_ctrl"].Get<T>();
            else
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
            BaseControl that = sender as BaseControl;
            if (!string.IsNullOrEmpty(that.Info))
                node["$"]["info"].Value = that.Info;

            if (that is IValueControl)
                node["$"]["value"].Value = (that as IValueControl).ControlValue;
        }

        protected virtual void Inspect(Node node)
        {
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            AppendInspectFromResource(
                tmp["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.base-dox].Value",
                true);
            node.Value = "control_id";
            node["visible"].Value = true;
            node["info"].Value = "any arbitrary additional info";
            node["onfirstload"].Value = "hyperlisp code";
        }
    }
}

