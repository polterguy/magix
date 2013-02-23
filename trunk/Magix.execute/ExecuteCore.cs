/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
	public class ExecuteCore : ActiveController
	{
		/**
		 * Executes all the children nodes starting with
		 * a lower case alpha character,
		 * expecting them to be callable keywords, directly embedded into
		 * the "magix.execute" namespace, such that they become extensions
		 * of the execution engine itself. Will internally keep a list
		 * pointer to where the code/instruction-pointer is, and where the
		 * data-pointer is, which is relevant for most other execution statements.
		 * Will ignore all node with Name not starting with a lower case alpha character.
		 * All child nodes with a name starting with a lower case letter,
		 * will be raise as "magix.execute" keywords, meaning e.g. "if"
		 * will become the active event "magix.executor.if", meaning you
		 * can extend the programming language in Magix itself by adding up 
		 * new keywords. All active events in the "magix.execute"
		 * namespace, can be called directly as keywords in magix.execute.
		 * And in fact, most active events in the magix.execute namespace, 
		 * such as "if", cannot be called directly, but must be called
		 * from within a "magix.execute" event code block
		 */
		[ActiveEvent(Name = "magix.execute")]
		public static void magix_execute (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") &&
			    e.Params["inspect"].Get<string>("") == "")
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the children nodes as if they
were a block of execute statements, or code, within 
the ""magix.execute"" namespace. All magix active 
events which starts with the ""magix.execute."" 
namespace, can be embedded inside a ""magix.execute""
scope as keywords, e.g. the ""if"" keyword, will
map towards the ""magix.execute.if"" active event.
This means that you can use all ""magix.execute""
active events as code instructions, which you can
execute by raising this active event, by using the
short version ""if"" instead of ""magix.execute.if"".
If you supply a Value for your ""execute"" node, the
event will be executed, with the code returned from
the Node-List expression from your Value, meaning
the Value effectively becomes a 'goto keyword'.";
				e.Params["Data"]["Value"].Value = "thomas";
				e.Params["if"].Value = "[Data][Value].Value==thomas";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "Hi Thomas!";
				e.Params["else"]["magix.viewport.show-message"].Value = null;
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "Hi Stranger!";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			// Checking to see if we've got a "context"
			if ((ip.Name == "execute" || ip.Name == "magix.execute") && 
				!string.IsNullOrEmpty(ip.Get<string>()))
			{
				ip = Expressions.GetExpressionValue (ip.Get<string>(), dp, ip) as Node;
				if (ip == null)
					throw new ArgumentException(
						"You can only supply a context pointing to an existing Node hierarchy");
			}

			ip["_state"].Value = null;

			foreach (Node idx in ip)
			{
				string nodeName = idx.Name;

				// Checking to see if it starts with a small letter
				if ("abcdefghijklmnopqrstuvwxyz".IndexOf (nodeName[0]) != -1)
				{
					// Checking to see if it's a reference to an active event
					if (nodeName.Contains ("."))
					{
						Node tmp = new Node();

						// "Instruction" pointer, code Node
						tmp["_ip"].Value = idx;

						// Data Pointer, pointer to data
						tmp["_dp"].Value = dp;

						RaiseEvent ("magix.execute.raise", tmp);
					}
					else
					{
						// This is a keyword
						string eventName = "magix.execute." + nodeName;

						if (nodeName == "execute")
							eventName = "magix.execute";

						Node tmp = new Node();

						// "Instruction" pointer, code Node
						tmp["_ip"].Value = idx;

						// Data Pointer, pointer to data
						tmp["_dp"].Value = dp;

						RaiseEvent (eventName, tmp);
					}
				}
			}
			ip.Remove (ip["_state"]);
		}

		/**
		 * Creates a try block, with an associated code block, and a catch block,
		 * which will be invoked if an exception is thrown. The catch statement
		 * will only be invoked if an exception is thrown
		 */
		[ActiveEvent(Name = "magix.execute.try")]
		public static void magix_execute_try (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Use the ""try"" keyword to create
a block of code, which will enter your ""catch"" 
execution block of code, if an exception is thrown
inside the ""code"" block, inside the ""try""
block.";
				e.Params["try"].Value = null;
				e.Params["try"]["code"]["throw"].Value = "To Throw or Not to Throw!!";
				e.Params["try"]["code"]["magix.viewport.show-message"]["message"].Value = "NOT supposed to show!!";
				e.Params["try"]["catch"]["set"].Value = "[magix.viewport.show-message][message].Value";
				e.Params["try"]["catch"]["set"]["value"].Value = "[exception].Value";
				e.Params["try"]["catch"]["magix.viewport.show-message"].Value = null;
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			if (!ip.Contains ("code"))
				throw new ApplicationException("No code block inside of try statement");

			if (!ip.Contains ("catch"))
				throw new ApplicationException("No catch block inside of try statement");

			try
			{
				RaiseEvent (
					"magix.execute",
					ip["code"]);
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				if (ip["code"].Contains ("_state"))
					ip["code"]["_state"].UnTie ();

				ip["catch"]["exception"].Value = err.Message;
				RaiseEvent (
					"magix.execute",
					ip["catch"]);
			}
		}

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
branching and control of flow of your program.";
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
but only if no paired ""if"" statement has returned True,
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
		public static void magix_execute_raise (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Will raise the active event
given in the Value, with the parameters found as child 
nodes underneath the ""raise"" keyword itself. Note, 
if your active event contains '.', then you can
raise the active event directly, without using
the ""raise"" keyword, by creating a node who's
Name is the name of your active event. Add up
""no-override"" with a Value of True to make 
sure you go directly towards the active event 
you're dereferencing, meaning no overrides of 
the active events will be used.";
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["message"].Value = "Hi there World 1.0...!!";
				e.Params["raise"]["no-override"].Value = "False";
				e.Params["magix.viewport.show-message"].Value = null;
				e.Params["magix.viewport.show-message"]["message"].Value = "Hi there World 2.0...!!";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params ["_dp"].Value as Node;

			Node pars = ip;

			// Unless we're given at least one parameter underneath our Instruction Pointer,
			// we pass in the Data-Pointer by default to the Active Event ...
			// But only if event is being "raised" and not directly referenced ...
			if (ip.Count == 0 && ip.Name == "raise")
				pars = dp;

			bool forceNoOverride = false;
			if (ip.Contains ("no-override") && ip["no-override"].Get<bool>())
				forceNoOverride = true;
			if (ip.Name == "raise")
				RaiseEvent (ip.Get<string>(), pars, forceNoOverride);
			else
				RaiseEvent (ip.Name, pars, forceNoOverride);
		}

		/**
		 * Will loop through all the given 
		 * node's Value Expression, and execute the underlaying code, once for 
		 * all nodes in the returned expression, with the Data-Pointer pointing
		 * to the index node currently being looped through. Is a "magix.execute"
		 * keyword. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.for-each")]
		public static void magix_execute_for_each (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["Data"]["Items"]["Item1"]["message"].Value = "Howdy World 1.0!";
				e.Params["Data"]["Items"]["Item2"]["message"].Value = "Howdy World 2.0!";
				e.Params["for-each"].Value = "[Data][Items]";
				e.Params["for-each"]["set"].Value = "[/][for-each][magix.viewport.show-message][message].Value";
				e.Params["for-each"]["set"]["value"].Value = "[.][message].Value";
				e.Params["for-each"]["magix.viewport.show-message"].Value = null;
				e.Params["inspect"].Value = @"Will loop through all the Nodes
in the given Node-List expression, setting the 
Data-Pointer to the currently processed item.
This means that your code will execute once
for every single Node you have in your return
expression. Use the [.] expression to de-reference
the current Node.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			if (Expressions.ExpressionExist (ip.Get<string>(), dp, ip))
			{
				Node tmp = Expressions.GetExpressionValue (ip.Get<string>(), dp, ip) as Node;
				foreach (Node idx in tmp)
				{
					Node tmp2 = new Node();
					tmp2["_ip"].Value = ip;
					tmp2["_dp"].Value = idx;
					RaiseEvent ("magix.execute", tmp2);
				}
			}
		}

		/**
		 * Sets given node to either the constant value
		 * of the Value of the child node called "value", a node expression found 
		 * in Value "value", or the children nodes of the "value" child, if the 
		 * left hand parts returns a node. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.set")]
		public static void magix_execute_set (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["Data"]["Children"].Value = "old value";
				e.Params["set"].Value = "[Data][Children].Value";
				e.Params["set"]["value"].Value = "new value";
				e.Params["inspect"].Value = @"Sets the given expression in the Value
of ""set"" to the Value of expression, or constant, in ""value"" node 
underneath ""set"". If you pass in no ""value"", the 
expression in Value will be nullified. Meaning, if
it's a Node-List it will be emptied and the Node
removed. If it's a Value, the value will become
null. The ""value""'s Value can be a constant,
the Value of ""set"" must be an expression.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();

			if (ip.Contains ("value"))
			{
				string right = ip["value"].Get<string>();
				if (right == null)
				{
					Expressions.Remove (left, dp, ip);
				}
				else
				{
					Expressions.SetNodeValue (left, right, dp, ip);
				}
			}
			else
			{
				Expressions.Remove (left, dp, ip);
			}
		}

		/**
		 * Adds the given "value" Node to the Value Expression, which must be a Node list
		 */
		[ActiveEvent(Name = "magix.execute.add")]
		public static void magix_execute_add (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Creates a copy of the Node returned by the 
expression in the ""value"" Node underneath 
the ""add"" node, and appends it into the 
Node-Expression found in Value of ""add"".";
				e.Params["Data"]["Children"].Value = "Original";
				e.Params["Data"]["Children1"].Value = "Will be copied";
				e.Params["Data"]["Children1"]["Item1"].Value = "Will be copied too";
				e.Params["Data"]["Children1"]["Item2"].Value = "Will also be copied";
				e.Params["add"].Value = "[Data][Children]";
				e.Params["add"]["value"].Value = "[Data][Children1]";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();

			if (!ip.Contains ("value"))
				throw new ArgumentException("Cannot add a null node, need value Node to declare which node to add");

			string right = ip["value"].Get<string>();
			Node leftNode = Expressions.GetExpressionValue (left, dp, ip) as Node;
			Node rightNode = Expressions.GetExpressionValue (right, dp, ip) as Node;

			if (leftNode == null || rightNode == null)
				throw new ArgumentException("Both Value and 'value' must return an existing Node-List");

			leftNode.Add (rightNode.Clone ());
		}

		/**
		 * Throws an exception with the Value descriptive message
		 */
		[ActiveEvent(Name = "magix.execute.throw")]
		public static void magix_execute_throw (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Throws an exception, which
will stop the entire current execution, and halt 
back to the previous catch in the stack of 
active events. Message thrown becomes the 
Value of the throw Node. Use together with
""try"" to handle errors.";
				e.Params["throw"].Value = "Some Exception Error Message";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			throw new ApplicationException(ip.Get<string>());
		}
	}
}

