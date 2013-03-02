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
	 * Contains logic for the "raise" keyword
	 */
	public class RaiseCore : ActiveController
	{
		/**
		 * Will raise the current node's Value
		 * as an active event, passing in the 
		 * parameters child collection. Functions as a "magix.execute"
		 * keyword. If "no-override" is true, then it won't check
		 * to see if the method is mapped through overriding. Which
		 * is useful for having 'calling base functionality', among 
		 * other things. For instance, if you've overridden 'foo'
		 * to raise 'bar', then inside of 'bar' you can raise 'foo'
		 * again, but this time with "no-override" set to False,
		 * which will then NOT call 'bar', which would if occurred,
		 * create a never ending loop. This makes it possible for
		 * you to have overridden active events call their overridden
		 * event, such that you can chain together active events, in
		 * a hierarchy of overridden events
		 */
		[ActiveEvent(Name = "magix.execute.raise")]
		public static void magix_execute_raise(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will raise the active event
given in value, with the parameters found as child 
nodes underneath the [raise] keyword itself.&nbsp;&nbsp;note, 
if your active event contains '.', then you can
raise the active event directly, without using
the raise keyword, by creating a node who's
name is the name of your active event.&nbsp;&nbsp;add up
[no-override] with a value of true if you wish
to go directly to the active event in question,
and not rely upon any overrides.&nbsp;&nbsp;this is useful
for having the possibility of calling base
functionality from overridden active events";
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["message"].Value = "Hi there World 1.0...!!";
				e.Params["raise"]["no-override"].Value = "False";
				e.Params["magix.viewport.show-message"].Value = null;
				e.Params["magix.viewport.show-message"]["message"].Value = "Hi there World 2.0...!!";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			bool forceNoOverride = false;
			if (ip.Contains("no-override") && ip["no-override"].Get<bool>())
				forceNoOverride = true;

			if (ip.Name == "raise")
				RaiseEvent(ip.Get<string>(), ip, forceNoOverride);
			else
				RaiseEvent(ip.Name, ip, forceNoOverride);
		}
	}
}

