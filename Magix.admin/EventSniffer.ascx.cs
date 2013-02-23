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

namespace Magix.admin
{
    /**
     */
    public class EventSniffer : ActiveModule
    {
		protected Label lbl;

		protected override void OnLoad (EventArgs e)
		{
			base.OnLoad (e);
			lbl.Text = "";
		}

		/**
		 */
		[ActiveEvent(Name = "")]
		public void magix_null_event_handler (object sender, ActiveEventArgs e)
		{
			if (e.Name == "magix.admin._transform-node-2-code")
				return;

			string code = "";
			if (e.Params != null)
			{
				Node tmp = new Node();
				tmp["JSON"].Value = e.Params;
				RaiseEvent(
					"magix.admin._transform-node-2-code",
					tmp);
				code = "<pre>" + tmp["code"].Get<string>() + "</pre>";
			}
			lbl.Text += "<label>" + e.Name + "</label>" + code;
		}
	}
}
