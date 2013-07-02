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
checks the web.config setting named ""Magix.Core.PageLoad-HyperLispFile"", and if set, 
expects it to be a pointer to a hyper lisp file, which will be executed";
				return;
			}

			string defaultHyperLispFile = ConfigurationManager.AppSettings["Magix.Core.PageLoad-HyperLispFile"];

			if (!string.IsNullOrEmpty(defaultHyperLispFile))
			{
				Node node = new Node ();
				node.Value = defaultHyperLispFile;

				RaiseActiveEvent(
					"magix.execute.execute-file",
					node);
			}
		}
	}
}

