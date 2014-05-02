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
	 * hyper lisp set keyword
	 */
	public class SetCore : ActiveController
	{
		/**
		 * hyepr lisp set keyword
		 */
		[ActiveEvent(Name = "magix.execute.set")]
		public static void magix_execute_set(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["_data"]["children"].Value = "old value";
				e.Params["set"].Value = "[_data][children].Value";
				e.Params["set"]["value"].Value = "new value";
				e.Params["inspect"].Value = @"<p>sets the given expression in the value
of [set] to the value of expression, or constant, in [value] node</p><p>if you pass in 
no [value], the expression in value will be nullified.&nbsp;&nbsp;meaning, if it's a 
node-list it will be emptied and the node removed.&nbsp;&nbsp;if it's a value, the value 
will become null.&nbsp;&nbsp;if it's a name, the name will be set to the empty string</p>
<p>the [value]'s value can be either a constant or an expression</p><p>the value of [set] 
must be an expression, unless it's a null value, at which point the result of the set will 
be put into the value of the [set] itself</p><p>you can set a value of a node to either a 
name, value of another node or a node-list.&nbsp;&nbsp;you can set a node-list to another 
node-list, at which case the entire node will be exchanged with a deep copy of the node 
from value.&nbsp;&nbsp;you can set the name of a node to a string, either through a constant 
or an expression</p><p>if you try to set a node that does not exist, the node will be created, 
and added to the tree</p><p>thread safe</p>";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [set] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = e.Params["_dp"].Value as Node;

			string destinationExpression = ip.Get<string>();
			string sourceExpression = null;

            if (ip.Contains("value"))
            {
                sourceExpression = ip["value"].Get<string>();
            }
			Expressions.SetNodeValue(destinationExpression, sourceExpression, dp, ip, ip.Contains("value"));
		}
	}
}

