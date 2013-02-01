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
    public class ButtonSample : ActiveModule
    {
        protected Button but;

        protected void but_Click(object sender, EventArgs e)
        {
			but.Text = "Clicked!";
			but.Enabled = false;
			but.Style[Styles.fontSize] = "12px";
			but.CssClass = "but-clicked";
			new EffectSize(but, 500, 50, 50)
				.Render ();

			Node node = new Node();
			node["Message"].Value = "Message from Button Module is; 'Hi!'";
			RaiseEvent ("Magix.Samples.FirstEvent", ref node);
        }
    }
}
