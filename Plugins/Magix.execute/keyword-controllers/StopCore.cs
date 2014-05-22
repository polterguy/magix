/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * stop hyperlisp keyword
	 */
	public class StopCore : ActiveController
	{
		public class HyperLispStopException : Exception
		{
		}

		/**
		 * stop hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.stop")]
		public static void magix_execute_stop(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.stop-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.stop-sample]");
                return;
			}
			throw new HyperLispStopException();
		}
	}
}

