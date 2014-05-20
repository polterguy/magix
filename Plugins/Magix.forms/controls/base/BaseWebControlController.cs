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
		/**
		 * sets css classes
		 */
		[ActiveEvent(Name = "magix.forms.set-class")]
		protected static void magix_forms_set_class(object sender, ActiveEventArgs e)
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

		/**
		 * retrieves css classes
		 */
		[ActiveEvent(Name = "magix.forms.get-class")]
		protected static void magix_forms_get_class(object sender, ActiveEventArgs e)
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

		/**
		 * adds css class
		 */
		[ActiveEvent(Name = "magix.forms.add-css")]
		protected static void magix_forms_add_css(object sender, ActiveEventArgs e)
		{
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.add-css"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["css"].Value = "css-class-to-add";
				e.Params["inspect"].Value = @"adds the given [css] to the css class name of the 
[id] control.&nbsp;&nbsp;not thread safe";
				return;
			}

            if (!Ip(e.Params).Contains("css"))
				throw new ArgumentException("Missing [css] in add-css");

            BaseWebControl ctrl = FindControl<BaseWebControl>(Ip(e.Params));

			if (ctrl != null)
			{
                if (ctrl.Class.IndexOf(Ip(e.Params)["css"].Get<string>()) == -1)
				{
                    ctrl.Class = ctrl.Class.Trim() + " " + Ip(e.Params)["css"].Get<string>().Trim();
				}
			}
		}

		/**
		 * removes css class
		 */
		[ActiveEvent(Name = "magix.forms.remove-css")]
		protected static void magix_forms_remove_css(object sender, ActiveEventArgs e)
		{
            if (ShouldInspect(e.Params))
            {
				e.Params["event:magix.forms.remove-css"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["css"].Value = "css-class-to-remove";
				e.Params["inspect"].Value = @"removes the given [css] from the css class property of the 
[id] control.&nbsp;&nbsp;not thread safe";
				return;
			}

            if (!Ip(e.Params).Contains("css"))
				throw new ArgumentException("Missing [css] in remove-css");

            BaseWebControl ctrl = FindControl<BaseWebControl>(Ip(e.Params));

			if (ctrl != null)
			{
                if (ctrl.Class.IndexOf(Ip(e.Params)["css"].Get<string>()) != -1)
				{
                    ctrl.Class = ctrl.Class.Replace(Ip(e.Params)["css"].Get<string>().Trim(), "").Replace("  ", " ").Trim();
				}
			}
		}

		/**
		 * sets focus to a specific mux web control
		 */
		[ActiveEvent(Name = "magix.forms.set-focus")]
		protected static void magix_forms_set_focus(object sender, ActiveEventArgs e)
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

		/**
		 * fills out the stuff from basewebcontrol
		 */
		protected override void FillOutParameters(Node pars, BaseControl ctrl)
		{
			base.FillOutParameters(pars, ctrl);

            Node node = Ip(pars)["_code"].Value as Node;

            BaseWebControl that = ctrl as BaseWebControl;

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
					node["style"].Get<string>().Split(
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
			Node tmp = node;
			while (!tmp.Contains("inspect"))
				tmp = tmp.Parent;
            base.Inspect(node);
            AppendInspect(tmp["inspect"], @"[class] is the css class(es) for your 
control.  these can be modified or retrieved after you have created your control, 
with for instance [magix.forms.set-class] and [magix.forms.get-class]

[style] is a collection of inline css styles.  create these the same way you would 
within a css class in a css file.  name of styles and values of styles are separated 
by colon ':', while a style setting must end with a semi-colon ';'.  for example 
""width:100px;height:50px;""

[dir] is the reading direction of text on your web control, and can be either 'ltr', 
or 'rtl'.  ltr means left-to-right, and is what is used for most languages.  use rtl 
as the [dir] for languages such as arabic and hebrew, which reads from right to left

[tabindex] is the tab index, or order in your forms tab hierarchy.  the lower this 
number is, the earlier the control will gain focus as the user clicks tab to cycle 
focus through the web controls on the form.  for instance, if you have two controls, 
one with tabindex equal to 1, and another with tabindex equal to 5, then the control 
with tabindex of 1 will have focus before the one with tabindex of 5, as the end user 
clicks tab to move focus through the form's controls

[title] is an informational tool tip, shown when the mouse is hovered above control.  
notice that this property only works for desktop system, which has a mouse, which can 
be hovered above your control's surface

[onclick] is raised when the web control is clicked.  attach any hyperlisp code you 
wish here

[ondblclick] is raised when web control is double clicked with the left mouse button, 
or somehow.  attach any hyperlisp code you wish here

[onmousedown] is raised when the left mouse button is pressed down on control, but 
before it is released.  attach any hyperlisp code you wish here

[onmouseup] is raised when mouse is released again.  attach any hyperlisp code you 
wish here

[onmouseover] is raised when mouse is hovered over control.  with magix, the 
onmouseover and onmouseout is logically fixed in regards to the html standard, such 
that if you have a panel, which has child controls, then as long as you move the 
mouse over the panel, it will raise onmouseover.  and not before you move the mouse 
outside of the panel and its child controls, the onmouseout will be raised.  please 
notice that this is different from the default brower logic, though more sensible in 
regards to your web application development.  attach any hyperlisp code you wish here

[onmouseout] is raised when mouse is moved out of control surface.  attach any hyper 
lisp code you wish here

[onkeypress] is raised when a key is pressed inside of web control, and your web 
control has focus somehow.  attach any hyperlisp code you wish here

[onesc] is raised when escape key is pressed inside of control.  this is an active 
event which is added for convenience, since very often it is interesting knowing if 
the user clicks esc, such that the application can close forms, or undo actions, and 
similar types of functionality.  attach any hyperlisp code you wish here", true);
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

