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
	 * runs the startup hyper lisp file
	 */
	public class StartupCore : ActiveController
	{
        private static string _lock = "";

        private static bool _hasRun = false;

		/**
		 * runs the startup hyper lisp file
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"raised when application is initially started, 
checks the web.config setting named ""Magix.Core.AppStart-HyperLispFile"", and if set, 
expects it to be a pointer to a hyper lisp file, which will be executed";
				return;
			}

            if (!_hasRun)
            {
                lock (_lock)
                {
                    if (!_hasRun)
                    {
                        string defaultHyperLispFile = ConfigurationManager.AppSettings["Magix.Core.AppStart-HyperLispFile"];

                        if (!string.IsNullOrEmpty(defaultHyperLispFile))
                        {
                            Node node = new Node();
                            node.Value = defaultHyperLispFile;

                            RaiseActiveEvent(
                                "magix.execute.execute-file",
                                node);
                        }
                        _hasRun = true;
                    }
                }
            }
		}
	}
}

