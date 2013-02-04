/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Level3: Controller that encapsulates execution engine
	 */
	[ActiveController]
	public class Utilities : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "Magix.Core.OverrideActiveEvent")]
		public void Magix_Core_OverrideActiveEvent (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("From"))
			{
				e.Params["From"].Value = "Name of event you wish to have associated with To";
				e.Params["To"].Value = "Which event you wish to have raised when From is raised";
				return;
			}
			ActiveEvents.Instance.CreateEventMapping (
				e.Params["From"].Get<string>(),
				e.Params["To"].Get<string>());
		}
	}
}

