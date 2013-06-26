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

namespace Magix.forms
{
    /**
     * Modules to show arbitrary HTML
     */
    public class HtmlViewer : DynamicFormBase
    {
		protected Panel pnl;

		private string Html
		{
			get { return ViewState["html"] as string; }
			set { ViewState["html"] = value; }
		}

		private List<Node> DataSources
		{
			get { return ViewState["DataSources"] as List<Node>; }
			set { ViewState["DataSources"] = value; }
		}

		public override void InitialLoading(Node node)
		{
			if (!node.Contains("html"))
				throw new ArgumentException("Cannot load an HtmlViewer without 'html'");

			Load +=
				delegate
				{
					Html = node["html"].Get<string>();
					DataSources = new List<Node>();
				};
			base.InitialLoading(node);
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EnsureChildControls();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			BuildControls();
		}

		private void BuildControls()
		{
			bool dataSources = DataSources.Count != 0;

			string html = Html;
			string buffer = "";
			int idxPre = 0;
			bool last = false;

			for (int idx = 0; idx < html.Length; idx++)
			{
				switch (html[idx])
				{
				case '{':
					if (!last && (idx < html.Length && html[idx + 1] != '{' ))
					{
						buffer += html[idx];
						continue;
					}
					if (last || idxPre == 0)
					{
						idxPre += 1;
						if (idxPre == 1)
						{
							// Finished with brand new HTML stuff
							LiteralControl lit = new LiteralControl();
							lit.Text = buffer;
							pnl.Controls.Add(lit);
							buffer = "";
						}
						last = true;
					} break;
				case '}':
					if (!last && (idx < html.Length && html[idx + 1] != '}' ))
					{
						buffer += html[idx];
						continue;
					}
					if (last || idxPre == 2)
					{
						idxPre -= 1;
						if (idxPre == 0)
						{
							// Finished with brand new Control collection
							Node tmp = new Node();
							tmp["code"].Value = buffer.Replace("&gt;", ">").Replace("&lt;", "<");

							RaiseEvent(
								"magix.code.code-2-node",
								tmp);

							Node codeNode = tmp["json"].Get<Node>();

							if (!dataSources)
								DataSources.Add(codeNode);

							foreach (Node idxN in codeNode)
							{
								BuildControl(idxN, pnl);
							}
							buffer = "";
						}
						last = true;
					}
					break;
				default:
					last = false;
					buffer += html[idx];
					break;
				}
			}

			if (!string.IsNullOrEmpty(buffer))
			{
				LiteralControl lit = new LiteralControl();
				lit.Text = buffer;
				pnl.Controls.Add(lit);
			}
		}

		/**
		 * Changes the HTML of the form
		 */
        [ActiveEvent(Name = "magix.forms.change-html")]
		protected void magix_forms_change_html(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.change-html"].Value = null;
				e.Params["inspect"].Value = @"changes the html of the 
given [form-id] to what is in [html].&nbsp;&nbsp;not thread safe";
				e.Params["form-id"].Value = "header";
				e.Params["html"].Value = @"
hello world";
				return;
			}

			if (!e.Params.Contains ("html"))
				throw new ArgumentException("Need html parameter to change-html");

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("Need a form-id to change-html");

			if (e.Params["form-id"].Get<string>() != FormID)
				return;

			isFirst = true;

			ViewState[ClientID + "_FirstLoad"] = null;

			DataSources.Clear();
			Html = e.Params["html"].Get<string>();
			pnl.Controls.Clear();
			BuildControls();
			pnl.ReRender();
		}
		
		/**
		 */
		[ActiveEvent(Name = "magix.forms.re-render")]
		protected void magix_forms_re_render(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.re-render"].Value = null;
				e.Params["inspect"].Value = @"re renders the
form with the [form-id].&nbsp;&nbsp;not thread safe";
				e.Params["form-id"].Value = "header";
				return;
			}

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("Need a [form-id] to re-render");

			if (e.Params["form-id"].Get<string>() != FormID)
				return;

			isFirst = true;

			ViewState[ClientID + "_FirstLoad"] = null;

			DataSources.Clear();
			pnl.Controls.Clear();
			BuildControls();
			pnl.ReRender();
		}
	}
}
