/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
     * event sniffer
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
		 * 'null Active Event Handler', which will swallow almost every
		 * single Active Event in the system, and show with its parameters,
		 * in a div
		 */
		[ActiveEvent(Name = "")]
		public void magix_null_event_handler(object sender, ActiveEventArgs e)
		{
            if (e.Name != "magix.execute")
                return;

			string code = "";
			if (e.Params != null)
			{
				Node tmp = new Node();
                tmp["node"].Value = Ip(e.Params);

				RaiseActiveEvent(
					"magix.execute.node-2-code",
					tmp);

				if (tmp.Contains("code") && !string.IsNullOrEmpty(tmp["code"].Get<string>()))
					code = "<pre class=\"span-22 last bottom-1\">" + tmp["code"].Get<string>() + "</pre>";
			}
            lbl.Text += code;
		}
	}
}
