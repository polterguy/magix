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
	 * runs the startup hyperlisp file
	 */
	public class StartupCore : ActiveController
	{
        private static string _lock = "";

        private static bool _hasRun = false;

		/**
		 * runs the startup hyperlisp file
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"executes the startup.hl hyperlisp file";
				return;
			}

            if (!_hasRun)
            {
                lock (_lock)
                {
                    if (!_hasRun)
                    {
			            Node node = new Node();
                        node["file"].Value = "plugin:magix.file.load-from-resource";
                        node["file"]["assembly"].Value = "Magix.tiedown";
                        node["file"]["resource-name"].Value = "Magix.tiedown.hyperlisp.startup.hl";

                        RaiseActiveEvent(
                            "magix.execute.execute-script",
                            node);
                        _hasRun = true;
                    }
                }
            }
		}
	}
}

