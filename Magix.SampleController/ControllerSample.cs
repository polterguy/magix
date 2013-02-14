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
	public class ControllerSample : ActiveController
	{
		/**
		 * Handled to make sure we load our Active Event Executor by default
		 */
		[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["initial-load"].Value = null;
				e.Params["inspect"].Value = @"This active event is being 
raised upon initial loading of a page, and is kind of the doorway event
from which everything else occurs as a consequence of. This is where you
lload up your default forms and such. Remove this controller, and replace 
with your own page-load handling logic to create your own applications.";
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

