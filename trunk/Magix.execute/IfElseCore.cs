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
	 */
	public class IfElseCore : ActiveController
	{
		/**
		 * Checks to see if the current 
		 * statement is returning true, and if so, executes the underlaying nodes
		 * as code through "magix.execute", expecting them to be keywords to
		 * the execution engine. You can either compare a node expression
		 * with another node expression, a node expression with a
		 * constant value or a single node expression for existence of 
		 * Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
		 * '<=', '>' and '<' when comparing two nodes or one node and 
		 * a constant, and '!' in front of operator if only one 
		 * expression is given to check for existence to negate the value.
		 * Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public static void magix_execute_if (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the child nodes as an execution
block, but only if the ""if"" statement returns True.
Pair together with ""else-if"" and ""else"" to create
branching and control of flow of your program. If an ""if""
statement returns True, then no paired ""else-if"" or ""else""
statements will be executed.";
				e.Params["Data"]["Item1"].Value = "Cache1";
				e.Params["Data"]["Cache"].Value = null;
				e.Params["if"].Value = "[Data][Item1].Value!=[Data][1].Name";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "They are NOT the same!";
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
				ExecuteIf (expr, ip, dp);
			}
			else if (expr.IndexOf ("!") == 0)
			{
				// Checking to see of "not exists"
				if (!Expressions.ExpressionExist (expr.TrimStart ('!'), dp, ip))
				{
					// Making sure if statement returns "true"
					if (ip.Parent != null)
						ip.Parent["_state"].Value = true;

					Node tmp = new Node();
					tmp["_ip"].Value = ip;
					tmp["_dp"].Value = dp;

					RaiseEvent ("magix.execute", tmp);
				}
			}
			else
			{
				// Checking to see if "exists"
				if (Expressions.ExpressionExist (expr, dp, ip))
				{
					// Making sure if statement returns "true"
					if (ip.Parent != null)
						ip.Parent["_state"].Value = true;

					Node tmp = new Node();
					tmp["_ip"].Value = ip;
					tmp["_dp"].Value = dp;

					RaiseEvent ("magix.execute", tmp);
				}
			}
		}

		// TODO: Refactor together with ExecuteWhile
		private static void ExecuteIf(string expr, Node ip, Node dp)
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
		}

		// TODO: Refactor together with "if" ...
		/**
		 * If no previous "if" statement,
		 * or "else-if" statement has returned true, will check to see if the current 
		 * statement is returning true, and if so, executes the underlaying nodes
		 * as code through "magix.execute", expecting them to be keywords to
		 * the execution engine. You can either compare a node expression
		 * with another node expression, a node expression with a
		 * constant value or a single node expression for existence of 
		 * Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
		 * '<=', '>' and '<' when comparing two nodes or one node and 
		 * a constant, and '!' in front of operator if only one 
		 * expression is given to check for existence to negate the value.
		 * Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.else-if")]
		public static void magix_execute_else_if (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the underlaying code block,
but only if no previous ""if"" or ""else-if"" statement has returned True,
and the statement inside the Value of the ""else-if"" 
returns True.";
				e.Params["Data"]["Node"].Value = null;
				e.Params["if"].Value = "[Data][Node].Value";
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "Darn it...";
				e.Params["else-if"].Value = "[Data][Node]";
				e.Params["else-if"]["magix.viewport.show-message"]["message"].Value = "Works...";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			// Checking to see if a previous "if" or "else-if" statement has returned true
			if (ip.Parent != null &&
			    ip.Parent["_state"].Get<bool>())
				return;

			string expr = ip.Value as string;

			// Checking to see if single statement, meaning "exists"
			if (expr.IndexOfAny (new char[]{'=','>','<'}) != -1)
			{
				// Comparing two nodes with each other
				ExecuteIf (expr, ip, dp);
			}
			else if (expr.IndexOf ("!") == 0)
			{
				// Checking to see of "not exists"
				if (!Expressions.ExpressionExist (expr.TrimStart ('!'), dp, ip))
				{
					// Making sure if statement returns "true"
					if (ip.Parent != null)
						ip.Parent["_state"].Value = true;

					Node tmp = new Node();
					tmp["_ip"].Value = ip;
					tmp["_dp"].Value = dp;

					RaiseEvent ("magix.execute", tmp);
				}
			}
			else
			{
				// Checking to see if "exists"
				if (Expressions.ExpressionExist (expr, dp, ip))
				{
					// Making sure if statement returns "true"
					if (ip.Parent != null)
						ip.Parent["_state"].Value = true;

					Node tmp = new Node();
					tmp["_ip"].Value = ip;
					tmp["_dp"].Value = dp;

					RaiseEvent ("magix.execute", tmp);
				}
			}
		}

		/**
		 * If no previous "if" statement,
		 * or "else-if" statement has returned true, execute the underlaying nodes
		 * as code through "magix.execute", expecting them to be keywords to
		 * the execution engine. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.else")]
		public static void magix_execute_else (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the underlaying code block,
but only if no paired ""if"" or ""else-if"" statement
has returned True.";
				e.Params["if"].Value = "[if].Name==x_if";
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "Ohh crap ...";
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "Yup, I'm still sane ...";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			// Checking to see if a previous "if" or "else-if" statement has returned true
			if (ip.Parent != null &&
			    ip.Parent["_state"].Get<bool>())
				return;

			Node node = new Node();
			node["_ip"].Value = ip;
			node["_dp"].Value = dp;
			RaiseEvent ("magix.execute", node);
		}
	}
}

