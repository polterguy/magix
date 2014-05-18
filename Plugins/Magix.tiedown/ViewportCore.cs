/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Configuration;
using Magix.Core;

namespace Magix.tiedown
{
	/*
	 * loads the default viewport
	 */
	public class ViewportCore : ActiveController
	{
		/*
		 * loads the default viewport
		 */
		[ActiveEvent(Name = "magix.viewport.load-viewport")]
		public void magix_viewport_load_viewport(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"executes the load-viewport.hl hyper lisp file";
				return;
			}

			Node node = new Node();
            node["file"].Value = "plugin:magix.file.load-from-resource";
            node["file"]["assembly"].Value = "Magix.tiedown";
            node["file"]["resource-name"].Value = "Magix.tiedown.hyperlisp.load-viewport.hl";

			RaiseActiveEvent(
				"magix.execute.execute-script",
				node);

            Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(node["params"]["viewport"].Get<string>());
            e.Params["viewport"].Value = ctrl;
        }
	}
}

