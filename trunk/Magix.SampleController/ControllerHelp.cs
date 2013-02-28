/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
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
	public class ControllerHelp : ActiveController
	{
		/**
		 * Starts the Magix Illuminate Help System [AKA; Marvin ...]
		 */
		[ActiveEvent(Name = "magix.help.start-help")]
		public void magix_help_start_help(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["inspect"].Value = @"Starts the Magix Illuminate 
Help system.";
				return;
			}

			Node tmp = new Node();
			tmp["form-id"].Value = "help";
			tmp["container"].Value = "modal";
			tmp["header"].Value = "Help from Marvin";

			using (TextReader reader = File.OpenText(Page.Server.MapPath("Help/index.txt")))
			{
				tmp["html"].Value = reader.ReadToEnd();
			}

			RaiseEvent(
				"magix.forms.create-web-page",
				tmp);
		}

		/**
		 * Opens a specific page on disc, as if it was html
		 */
		[ActiveEvent(Name = "magix.help.open-file")]
		public void magix_help_open_file(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();
			tmp["form-id"].Value = "help";

			using (TextReader reader = File.OpenText(Page.Server.MapPath(e.Params["file"].Get<string>())))
			{
				tmp["html"].Value = reader.ReadToEnd();
			}

			RaiseEvent(
				"magix.forms.change-html",
				tmp);
		}
	}
}

