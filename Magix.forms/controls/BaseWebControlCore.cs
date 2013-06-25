/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
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

			if (node.Contains("onclick"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onclick"].Clone();

				that.Click += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("ondblclick"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["ondblclick"].Clone();

				that.DblClick += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onmousedown"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onmousedown"].Clone();

				that.MouseDown += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onmouseup"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onmouseup"].Clone();

				that.MouseUp += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onmouseover"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onmouseover"].Clone();

				that.MouseOver += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onmouseout"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onmouseout"].Clone();

				that.MouseOut += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onkeypress"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onkeypress"].Clone();

				that.KeyPress += delegate(object sender, EventArgs e)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}

			if (node.Contains("onesc"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onesc"].Clone();

				that.EscKey += delegate(object sender, EventArgs e)
				{
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
			tmp["inspect"].Value = tmp["inspect"].Get<string>() + @".&nbsp;&nbsp;
[css] is the css class(es) for your control, [dir] is reading direction, and can be ltr, or 
rtl.&nbsp;&nbsp;[tab] is the tab index, or order in tab hierarchy.&nbsp;&nbsp;[tip] 
is an informational tool tip, shown when for instance mouse is hovered above control.&nbsp;&nbsp;
[onclick] is raised when control is clicked.&nbsp;&nbsp;[ondblclick] when controls is 
double clicked.&nbsp;&nbsp;[onmousedown] is raised when mouse is pressed down, on control.&nbsp;&nbsp;
[onmouseup] is raised when mouse is released again, on top of control.&nbsp;&nbsp;[onmouseover] is 
raised when mouse is hovered over control.&nbsp;&nbsp;[onmouseout] is raised when mouse is 
moved out of control surface.&nbsp;&nbsp;[onkeypress] is raised when a key is pressed inside 
of control.&nbsp;&nbsp;[onesc] is raised when escape key is pressed inside of control.
&nbsp;&nbsp;not thread safe";
			node["css"].Value = "css classes";
			node["dir"].Value = "ltr|rtl";
			node["tab"].Value = 5;
			node["tip"].Value = "informational tooltip";
			node["style"].Value = "css style collection";
			base.Inspect(node);
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

