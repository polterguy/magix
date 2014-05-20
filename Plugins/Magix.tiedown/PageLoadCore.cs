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
	 * runs the page load hyperlisp file
	 */
	public class PageLoadCore : ActiveController
	{
		/**
		 * runs the page load hyperlisp file
		 */
		[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"executes the index.hl hyperlisp file";
				return;
			}

			Node node = new Node();
            node["file"].Value = "plugin:magix.file.load-from-resource";
            node["file"]["assembly"].Value = "Magix.tiedown";
            node["file"]["resource-name"].Value = "Magix.tiedown.hyperlisp.index.hl";

			RaiseActiveEvent(
				"magix.execute.execute-script",
				node);
		}
	}
}

