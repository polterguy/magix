/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Threading;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Controller logic for the magix.execute programming language in magix, which
	 * facilitates for creating logic in nodes, such that you can execute logic
	 * based upon your nodes dynamically
	 */
	public class WhileCore : ActiveController
	{
		// TODO: Refactor together with "if"
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
		public static void magix_execute_while (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Creates a loop that executes the
underlaying code block repeatedly, as long as the
statement in the Value of while is True.";
				e.Params["Data"]["txt1"].Value = "Hello World 1.0";
				e.Params["Data"]["txt2"].Value = "Hello World 2.0";
				e.Params["Data"]["txt3"].Value = "Hello World 3.0";
				e.Params["while"].Value = "[Data].Count!=0";
				e.Params["while"]["set"].Value = "[while][magix.viewport.show-message][message].Value";
				e.Params["while"]["set"]["value"].Value = "[Data][0].Value";
				e.Params["while"].Add (new Node("set", "[Data][0]"));
				e.Params["while"]["magix.viewport.show-message"].Value = null;
				e.Params["while"]["magix.viewport.show-message"]["message"].Value = "Message...";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string expr = ip.Value as string;
			if (string.IsNullOrEmpty (expr))
				throw new ArgumentException ("You cannot have an empty while statement");

			while (Expressions.IsTrue(expr, ip, dp))
			{
				Node tmp = new Node("magix.execute.while");
				tmp["_ip"].Value = ip;
				tmp["_dp"].Value = dp;

				RaiseEvent ("magix.execute", tmp);
			}
		}
	}
}

