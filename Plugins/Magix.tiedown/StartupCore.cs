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
	 * runs the startup hyperlisp file
	 */
	public sealed class StartupCore : ActiveController
	{
        private static string _lock = "";

        private static bool _hasRun = false;

		/*
		 * runs the startup hyperlisp file
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.core.application-startup-dox].value");
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

