/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using Magix.Core;

namespace Magix.SampleController
{
	/**
	 * Please notice, by removing this assembly, and replacing it with
	 * another Assembly, containing a controller, which handles the
	 * "magix.viewport.page-load" event, you can create your 
	 * own Application Startup logic, which effectively would become the 
	 * starting point of your application, serving up whatever Module
	 * you wish to use
	 */
	public class ControllerSample : ActiveController
	{
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			string defaultHyperLispFile = ConfigurationManager.AppSettings["Magix.Core.AppStart-HyperLispFile"];

			if (!string.IsNullOrEmpty(defaultHyperLispFile))
			{
				Node node = new Node ();
				node ["file"].Value = defaultHyperLispFile;

				RaiseActiveEvent(
					"magix.admin.run-file",
					node);
			}

			Node tp = new Node();

			tp["prototype"]["type"].Value = "magix.pages.page";
			tp["prototype"]["name"].Value = "default";

			RaiseActiveEvent(
				"magix.data.load",
				tp);

			if (!tp.Contains("objects") || tp["objects"].Count == 0)
			{
				// creating default page ...
				Node tmp = new Node();

				tmp["object"]["value"].Value = @"
<h1>welcome to magix illuminate</h1>
<p>magix is an open source web app builder program, which means you can create your own
applications with magix.&nbsp;&nbsp;click <a href='?dashboard='>here to start building apps</a></p>
<p>you will be asked for your username and password, which by default is admin/admin, make sure 
you change this as fast as you can, since otherwise your system is not secure</p>
".Replace("\n", "").Replace("\r\n", "");
				tmp["object"]["type"].Value = "magix.pages.page";
				tmp["object"]["name"].Value = "default";

				RaiseActiveEvent(
					"magix.data.save",
					tmp);
			}

			new Node();

			tp["prototype"]["type"].Value = "magix.core.user";

			RaiseActiveEvent(
				"magix.data.load",
				tp);

			if (!tp.Contains("objects") || tp["objects"].Count == 0)
			{
				// creating default page ...
				Node tmp = new Node();

				tmp["object"]["type"].Value = "magix.core.user";
				tmp["object"]["name"].Value = "admin";
				tmp["object"]["password"].Value = "admin";

				RaiseActiveEvent(
					"magix.data.save",
					tmp);
			}
		}

		/**
		 * Handled to make sure we load our Active Event Executor by default.
		 * Replace this code, with your own Active Event handler, which handles
		 * the page-load event, and you've got your own application
		 */
		[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load_2(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"raised when page is initially loaded, 
or refreshed.&nbsp;&nbsp;will by default load a page, either the default page or the given 
'page' http get parameter page, or if your web.config contains 'Magix.Core.PageLoad-HyperLispFile' it will
run that hyperlisp file, and nothing else, unless http get parameter 'page' is given";
				return;
			}

			if (Page.Request.Params["dashboard"] != null)
			{
				if (Page.Session["magix.core.user"] != null)
					RaiseActiveEvent("magix.admin.load-start");
				else
					RaiseActiveEvent("magix.admin.load-login");
			}
			else
			{
				string defaultHyperLispFile = ConfigurationManager.AppSettings["Magix.Core.PageLoad-HyperLispFile"];
				string page = Page.Request.Params["page"] ?? "default";

				if (!string.IsNullOrEmpty(defaultHyperLispFile) && page == "default")
				{
					Node node = new Node ();
					node ["file"].Value = defaultHyperLispFile;

					RaiseActiveEvent(
						"magix.admin.run-file",
						node);
				}
				else
				{
					Node tp = new Node();

					tp["prototype"]["type"].Value = "magix.pages.page";
					tp["prototype"]["name"].Value = page;

					RaiseActiveEvent(
						"magix.data.load",
						tp);

					Node tmp = new Node();

					tmp["html"].Value = tp["objects"][0]["value"].Value;
					tmp["container"].Value = "content1";
					tmp["css"].Value = "span12";
					tmp["form-id"].Value = "webpages";

					RaiseActiveEvent(
						"magix.forms.create-web-page",
						tmp);
				}
			}
		}

		[ActiveEvent(Name = "magix.admin.load-login")]
		public void magix_viewport_load_login(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["file"].Value = "core-scripts/misc/login-box.hl";

			RaiseActiveEvent(
				"magix.admin.run-file",
				tmp);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.admin.load-start")]
		public void magix_viewport_load_start(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"loads up the default administrator dashboard";
				return;
			}

			Node tmp = new Node();

			tmp["form-id"].Value = "header";
			tmp["html"].Value = "<h1 class=\"span9 offset3\">active event executor</h1>";
			tmp["container"].Value = "header";
			tmp["css"].Value = "span12";

			RaiseActiveEvent(
				"magix.forms.create-web-page",
				tmp);

			// Loads up Event Viewer, or IDE
			tmp = new Node();
			tmp["container"].Value = "content1";

			RaiseActiveEvent(
				"magix.admin.open-event-executor", 
				tmp);

			tmp = new Node();

			tmp["file"].Value = "core-scripts/misc/main-menu.hl";

			RaiseActiveEvent(
				"magix.admin.run-file",
				tmp);

			Node del = new Node();
			del["container"].Value = "footer";

			RaiseActiveEvent(
				"magix.viewport.clear-controls",
				del);
		}
	}
}

