/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.modules
{
    /**
     * Modules to show arbitrary HTML
     */
    public class PlainMenu : ActiveModule
    {
		protected System.Web.UI.WebControls.Repeater rep;
		protected Panel wrp;

		private string WidgetID
		{
			get { return ViewState["WidgetID"] as string; }
			set { ViewState["WidgetID"] = value; }
		}

		public override void InitialLoading (Node node)
		{
			base.InitialLoading (node);

			if (!node.Contains ("id"))
				throw new ArgumentNullException("Cannot load an HtmlViewer without assigning it an id");

			WidgetID = node["id"].Get<string>();
		}

		/**
		 */
		[ActiveEvent(Name = "magix.modules.set-menu-items")]
		public void magix_modules_set_menu_items(object sender, ActiveEventArgs e)
		{
			rep.DataSource = e.Params["Items"];
			rep.DataBind();
			wrp.ReRender();
		}
	}
}
