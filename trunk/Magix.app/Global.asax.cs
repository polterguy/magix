/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.Web;

namespace Magix.app
{
	public class Global : HttpApplication
	{
		protected virtual void Application_Start (object sender, EventArgs e)
		{
			// Registering our Resource Provider User-Control-Loader Classes
	        Magix.Core.AssemblyResourceProvider sampleProvider = new Magix.Core.AssemblyResourceProvider();
	        System.Web.Hosting.HostingEnvironment.RegisterVirtualPathProvider(sampleProvider);
		}
		
		protected virtual void Session_Start (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_BeginRequest (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_EndRequest (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_AuthenticateRequest (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_Error (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Session_End (Object sender, EventArgs e)
		{
		}
		
		protected virtual void Application_End (Object sender, EventArgs e)
		{
		}
	}
}

