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
	 * stop hyper lisp keyword
	 */
	public class StopCore : ActiveController
	{
		public class HyperLispStopException : Exception
		{
		}

		/**
		 * stop hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.stop")]
		public static void magix_execute_stop(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>stops the execution of the current level
of code nodes in the tree</p><p>affects [while], [for-each] and other code scopes, such 
as events and event handlers</p><p>thread safe</p>";
				ip["_data"].Value = 5;
				ip["while"].Value = "more-than-equals";
                ip["while"]["lhs"].Value = "[_data].Value";
                ip["while"]["rhs"].Value = "1";
                ip["while"]["code"]["using"].Value = "magix.math";
                ip["while"]["code"]["using"]["add"].Value = null;
                ip["while"]["code"]["using"]["add"][""].Value = "[_data].Value";
                ip["while"]["code"]["using"]["add"]["",1].Value = "-1";
                ip["while"]["code"]["set"].Value = "[_data].Value";
                ip["while"]["code"]["set"]["value"].Value = "[@][using][add].Value";
                ip["while"]["code"]["stop"].Value = null;
				return;
			}
			throw new HyperLispStopException();
		}
	}
}

