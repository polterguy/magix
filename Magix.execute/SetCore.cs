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
	 * Contains logic for the "set" keyword
	 */
	public class SetCore : ActiveController
	{
		/**
		 * Sets given node to either the constant value
		 * of the Value of the child node called "value", a node expression found 
		 * in Value "value", or the children nodes of the "value" child, if the 
		 * left hand parts returns a node. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.set")]
		public static void magix_execute_set(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["_data"]["children"].Value = "old value";
				e.Params["set"].Value = "[_data][children].Value";
				e.Params["set"]["value"].Value = "new value";
				e.Params["inspect"].Value = @"sets the given expression in the value
of [set] to the value of expression, or constant, in [value] node.&nbsp;&nbsp;
if you pass in no [value], the 
expression in value will be nullified.&nbsp;&nbsp;if
it's a node-list it will be emptied and the node
removed.&nbsp;&nbsp;if it's a value, the value will become
null.&nbsp;&nbsp;the [value]'s value can be a constant,
the value of [set] must be an expression";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();
			string right = null;

			if (ip.Contains("value"))
				right = ip["value"].Get<string>();

			Expressions.SetNodeValue(left, right, dp, ip, ip.Contains("value"));
		}
	}
}

