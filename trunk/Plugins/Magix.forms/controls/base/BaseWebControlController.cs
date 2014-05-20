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
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.set-class"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = "some-css-class";
				e.Params["inspect"].Value = @"sets the css class of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

            BaseWebControl ctrl = FindControl<BaseWebControl>(Ip(e.Params));

			if (ctrl != null)
			{
				string className = "";
                if (Ip(e.Params).Contains("value"))
                    className = Ip(e.Params)["value"].Get<string>();

				ctrl.Class = className;
			}
		}

		/*
		 * retrieves css classes
		 */
		[ActiveEvent(Name = "magix.forms.get-class")]
		private static void magix_forms_get_class(object sender, ActiveEventArgs e)
		{
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.get-class"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"retrieves the css class of the given 
[id] web control, in the [form-id] form, into [value].&nbsp;&nbsp;not thread safe";
				return;
			}

            BaseWebControl ctrl = FindControl<BaseWebControl>(Ip(e.Params));

			if (ctrl != null)
			{
                Ip(e.Params)["value"].Value = ctrl.Class;
			}
		}

		/*
		 * sets focus to a specific mux web control
		 */
		[ActiveEvent(Name = "magix.forms.set-focus")]
		private static void magix_forms_set_focus(object sender, ActiveEventArgs e)
		{
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.set-focus"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["inspect"].Value = @"sets focus to the specific 
[id] web control, in the [form-id] form.&nbsp;&nbsp;not thread safe";
				return;
			}

            BaseWebControl ctrl = FindControl<BaseWebControl>(Ip(e.Params));

			if (ctrl != null)
			{
				new EffectFocusAndSelect(ctrl).Render();
			}
		}

		/*
		 * fills out the stuff from basewebcontrol
		 */
		protected override void FillOutParameters(Node pars, BaseControl ctrl)
		{
			base.FillOutParameters(pars, ctrl);
            BaseWebControl that = ctrl as BaseWebControl;

            Node ip = Ip(pars);
            Node node = ip["_code"].Value as Node;

			if (node.Contains("class") && !string.IsNullOrEmpty(node["class"].Get<string>()))
				that.Class = node["class"].Get<string>();

			if (node.Contains("dir") && !string.IsNullOrEmpty(node["dir"].Get<string>()))
				that.Dir = node["dir"].Get<string>();

            if (node.Contains("tabindex") && !string.IsNullOrEmpty(node["tabindex"].Get<string>()))
                that.TabIndex = node["tabindex"].Get<string>();

            if (node.Contains("title") && !string.IsNullOrEmpty(node["title"].Get<string>()))
                that.Title = node["title"].Get<string>();

			if (node.Contains("style") && !string.IsNullOrEmpty(node["style"].Get<string>()))
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

