/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.Core;
using Magix.UX.Effects;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/*
	 * contains the basewebcontrol control
	 */
    public abstract class BaseWebControlController : BaseControlController
	{
		/*
		 * sets css classes
		 */
		[ActiveEvent(Name = "magix.forms.set-class")]
		private static void magix_forms_set_class(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-class-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-class-sample]");
				return;
			}

            BaseWebControl ctrl = FindControl<BaseWebControl>(ip);
			ctrl.Class = ip["value"].Get("");
		}

		/*
		 * retrieves css classes
		 */
		[ActiveEvent(Name = "magix.forms.get-class")]
		private static void magix_forms_get_class(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-class-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.get-class-sample]");
				return;
			}

            BaseWebControl ctrl = FindControl<BaseWebControl>(ip);
            ip["value"].Value = ctrl.Class;
		}

		/*
		 * sets focus to a specific mux web control
		 */
		[ActiveEvent(Name = "magix.forms.set-focus")]
		private static void magix_forms_set_focus(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-focus-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.set-focus-sample]");
				return;
			}

            FindControl<BaseWebControl>(ip).Focus();
		}

		/*
		 * fills out the stuff from basewebcontrol
		 */
		protected override void FillOutParameters(Node pars, BaseControl ctrl)
		{
			base.FillOutParameters(pars, ctrl);
            BaseWebControl that = ctrl as BaseWebControl;

            Node ip = Ip(pars);
            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("class"))
				that.Class = node["class"].Get<string>();

            if (node.ContainsValue("dir"))
				that.Dir = node["dir"].Get<string>();

            if (node.ContainsValue("tabindex"))
                that.TabIndex = node["tabindex"].Get<string>();

            if (node.ContainsValue("title"))
                that.Title = node["title"].Get<string>();

            if (node.ContainsValue("style"))
			{
				string[] styles = 
					node["style"].Get<string>().Replace("\n", "").Replace("\r", "").Split(
						new char[] {';'}, 
						StringSplitOptions.RemoveEmptyEntries);
				foreach (string idxStyle in styles)
				{
					that.Style[idxStyle.Split(':')[0]] = idxStyle.Split(':')[1];
				}
			}

			if (ShouldHandleEvent("onclick", node))
			{
				Node codeNode = node["onclick"].Clone();
				that.Click += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("ondblclick", node))
			{
				Node codeNode = node["ondblclick"].Clone();
				that.DblClick += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onmousedown", node))
			{
				Node codeNode = node["onmousedown"].Clone();
				that.MouseDown += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onmouseup", node))
			{
				Node codeNode = node["onmouseup"].Clone();
				that.MouseUp += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onmouseover", node))
			{
				Node codeNode = node["onmouseover"].Clone();
				that.MouseOver += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onmouseout", node))
			{
				Node codeNode = node["onmouseout"].Clone();
				that.MouseOut += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onkeypress", node))
			{
				Node codeNode = node["onkeypress"].Clone();
				that.KeyPress += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (ShouldHandleEvent("onesc", node))
			{
				Node codeNode = node["onesc"].Clone();
				that.Esc += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
		}

		protected override void Inspect(Node node)
		{
            base.Inspect(node);
            Node tmp = node;
            while (!tmp.Contains("inspect"))
                tmp = tmp.Parent;
            AppendInspectFromResource(
                tmp["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.base-web-dox].Value",
                true);
			node["class"].Value = "css classes";
			node["dir"].Value = "ltr";
            node["tabindex"].Value = 5;
            node["title"].Value = "informational tooltip";
			node["style"].Value = "width:120px;height:120px;";
			node["onclick"].Value = "hyperlisp code";
			node["ondblclick"].Value = "hyperlisp code";
			node["onmousedown"].Value = "hyperlisp code";
			node["onmouseup"].Value = "hyperlisp code";
			node["onmouseover"].Value = "hyperlisp code";
			node["onmouseout"].Value = "hyperlisp code";
			node["onkeypress"].Value = "hyperlisp code";
			node["onesc"].Value = "hyperlisp code";
		}
	}
}

