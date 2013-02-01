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

namespace Magix.SampleModules
{
    /**
     */
	[ActiveModule]
    public class ComplexSample : ActiveModule
    {
		protected Label lbl;
		protected Panel wrp;

		public override void InitialLoading (Node node)
		{
			base.InitialLoading (node);
			lbl.Text = string.Format ("Caller says; {0} - Controller says; {1}",
			                          node["Message"].Get<string>("nothing"),
			                          node["MessageFromController"].Get<string>("nothing"));
			new EffectTimeout(500)
				.ChainThese (
					new EffectFadeIn(wrp, 500))
				.Render ();
		}

		protected void rabbit_Click(object sender, EventArgs e)
		{
			Node node = new Node();
			node["Container"].Value = "content1";
			RaiseEvent("Magix.Samples.OpenEventViewer", ref node);
		}
    }
}
