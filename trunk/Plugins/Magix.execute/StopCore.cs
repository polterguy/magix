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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"stops the execution of the current level
of code nodes in the tree.&nbsp;&nbsp;affects [while], [for-each] and other code scopes, 
such as events and event handlers.&nbsp;&nbsp;thread safe";
				e.Params["_data"].Value = 5;
				e.Params["while"].Value = "more-than-equals";
                e.Params["while"]["lhs"].Value = "[_data].Value";
                e.Params["while"]["rhs"].Value = "1";
                e.Params["while"]["code"]["using"].Value = "magix.math";
                e.Params["while"]["code"]["using"]["add"].Value = null;
                e.Params["while"]["code"]["using"]["add"][""].Value = "[_data].Value";
                e.Params["while"]["code"]["using"]["add"]["",1].Value = "-1";
                e.Params["while"]["code"]["set"].Value = "[_data].Value";
                e.Params["while"]["code"]["set"]["value"].Value = "[@][using][add].Value";
                e.Params["while"]["code"]["stop"].Value = null;
				return;
			}

			throw new HyperLispStopException();
		}
	}
}

