/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using Magix.Core;

namespace Magix.tiedown
{
	/*
	 * runs the page load hyperlisp file
	 */
	public class PageLoadCore : ActiveController
	{
		/*
		 * runs the page load hyperlisp file
		 */
		[ActiveEvent(Name = "magix.viewport.page-load")]
		public void magix_viewport_page_load(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.viewport.page-load-dox].Value");
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

