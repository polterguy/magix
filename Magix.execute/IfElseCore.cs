/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * if/else-if/else hyper lisp active events
	 */
	public class IfElseCore : ActiveController
	{
		/*
		 * if implementation
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public static void magix_execute_if(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"executes the child nodes as an execution
block, but only if the [if] statement returns true.&nbsp;&nbsp;
pair together with [else-if] and [else] to create
branching and control of flow of your program.&nbsp;&nbsp;if an if
statement returns true, then no paired [else-if] or [else]
statements will be executed.&nbsp;&nbsp;thread safe";
				e.Params["_data"]["item"].Value = "cache-object";
				e.Params["_data"]["cache"].Value = null;
				e.Params["if"].Value = "[_data][item].Value!=[_data][1].Name";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "they are not the same";
				return;
			}

			IfImplementation(
				e.Params, 
				"magix.execute.if");
		}

		/*
		 * else-if implementation
		 */
		[ActiveEvent(Name = "magix.execute.else-if")]
		public static void magix_execute_else_if(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"executes the underlaying code block,
but only if no previous [if] or [else-if] statement has returned true,
and the statement inside the value of the [else-if] 
returns true.&nbsp;&nbsp;thread safe";
				e.Params["_data"]["node"].Value = null;
				e.Params["if"].Value = "[_data][node].Value";
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "darn it";
				e.Params["else-if"].Value = "[_data][node]";
				e.Params["else-if"]["magix.viewport.show-message"]["message"].Value = "puuh";
				return;
			}
			
			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.else-if] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			// Checking to see if a previous "if" or "else-if" statement has returned true
			if (ip.Parent.Contains("_state_if") &&
			    ip.Parent["_state_if"].Get<bool>())
				return;

			IfImplementation(
				e.Params, 
				"magix.execute.else-if");
		}
		
		// Private helper, implementation for both "if" and "else-if" ...
		private static void IfImplementation (Node pars, string evt)
		{
			if (!pars.Contains("_ip") || !(pars["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [" + evt + "] directly, except for inspect purposes");

			Node ip = pars["_ip"].Value as Node;

			Node dp = ip;
			if (pars.Contains("_dp"))
				dp = pars["_dp"].Value as Node;

			string expr = ip.Value as string;

			if (string.IsNullOrEmpty(expr))
				throw new ArgumentException("You cannot have an empty [" + evt + "] statement");

			if (Expressions.IsTrue(expr, ip, dp))
			{
				// Making sure statement returns "true"
				ip.Parent["_state_if"].Value = true;

				RaiseActiveEvent(
					"magix._execute", 
					pars);
			}
			else
				ip.Parent["_state_if"].UnTie();
		}

		/*
		 * else implementation
		 */
		[ActiveEvent(Name = "magix.execute.else")]
		public static void magix_execute_else(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"executes the underlaying code block,
but only if no paired [if] or [else-if] statement
has returned true.&nbsp;&nbsp;thread safe";
				e.Params["if"].Value = "[if].Name==x_if";
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "ohh crap";
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "yup, still sane";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.else-if] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			// Checking to see if a previous "if" or "else-if" statement has returned true
			if (ip.Parent.Contains("_state_if") &&
			    ip.Parent["_state_if"].Get<bool>())
				return;


			RaiseActiveEvent(
				"magix._execute", 
				e.Params);
		}
	}
}

