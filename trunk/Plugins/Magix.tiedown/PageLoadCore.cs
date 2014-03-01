/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using Magix.Core;

namespace Magix.tiedown
{
	/**
	 * runs the page load hyper lisp file
	 */
	public class PageLoadCore : ActiveController
	{
		/**
		 * runs the page load hyper lisp file
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

