/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	// TODO: is this class necessary?
	/*
	 * override hyper lisp keyword
	 */
	public class OverrideCore : ActiveController
	{
		/*
		 * override hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.override")]
		public static void magix_execute_set(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["override"].Value = "namespace.foo";
				e.Params["override"]["value"].Value = "namespace.bar";
				e.Params["inspect"].Value = @"overrides the given value active event 
with the [value] node's value event.&nbsp;&nbsp;if no [value] is given, existing 
override is removed.&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.for-each] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			if (ip.Contains("value"))
				ActiveEvents.Instance.CreateEventMapping(ip.Get<string>(), ip["value"].Get<string>());
			else
				ActiveEvents.Instance.RemoveMapping(ip.Get<string>());
		}
	}
}

