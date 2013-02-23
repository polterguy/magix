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

			/// Defaulting if statement to return "false"
			if (ip.Parent != null)
				ip.Parent ["_state"].Value = false;

			string expr = ip.Value as string;
			if (string.IsNullOrEmpty (expr))
				throw new ArgumentException ("You cannot have an empty if statement");

			// Checking to see if single statement, meaning "exists"
			if (expr.IndexOfAny (new char[]{'=','>','<'}) != -1)
			{
				// Comparing two nodes with each other
				ExecuteWhile (expr, ip, dp);
			}
			else if (expr.IndexOf ("!") == 0)
			{
				// Checking to see of "not exists"
				while (!Expressions.ExpressionExist (expr.TrimStart ('!'), dp, ip))
				{
					Node tmp = new Node();
					tmp["_ip"].Value = ip;
					tmp["_dp"].Value = dp;

					RaiseEvent ("magix.execute", tmp);
				}
			}
			else
			{
				// Checking to see if "exists"
				while (Expressions.ExpressionExist (expr, dp, ip))
				{
					Node tmp = new Node();
					tmp["_ip"].Value = ip;
					tmp["_dp"].Value = dp;

					RaiseEvent ("magix.execute", tmp);
				}
			}
		}

		private static void ExecuteWhile(string expr, Node ip, Node dp)
		{
			while (true)
			{
				string left = expr.Substring (0, expr.IndexOfAny (new char[]{'!','=','>','<'}));;
				string comparison = expr.Substring (left.Length, 2);
				if (comparison[comparison.Length - 1] != '=')
					comparison = comparison.Substring (0, 1);
				string right = expr.Substring (expr.IndexOf (comparison) + comparison.Length);
				if (right.IndexOf ("\"") == 0)
				{
					right = right.Substring (1);
					right = right.Substring (0, right.Length - 1);
				}
				if (right.IndexOf("[") == 0)
				{
					right = Expressions.GetExpressionValue (right, dp, ip).ToString ();
				}
				bool isTrue = false;
				string valueOfExpr = left;
				if (valueOfExpr.IndexOf ("[") == 0)
				{
					valueOfExpr = Expressions.GetExpressionValue (valueOfExpr, dp, ip).ToString ();
				}
				switch (comparison)
				{
				case "==":
					isTrue = valueOfExpr == right;
					break;
				case "!=":
					isTrue = valueOfExpr != right;
					break;
				case ">":
					isTrue = valueOfExpr.CompareTo (right) == -1;
					break;
				case "<":
					isTrue = valueOfExpr.CompareTo (right) == 1;
					break;
				case ">=":
					isTrue = valueOfExpr.CompareTo (right) < 1;
					break;
				case "<=":
					isTrue = valueOfExpr.CompareTo (right) > -1;
					break;
				}

				// Signaling to else and else-if that statement was true
				if (ip.Parent != null)
					ip.Parent["_state"].Value = isTrue;

				if (isTrue)
				{
					Node node = new Node();
					node["_ip"].Value = ip;
					node["_dp"].Value = dp;
					RaiseEvent ("magix.execute", node);
				}
				else
					break;
			}
		}
	}
}

