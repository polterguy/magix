/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
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

namespace Magix.ide
{
    public class Editor : ActiveModule
    {
		protected TextArea surface;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (FirstLoad)
			{
				AjaxManager.Instance.IncludeScriptFromFile("media/xing-wysihtml5/parser_rules/advanced.js");
				AjaxManager.Instance.IncludeScriptFromFile("media/xing-wysihtml5/dist/wysihtml5-0.4.0pre.min.js");
				AjaxManager.Instance.WriterAtBack.InnerWriter.Write(
					"window.actualEditor = new wysihtml5.Editor('" + 
					surface.ClientID + 
					@"', {
toolbar:'wysihtml5-editor-toolbar',
stylesheets: ['css/editor.css'],
useLineBreaks: false,
parserRules: wysihtml5ParserRules});");
			}
		}

		[ActiveEvent(Name="magix.execute.get-page-value")]
		protected void magix_execute_get_page_value(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"returns mml of editor as [value].&nbsp;&nbsp;
not thread safe";
				e.Params["get-page-value"].Value = null;
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			ip["value"].Value = surface.Text;
		}

		[ActiveEvent(Name = "magix.execute.load-page")]
		protected void magix_execute_load_page(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"loads the page with the given [name].&nbsp;&nbsp;
not thread safe";
				e.Params["load-page"]["name"].Value = "some-name-of-page";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Node tp = new Node();

			tp["prototype"]["type"].Value = "magix.pages.page";
			tp["prototype"]["name"].Value = ip["name"].Get<string>();

			RaiseEvent(
				"magix.data.load",
				tp);

			if (tp.Contains("objects") && tp["objects"].Count > 0)
			{
				AjaxManager.Instance.WriterAtBack.Write("window.actualEditor.setValue('" + 
				tp["objects"][0]["value"].Value + "');");
			}
		}

		[ActiveEvent(Name="magix.execute.save-page")]
		protected void magix_execute_save_page(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"saves the content of the page from [value] as [name].&nbsp;&nbsp;
not thread safe";
				e.Params["save-page"]["name"].Value = "somename";
				e.Params["save-page"]["value"].Value = "<p>some magix markup language</p>";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("name"))
				throw new ArgumentException("no [name] given to save page as");

			if (!ip.Contains("value"))
				throw new ArgumentException("no [value] given to save page");

			Node tp = new Node();

			tp["prototype"]["type"].Value = "magix.pages.page";
			tp["prototype"]["name"].Value = ip["name"].Get<string>();

			RaiseEvent(
				"magix.data.load",
				tp);

			Node tmp = new Node();

			if (tp.Contains("objects") && tp["objects"].Count > 0)
			{
				tmp["id"].Value = tp["objects"][0].Name;
			}

			tmp["object"]["value"].Value = ip["value"].Get<string>();
			tmp["object"]["type"].Value = "magix.pages.page";
			tmp["object"]["name"].Value = ip["name"].Get<string>();

			RaiseEvent(
				"magix.data.save",
				tmp);
		}

		[ActiveEvent(Name="magix.execute.delete-page")]
		protected void magix_execute_delete_page(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"deletes the page with the name of [name].&nbsp;&nbsp;
not thread safe";
				e.Params["delete-page"]["name"].Value = "somename";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("name"))
				throw new ArgumentException("no [name] given to save page as");

			Node tp = new Node();

			tp["prototype"]["type"].Value = "magix.pages.page";
			tp["prototype"]["name"].Value = ip["name"].Get<string>();

			RaiseEvent(
				"magix.data.remove",
				tp);
		}

		[ActiveEvent(Name = "magix.execute.list-pages")]
		protected void magix_execute_list_pages(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"lists all pages available in system in [pages].&nbsp;&nbsp;
not thread safe";
				e.Params["list-pages"].Value = null;
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Node tmp = new Node();

			tmp["prototype"]["type"].Value = "magix.pages.page";

			RaiseEvent(
				"magix.data.load",
				tmp);

			foreach (Node idx in tmp["objects"])
			{
				Node tp = new Node("page");
				tp["name"].Value = idx["name"].Get<string>();
				tp["object-id"].Value = idx.Name;
				ip["pages"].Add(tp);
			}
		}
	}
}
