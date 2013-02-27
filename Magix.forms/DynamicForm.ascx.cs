/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.Core;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
    /**
     * Active module encapsulating a dynamically created form
     */
    public class DynamicForm : DynamicFormBase
    {
        protected Panel pnl;

		private Node DataSource
		{
			get { return ViewState["DataSource"] as Node; }
			set { ViewState["DataSource"] = value; }
		}

		public override void InitialLoading(Node node)
		{
			Load +=
				delegate
				{
					DataSource = node.Clone();

					if (!DataSource.Contains("controls"))
						throw new ArgumentException("Couldn't find any 'controls' node underneath form");
				};

			base.InitialLoading(node);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			BuildControls();
		}

		private void BuildControls()
		{
			foreach (Node idx in DataSource["controls"])
			{
				BuildControl(idx, pnl, 
					delegate(string path)
					{
						return GetNode(path, DataSource);
					});
			}
		}
    }
}
