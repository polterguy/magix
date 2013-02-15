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
		[ActiveEvent(Name = "magix.execute.execute")]
		public static void magix_execute (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") 
				&& e.Params["inspect"].Get<string> ("") == "") {
				e.Params["Data"]["Value"].Value = "thomas";
				e.Params["if"].Value = "[Data][Value].Value==thomas";
				e.Params["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["if"]["raise"]["message"].Value = "Hi Thomas!";
				e.Params["inspect"].Value = @"Executes all the children nodes starting with
a lower case alpha character,
expecting them to be callable keywords, directly embedded into
the ""magix.execute"" namespace, such that they become extensions
of the execution engine itself. Will internally keep a list
pointer to where the code/instruction-pointer is, and where the
data-pointer is, which is relevant for most other execution statements.
Will ignore all node with Name not starting with a lower case alpha character.
All child nodes with a name starting with a lower case letter,
will be raise as ""magix.execute"" keywords, meaning e.g. ""if""
will become the active event ""magix.executor.if"", meaning you
can extend the programming language in Magix itself by adding up 
new keywords. All active events in the ""magix.execute""
namespace, can be called directly as keywords in magix.execute.
And in fact, most active events in the magix.execute namespace, 
such as ""if"", cannot be called directly, but must be called
from within a ""magix.execute"" event code block. If you supply
a Value for the ""execute"" keyword, this will be expected to be 
an Expressio, returning a Node-list, which will become the 
Data-Pointer of your execution block.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			// Cheking to see if we've got a "context"
			if (ip.Name == "execute" && 
				!string.IsNullOrEmpty(ip.Get<string>()))
			{
				dp = Expressions.GetExpressionValue (ip.Get<string>(), dp, ip) as Node;
				if (dp == null)
					throw new ArgumentException("You can only supply a context pointing to an existing Node hierarchy");
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
				e.Params["Data"]["Item1"].Value = "Cache1";
				e.Params["Data"]["Cache"].Value = null;
				e.Params["if"].Value = "[Data][Item1].Value!=[Data][1].Name";
				e.Params["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["if"]["raise"]["message"].Value = "Message...";
				e.Params["inspect"].Value = @"Checks to see if the current 
statement is returning true, and if so, executes the underlaying nodes
as code through ""magix.execute"", expecting them to be keywords to
the execution engine. You can either compare a node expression
with another node expression, a node expression with a
constant value or a single node expression for existence of 
Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
'<=', '>' and '<' when comparing two nodes or one node and 
a constant, and '!' in front of operator if only one 
expression is given to check for existence to negate the value.
Functions as a ""magix.execute"" keyword.";
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
				if (!Expressions.ExpressionExist (expr.TrimStart ('!'), ip, dp))
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
				if (Expressions.ExpressionExist (expr, ip, dp))
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
				e.Params["Data"]["Node"].Value = null;
				e.Params["else-if"].Value = "[Data][Node]";
				e.Params["else-if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["else-if"]["raise"]["message"].Value = "Message...";
				e.Params["inspect"].Value = @"If no previous ""if"" statement,
or ""else-if"" statement has returned true, will check to see if the current 
statement is returning true, and if so, executes the underlaying nodes
as code through ""magix.execute"", expecting them to be keywords to
the execution engine. You can either compare a node expression
with another node expression, a node expression with a
constant value or a single node expression for existence of 
Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
'<=', '>' and '<' when comparing two nodes or one node and 
a constant, and '!' in front of operator if only one 
expression is given to check for existence to negate the value.
Functions as a ""magix.execute"" keyword.";
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
			ExecuteIf (expr, ip, dp);
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
				e.Params["else"]["raise"].Value = "magix.viewport.show-message";
				e.Params["else"]["raise"]["message"].Value = "Message...";
				e.Params["inspect"].Value = @"If no previous ""if"" statement,
or ""else-if"" statement has returned true, execute the underlaying nodes
as code through ""magix.execute"", expecting them to be keywords to
the execution engine. Functions as a ""magix.execute"" keyword.";
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
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["message"].Value = "Hi there World...!!";
				e.Params["raise"]["no-override"].Value = "False";
				e.Params["inspect"].Value = @"Will raise the current node's Value
as an active event, passing in the 
current Node. Functions as a ""magix.execute""
keyword. If ""no-override"" is true, then it won't check
to see if the method is mapped through overriding. Which
is useful for having 'calling base functionality', among 
other things. For instance, if you've overridden 'foo'
to raise 'bar', then inside of 'bar' you can raise 'foo'
again, but this time with ""no-override"" set to False,
which will then NOT call 'bar', which would if occurred,
create a never ending loop. This makes it possible for
you to have overridden active events call their overridden
event, such that you can chain together active events, in
a hierarchy of overridden events.";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			Node pars = ip;

			bool forceNoOverride = false;
			if (ip.Contains ("no-override") && ip["no-override"].Get<bool>())
				forceNoOverride = true;
			if (ip.Name == "raise")
				RaiseEvent (ip.Get<string>(), pars, forceNoOverride);
			else
				RaiseEvent (ip.Name, pars, forceNoOverride);
		}

		/**
		 * Spawns a new thread which the given
		 * code block will be executed within. Useful for long operations, 
		 * where you'd like to return to caller before the operation is
		 * finished
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		public static void magix_execute_fork (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["Value"].Value = "thomas";
				e.Params["if"].Value = "[Data][Value].Value==thomas";
				e.Params["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["if"]["raise"]["message"].Value = "Hi Thomas!";
				e.Params["inspect"].Value = @"Spawns a new thread which the given
code block will be executed within. Useful for long operations, 
where you'd like to return to caller before the operation is
finished.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			Node pars = new Node();
			pars["_ip"].Value = ip.Clone ();
			pars["_dp"].Value = dp.Clone ();

			Thread thread = new Thread(ExecuteThread);
			thread.Start (pars);
		}

		private static void ExecuteThread (object pars)
		{
			Node par = pars as Node;
			RaiseEvent (
				"magix.execute",
				par);
		}

		/**
		 * Spawns a new thread which the given
		 * code block will be executed within. Useful for long operations, 
		 * where you'd like to return to caller before the operation is
		 * finished
		 */
		[ActiveEvent(Name = "magix.execute.sleep")]
		public static void magix_execute_sleep (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["sleep"].Value = 500;
				e.Params["inspect"].Value = @"Sleeps the current threat
for Value number of milliseconds.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Thread.Sleep (ip.Get<int>());
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
				e.Params["Data"]["Items"]["Item1"]["message"].Value = "Howdy World!";
				e.Params["Data"]["Items"]["Item2"]["message"].Value = "Howdy World!";
				e.Params["for-each"].Value = "[Data][Items]";
				e.Params["for-each"]["raise"].Value = "magix.viewport.show-message";
				e.Params["inspect"].Value = @"Will loop through all the given 
node's Value Expression, and execute the underlaying code, once for 
all nodes in the returned expression, with the Data-Pointer pointing
to the index node currently being looped through. Is a ""magix.execute""
keyword. Functions as a ""magix.execute"" keyword.";
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
				e.Params["Data"]["Children"].Value = "old value";
				e.Params["set"].Value = "[Data][Children].Value";
				e.Params["set"]["value"].Value = "new value";
				e.Params["inspect"].Value = @"Sets given node to either the constant value
of the Value of the child node called ""value"", a node expression found 
in Value ""value"", or the children nodes of the ""value"" child, if the 
left hand parts returns a node. Functions as a ""magix.execute"" keyword.";
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
					Expressions.Empty (left, dp, ip);
				}
				else
				{
					Expressions.SetNodeValue (left, right, dp, ip);
				}
			}
			else
			{
				Expressions.Empty (left, dp, ip);
			}
		}

		/**
		 * Removes the node pointed to
		 * in the Value of the "remove" node, which is expected to be a Node Expression,
		 * returning a node list. Functions as a "magix.executor" keyword
		 */
		[ActiveEvent(Name = "magix.execute.remove")]
		public static void magix_execute_remove (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["NodeToRemove"].Value = "Will be removed ...";
				e.Params["remove"].Value = "[Data][NodeToRemove]";
				e.Params["inspect"].Value = @"Removes the node pointed to
in the Value of the ""remove"" node, which is expected to be a Node Expression,
returning a node list. Functions as a ""magix.executor"" keyword.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string path = ip.Get<string>();

			Expressions.Remove (path, dp, ip);
		}

		/**
		 * Transforms a given "JSON" node into 'code syntax'
		 */
		[ActiveEvent(Name = "magix.core._transform-node-2-code")]
		public static void Magix_Samples__TransformNodeToCode (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("JSON"))
			{
				throw new ArgumentException("No node JSON passed into transform-node-2-code");
			}
			string txt = "";
			Node node = e.Params["JSON"].Value as Node;
			int startIdx = 0;
			if (!string.IsNullOrEmpty (node.Name))
			{
				txt += node.Name;
				if (node.Value != null)
				{
					if (node.Get<string>("") != "")
					{
						if (node.Get<string>().Contains ("\n") || 
						    node.Get<string>().StartsWith ("\"") ||
						    node.Get<string>().StartsWith (" "))
						{
							string nValue = node.Get<string>();
							txt += "=>" + "@\"" + nValue + "\"";
						}
						else
							txt += "=>" + node.Get<string>("");
					}
				}
				startIdx += 1;
				txt += "\r\n";
			}
			txt += ParseNodes(startIdx, node).TrimEnd ();
			e.Params["code"].Value = txt;
		}

		private static string ParseNodes (int indent, Node node)
		{
			string retVal = "";
			foreach (Node idx in node)
			{
				for (int idxNo = 0; idxNo < indent * 2; idxNo ++)
				{
					retVal += " ";
				}
				string value = "";
				if (idx.Get<string>("") != "")
				{
					if (idx.Get<string>().Contains ("\n") || 
					    idx.Get<string>().StartsWith ("\"") ||
					    idx.Get<string>().StartsWith (" "))
					{
						string nValue = idx.Get<string>();
						nValue = nValue.Replace ("\"", "\"\"");
						value += "=>" + "@\"" + nValue + "\"";
					}
					else
						value += "=>" + idx.Get<string>("").Replace ("\r\n", "\\n").Replace ("\n", "\\n");
				}
				retVal += idx.Name + value;
				retVal += "\n";
				if (idx.Count > 0)
				{
					retVal += ParseNodes (indent + 1, idx);
				}
			}
			return retVal;
		}

		/**
		 * Transforms the given "code" node into a node structure, according to
		 * spaces which indents the code
		 */
		[ActiveEvent(Name = "magix.core._transform-code-2-node")]
		public static void magix_samples__transform_code_2_node (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("code"))
			{
				throw new ArgumentException("No code node passed into _transform-code-2-node");
			}
			string txt = e.Params["code"].Get<string>();
			Node ret = new Node();
			using (TextReader reader = new StringReader(txt))
			{
				int indents = 0;
				Node idxNode = ret;
				while (true)
				{
					string line = reader.ReadLine ();
					if (line == null)
						break;

					if (line.Trim () == "")
						continue; // Skipping white lines
					if (line.Trim ().StartsWith ("//"))
						continue;

					// Skipping "white lines"
					if (line.Trim ().Length == 0)
						continue;

					// Skipping "commenting lines"
					if (line.Trim ().IndexOf ("//") == 0)
						continue;

					// Counting indents
					int currentIndents = 0;
					foreach (char idx in line)
					{
						if (idx != ' ')
							break;
						currentIndents += 1;
					}
					if (currentIndents % 2 != 0)
						throw new ArgumentException("Only even number of indents allowed in JSON code syntax");
					currentIndents = currentIndents / 2; // Number of nodes inwards/outwards

					string name = "";
					string value = null;

					string tmp = line.TrimStart ();
					if (!tmp.Contains ("=>"))
					{
						name = tmp;
					}
					else
					{
						name = tmp.Split (new string[]{"=>"}, StringSplitOptions.RemoveEmptyEntries)[0];
						value = tmp.Substring (name.Length + 2).TrimStart ();
						if (value.StartsWith ("@"))
						{
							value += "\r\n";
							while (true)
							{
								int noFnut = 0;
								for (int idxNo = value.Length - 3; idxNo >=0; idxNo-- )
								{
									if (value[idxNo] == '"')
										noFnut += 1;
									else
										break;
								}
								if (noFnut % 2 != 0)
									break;
								string tmpLine = reader.ReadLine ();
								if (tmpLine == null)
									throw new ArgumentException("Unfinished string literal: " + value);
								value += tmpLine.Replace ("\"\"", "\"") + "\r\n";
							}
							value = value.Substring (2, value.Length - 5);
						}
					}

					if (currentIndents == indents)
					{
						Node xNode = new Node(name, value);
						idxNode.Add (xNode);
					}

					// Decreasing, upwards in hierarchy...
					if (currentIndents < indents)
					{
						while (currentIndents < indents)
						{
							idxNode = idxNode.Parent;
							indents -= 1;
						}
						idxNode.Add (new Node(name, value));
					}

					if (currentIndents != indents && currentIndents > indents && currentIndents - indents > 1)
						throw new ArgumentException("Multiple indentations, without specifying child node name");

					// Increasing, downwards in hierarchy...
					if (currentIndents > indents)
					{
						idxNode = idxNode[idxNode.Count - 1];
						idxNode.Add (new Node(name, value));
						indents += 1;
					}
				}
			}
			e.Params["JSON"].Value = ret;
		}
	}
}

