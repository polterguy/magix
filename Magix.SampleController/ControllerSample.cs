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
			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"raised when page is initially loaded, 
or refreshed";
				return;
			}

			Node tmp = new Node();

			tmp["form-id"].Value = "header";
			tmp["html"].Value = "<h1>Active Event Executor</h1>";
			tmp["container"].Value = "header";

			RaiseEvent(
				"magix.forms.create-web-page",
				tmp);

			// Loads up Event Viewer, or IDE
			tmp = new Node();
			tmp["container"].Value = "content";

			RaiseEvent(
				"magix.admin.open-event-executor", 
				tmp);

			tmp = new Node();
			tmp["id"].Value = "menu";

			LoadModule(
				"Magix.modules.PlainMenu",
				"menu",
				tmp);

			tmp = new Node();

			tmp["Items"]["Item1"].Value = "help";
			tmp["Items"]["Item1"]["Event"].Value = "magix.help.start-help";
			tmp["Items"]["Item2"].Value = "executor";
			tmp["Items"]["Item2"]["Event"].Value = "magix.admin.open-event-executor";
			tmp["Items"]["Item2"]["Event"]["container"].Value = "content";

			RaiseEvent(
				"magix.modules.set-menu-items", 
				tmp);
		}
	}
}

