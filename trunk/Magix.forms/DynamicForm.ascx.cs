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

					if (DataSource.Contains("parent-css"))
						pnl.CssClass = DataSource["parent-css"].Get<string>();
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
				BuildControl(idx, pnl);
			}
			foreach (Node idx in DataSource["events"])
			{
				BuildControl(idx, pnl);
			}
		}

		private void CleanUpDataSource(Node node)
		{
			bool hasMore = true;
			while (hasMore)
			{
				hasMore = false;
				foreach (Node idx in node)
				{
					if (idx.Name == "_buffer")
					{
						idx.UnTie();
						hasMore = true;
						break;
					}
				}
			}
			foreach (Node idx in node)
			{
				CleanUpDataSource(idx);
			}
		}
		
		protected override void ReRender()
		{
			isFirst = true;

			ViewState[ClientID + "_FirstLoad"] = null;

			CleanUpDataSource(DataSource);

			pnl.Controls.Clear();
			BuildControls();
			pnl.ReRender();
		}
    }
}
