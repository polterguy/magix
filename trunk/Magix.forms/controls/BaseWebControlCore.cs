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

			if (node.Contains("onclick"))
			{
				Node codeNode = node["onclick"].Clone();

				that.Click += delegate(object sender, EventArgs e)
				{
					RaiseEvent(
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
is an informational tool tip, shown when for instance mouse is hovered above control";
			base.Inspect(node);
			node["css"].Value = "css classes";
			node["dir"].Value = "ltr";
			node["tab"].Value = 5;
			node["tip"].Value = "informational tooltip";
		}
	}
}

