﻿/*
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

namespace Magix.SampleModules
{
    /**
     */
	[ActiveModule]
    public class EventViewer : ActiveModule
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
				activeEvent.Select ();
				activeEvent.Focus ();
			}
		}

		[ActiveEvent(Name = "Magix.Samples.PopulateEventViewer")]
		public void Magix_Samples_PopulateEventViewer (object sender, ActiveEventArgs e)
		{
			if (e.Params.Count == 0)
			{
				e.Params["ActiveEvents"]["Active_Event_No_1"].Value = "Name.Of.Active.Events";
			}
			else
			{
				rep.DataSource = e.Params ["ActiveEvents"];
				rep.DataBind ();
				wrp.ReRender ();
			}
		}

		protected void run_Click (object sender, EventArgs e)
		{
			if (txtIn.Text != "")
			{
				Node node = Node.FromJSONString (txtIn.Text);
				RaiseEvent (activeEvent.Text, ref node);
				txtOut.Text = node.ToJSONString ();
			}
			else
			{
				Node node = RaiseEvent (activeEvent.Text);
				txtOut.Text = node.ToJSONString ();
				txtOut.Select ();
				txtOut.Focus ();
			}
		}

		protected void paste_Click (object sender, EventArgs e)
		{
			txtIn.Text = txtOut.Text;
			txtIn.Select ();
			txtIn.Focus ();
		}

		protected void EventClicked(object sender, EventArgs e)
		{
			LinkButton btn = sender as LinkButton;
			activeEvent.Text = btn.Text;
			activeEvent.Select ();
			activeEvent.Focus ();
			txtIn.Text = "";
		}
	}
}
