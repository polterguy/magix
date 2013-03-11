/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Contains logic for the "override" keyword
	 */
	public class OverrideCore : ActiveController
	{
		/**
		 * Sets given node to either the constant value
		 * of the Value of the child node called "value", a node expression found 
		 * in Value "value", or the children nodes of the "value" child, if the 
		 * left hand parts returns a node. Functions as a "magix.execute" keyword
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
override is removed";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			if (ip.Contains("value"))
				ActiveEvents.Instance.CreateEventMapping(ip.Get<string>(), ip["value"].Get<string>());
			else
				ActiveEvents.Instance.RemoveMapping(ip.Get<string>());
		}
	}
}

