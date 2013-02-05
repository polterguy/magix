/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.SampleController
{
	/**
	 * Please notice, by removing this assembly, you can create your 
	 * own Application Startup logic by overriding the page-load event
	 * to do your own custom logic
	 */
	[ActiveController]
	public class ControllerSample : ActiveController
	{
		[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("initial-load"))
			{
				e.Params["initial-load"].Value = null;
				return;
			}
			// Loads up Event Viewer, or IDE
			Node tmp = new Node();
			tmp["container"].Value = "content1";
			RaiseEvent(
				"magix.admin.open-event-viewer", 
				tmp);
		}
	}
}

