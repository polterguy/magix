/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
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

namespace Magix.admin
{
    /**
     */
	[ActiveModule]
    public class ExecutorForm : ActiveModule
    {
		protected TextArea activeEvent;
		protected TextArea txtIn;
		protected TextArea txtOut;
		protected Panel wrp;
		protected System.Web.UI.WebControls.Repeater rep;

		protected void Page_Load (object sender, EventArgs e)
		{
			if (this.FirstLoad)
			{
				txtIn.Select ();
				txtIn.Focus ();
				activeEvent.Text = "magix.execute";
				txtIn.Text = @"raise=>magix.viewport.show-message
  params
    message=>Hi Thomas!";
			}
		}

		[ActiveEvent(Name = "magix.execute._event-overridden")]
		public void magix_execute__event_overridden (object sender, ActiveEventArgs e)
		{
			Node node = new Node();
			RaiseEvent ("magix.admin.get-active-events", node);
			rep.DataSource = node ["ActiveEvents"];
			rep.DataBind ();
			wrp.ReRender ();
		}

		[ActiveEvent(Name = "magix.execute._event-override-removed")]
		public void magix_execute__event_override_removed (object sender, ActiveEventArgs e)
		{
			Node node = new Node();
			RaiseEvent ("magix.admin.get-active-events", node);
			rep.DataSource = node ["ActiveEvents"];
			rep.DataBind ();
			wrp.ReRender ();
		}

		protected string GetCSS(object value)
		{
			return "span-7 " + (string)value;
		}

		protected void run_Click (object sender, EventArgs e)
		{
			if (txtIn.Text != "")
			{
				Node tmp = new Node();
				tmp["Code"].Value = txtIn.Text;
				RaiseEvent (
					"magix.core._transform-code-2-node",
					tmp);
				RaiseEvent (activeEvent.Text, tmp["JSON"].Get<Node>());
				RaiseEvent (
					"magix.core._transform-node-2-code", tmp);
				txtOut.Text = tmp["Code"].Get<string>();
			}
			else
			{
				Node node = RaiseEvent (activeEvent.Text);
				Node tmp = new Node();
				tmp["JSON"].Value = node;
				RaiseEvent (
					"magix.core._transform-node-2-code", 
					tmp);
				txtOut.Text = tmp["Code"].Get<string>();
				txtOut.Select ();
				txtOut.Focus ();
			}
		}

		protected void EventClicked(object sender, EventArgs e)
		{
			LinkButton btn = sender as LinkButton;
			activeEvent.Text = btn.Text;
			activeEvent.Select ();
			activeEvent.Focus ();
		}
	}
}
