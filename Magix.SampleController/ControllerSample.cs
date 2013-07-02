/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using Magix.Core;

namespace Magix.SampleController
{
	/**
	 */
	public class ControllerSample : ActiveController
	{
		// uncomment below line to create a c# event handler for the application startup active event
		//[ActiveEvent(Name = "magix.core.application-startup")]
		public void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
		}

		// uncomment below line to create a c# event handler for the page load active event
		//[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load(object sender, ActiveEventArgs e)
		{
		}
	}
}

