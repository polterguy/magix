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
	/**
	 * contains the basewebcontrol control
	 */
	public abstract class BaseWebControlCore : BaseControlCore
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

				ctrl.CssClass = className;
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
                Ip(e.Params)["value"].Value = ctrl.CssClass;
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
                if (ctrl.CssClass.IndexOf(Ip(e.Params)["css"].Get<string>()) == -1)
				{
                    ctrl.CssClass = ctrl.CssClass.Trim() + " " + Ip(e.Params)["css"].Get<string>().Trim();
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
                if (ctrl.CssClass.IndexOf(Ip(e.Params)["css"].Get<string>()) != -1)
				{
                    ctrl.CssClass = ctrl.CssClass.Replace(Ip(e.Params)["css"].Get<string>().Trim(), "").Replace("  ", " ").Trim();
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
		protected override void FillOutParameters(Node node, BaseControl ctrl)
		{
			base.FillOutParameters(node, ctrl);

			BaseWebControl that = ctrl as BaseWebControl;

			if (node.Contains("css") && !string.IsNullOrEmpty(node["css"].Get<string>()))
				that.CssClass = node["css"].Get<string>();

			if (node.Contains("dir") && !string.IsNullOrEmpty(node["dir"].Get<string>()))
				that.Dir = node["dir"].Get<string>();

			if (node.Contains("tab") && !string.IsNullOrEmpty(node["tab"].Get<string>()))
				that.TabIndex = node["tab"].Get<string>();

			if (node.Contains("tip") && !string.IsNullOrEmpty(node["tip"].Get<string>()))
				that.ToolTip = node["tip"].Get<string>();

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
				that.EscKey += delegate(object sender, EventArgs e)
				{
                    FillOutEventInputParameters(codeNode, sender);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

            // filling out extra attributes for control
            if (ctrl is AttributeControl)
            {
                AttributeControl atrCtrl = ctrl as AttributeControl;
                foreach (Node idx in node)
                {
                    if (idx.Name.StartsWith("@"))
                    {
                        atrCtrl.Attributes.Add(new AttributeControl.Attribute(idx.Name.Substring(1), idx.Get<string>()));
                    }
                }
            }
            else
            {
                foreach (Node idx in node)
                {
                    if (idx.Name.StartsWith("@"))
                    {
                        throw new ArgumentException("tried to add up a generic attribute to a control which didn't support it");
                    }
                }
            }
		}

		protected override void Inspect(Node node)
		{
			Node tmp = node;
			while (!tmp.Contains("inspect"))
				tmp = tmp.Parent;
            base.Inspect(node);
            tmp["inspect"].Value = tmp["inspect"].Get<string>() + @"<p><strong>properties inherited from web control</strong></p>
<p>[css] is the css class(es) for your control.&nbsp;&nbsp;these can be modified or retrieved after you 
have created your control, with for instance [magix.forms.set-class] and [magix.forms.add-css], 
or [magix.forms.get-class] and [magix.forms.remove-css]</p>
<p>[style] is a collection of inline css styles.&nbsp;&nbsp;create these the same way you would within a css class in a css 
file.&nbsp;&nbsp;name of styles and values of styles are separated by colon ':', while a style setting must end with a 
semi-colon ';'.&nbsp;&nbsp;for example ""width:100px;height:50px;""</p>
<p>[dir] is the reading direction of text on your web control, and can be either 'ltr', or 
'rtl'.&nbsp;&nbsp;ltr means left-to-right, and is what is used for most languages.&nbsp;&nbsp;
use rtl as the [dir] for languages such as arabic and hebrew, which reads from right to left</p>
<p>[tab] is the tab index, or order in your forms tab hierarchy.&nbsp;&nbsp;the lower this number is, the earlier 
the control will gain focus in the tab order.&nbsp;&nbsp;for instance, if you have two controls, 
one with [tab] equal to 1, and another with [tab] equal to 5, then the control with tab order 
of 1 will have focus before the one with tab order of 5, as the end user clicks tab to 
move focus through the form's controls</p><p>[tip] 
is an informational tool tip, shown when the mouse is hovered above control.&nbsp;&nbsp;notice that this 
property only works for desktop system, which has a mouse, which can be hovered above your control's surface</p><p>
[onclick] is raised when control is clicked.&nbsp;&nbsp;attach any hyper lisp code you wish here</p>
<p>[ondblclick] is raised when controls is 
double clicked.&nbsp;&nbsp;attach any hyper lisp code you wish here</p>
<p>[onmousedown] is raised when the left mouse button is pressed down on control, but before it 
is released.&nbsp;&nbsp;attach any hyper lisp code you wish here</p><p>
[onmouseup] is raised when mouse is released again.&nbsp;&nbsp;attach any hyper lisp code you wish here</p><p>[onmouseover] is 
raised when mouse is hovered over control.&nbsp;&nbsp;with magix, the [onmouseover] and [onmouseout] is 
logically fixed in regards to the html standard, such that if you have a panel, which has child controls, then 
as long as you move the mouse over the panel, it will raise the [onmouseover].&nbsp;&nbsp;and not before you 
move the mouse outside of the panel and its child controls, the [onmouseout] will be raised.&nbsp;&nbsp;
please notice that this is different from the default brower logic, though more sensible in regards to 
your web application development.&nbsp;&nbsp;attach any hyper lisp code you wish here</p>
<p>[onmouseout] is raised when mouse is 
moved out of control surface.&nbsp;&nbsp;with magix, the [onmouseover] and [onmouseout] is 
logically fixed in regards to the html standard, such that if you have a panel, which has child controls, then 
as long as you move the mouse over the panel, it will raise the [onmouseover].&nbsp;&nbsp;and not before you 
move the mouse outside of the panel and its child controls, the [onmouseout] will be raised.&nbsp;&nbsp;
please notice that this is different from the default brower logic, though more sensible in regards to 
your web application development.&nbsp;&nbsp;attach any hyper lisp code you wish here</p>
<p>[onkeypress] is raised when a key is pressed inside 
of web control, and your web control has focus somehow.&nbsp;&nbsp;attach any hyper lisp code you wish here</p>
<p>[onesc] is raised when escape key is pressed inside of control.&nbsp;&nbsp;this is an active event which 
are added for convenience, since very often it is interesting knowing if the user clicks esc, such that 
the application can close forms, or undo actions, and similar types of functionality.&nbsp;&nbsp;
attach any hyper lisp code you wish here</p>";
			node["css"].Value = "css classes";
			node["dir"].Value = "ltr";
			node["tab"].Value = 5;
			node["tip"].Value = "informational tooltip";
			node["style"].Value = "width:120px;height:120px;";
			node["onclick"].Value = "hyper lisp code";
			node["ondblclick"].Value = "hyper lisp code";
			node["onmousedown"].Value = "hyper lisp code";
			node["onmouseup"].Value = "hyper lisp code";
			node["onmouseover"].Value = "hyper lisp code";
			node["onmouseout"].Value = "hyper lisp code";
			node["onkeypress"].Value = "hyper lisp code";
			node["onesc"].Value = "hyper lisp code";
		}
	}
}

