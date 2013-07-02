/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Controller logic for the "while" logic
	 */
	public class WhileCore : ActiveController
	{
		/**
		 * Checks to see if the current 
		 * statement is returning true, and if so, executes the underlaying nodes
		 * as code through "magix.execute", repeatedly, expecting them to be keywords to
		 * the execution engine. You can either compare a node expression
		 * with another node expression, a node expression with a
		 * constant value or a single node expression for existence of 
		 * Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
		 * '<=', '>' and '<' when comparing two nodes or one node and 
		 * a constant, and '!' in front of operator if only one 
		 * expression is given to check for existence to negate the value.
		 * Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.while")]
		public static void magix_execute_while(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"creates a loop that executes the
underlaying code block repeatedly, as long as the
statement in the value of [while] is true.&nbsp;&nbsp;thread safe";
				e.Params["_data"]["txt1"].Value = "hello world 1.0";
				e.Params["_data"]["txt2"].Value = "hello world 2.0";
				e.Params["_data"]["txt3"].Value = "hello world 3.0";
				e.Params["while"].Value = "[_data].Count!=0";
				e.Params["while"]["set"].Value = "[while][magix.viewport.show-message][message].Value";
				e.Params["while"]["set"]["value"].Value = "[_data][0].Value";
				e.Params["while"].Add (new Node("set", "[_data][0]"));
				e.Params["while"]["magix.viewport.show-message"].Value = null;
				e.Params["while"]["magix.viewport.show-message"]["message"].Value = "to be changed";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.set] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string expr = ip.Value as string;
			if (string.IsNullOrEmpty(expr))
				throw new ArgumentException ("you cannot have an empty while statement");

			while (Expressions.IsTrue(expr, ip, dp))
			{
				RaiseActiveEvent(
					"magix._execute", 
					e.Params);
			}
		}
	}
}

