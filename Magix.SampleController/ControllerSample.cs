/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
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
		/**
		 * Handled to make sure we load our Active Event Executor by default.
		 * Replace this code, with your own Active Event handler, which handles
		 * the page-load event, and you've got your own application
		 */
		[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"raised when page is initially loaded, 
or refreshed";
				return;
			}
			RaiseActiveEvent("magix.admin.load-start");
		}

		/**
		 */
		[ActiveEvent(Name = "magix.admin.load-start")]
		public void magix_viewport_load_start(object sender, ActiveEventArgs e)
		{
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
			tmp["container"].Value = "content";

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

