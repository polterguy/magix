/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.execute
{
	/**
	 */
	[ActiveController]
	public class ExecuteCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.execute")]
		public void magix_execute (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["params"]["message"].Value = "Hi Thomas!";
				e.Params["inspect"].Value = @"Executes all the children nodes
expecting them to be callable keywords, directly embedded into
the ""magix.execute"" namespace, such that they become extensions
of the execution engine itself. Will internally keep a list
pointer to where the code/instruction-pointer is, and where the
data-pointer is, which is relevant for most other execution statements.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			ip["_state"].Value = null;

			foreach (Node idx in ip)
			{
				string nodeName = idx.Name;

				// Checking to see if it starts with a small letter
				if ("abcdefghijklmnopqrstuvwxyz".IndexOf (nodeName[0]) != -1)
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
			ip.Remove (ip["_state"]);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public void magix_execute_if (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["Item1"].Value = "Cache1";
				e.Params["Data"]["Cache"].Value = null;
				e.Params["if"].Value = "[Data][Item1].Value!=[Data][1].Name";
				e.Params["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["if"]["raise"] ["params"] ["message"].Value = "Message...";
				e.Params["inspect"].Value = @"Checks to see if the current 
statement is returning true, and if so, executes the underlaying nodes
as code through ""magix.execute"", expecting them to be keywords to
the execution engine. You can either compare a node expression
with another node expression, a node expression with a
constant value or a single node expression for existence of 
Node itself, Value or Name. Operators you can use are '!=', '==', '>=',
'<=', '>' and '<' when comparing two nodes or one node and 
a constant, and '!' in front of operator if only one 
expression is given to check for existence to negate the value.";
				return;
			}
			Node dp = e.Params ["_dp"].Value as Node;
			Node ip = e.Params ["_ip"].Value as Node;

			if (ip.Parent != null)
				ip.Parent [ip.Parent.Count - 1].Value = null;

			string expr = ip.Value as string;
			if (string.IsNullOrEmpty (expr))
				throw new ArgumentException ("You cannot have an empty if statement");

			// Checking to see if single statement, meaning "exists"
			if (expr.IndexOfAny (new char[]{'=','>','<'}) != -1)
			{
				ExecuteIf (expr, ip, dp, e.Params);
			}
			else if (expr.IndexOf ("!") == 0)
			{
				if (!Expressions.ExpressionExist (expr, ip, dp))
				{
					if (ip.Parent != null)
						ip.Parent[ip.Parent.Count - 1].Value = true;
					RaiseEvent ("magix.execute", e.Params);
				}
			}
			else
			{
				if (Expressions.ExpressionExist (expr, ip, dp))
				{
					if (ip.Parent != null)
						ip.Parent[ip.Parent.Count - 1].Value = true;
					RaiseEvent ("magix.execute", e.Params);
				}
			}
		}

		private void ExecuteIf(string expr, Node ip, Node dp, Node parms)
		{
			string left = expr.Substring (0, expr.IndexOfAny (new char[]{'=','>','<'}));;
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
			if (ip.Parent != null)
				ip.Parent[ip.Parent.Count - 1].Value = isTrue;
			if (isTrue)
			{
				RaiseEvent ("magix.execute", parms);
			}
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.else-if")]
		public void magix_execute_else_if (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["Node"].Value = null;
				e.Params["else-if"].Value = "[Data][Node]";
				e.Params["else-if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["else-if"]["raise"]["params"]["message"].Value = "Message...";
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
expression is given to check for existence to negate the value.";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			if (ip.Parent != null &&
			    ip.Parent[ip.Parent.Count - 1] != null &&
			    ip.Parent[ip.Parent.Count - 1].Get<bool>())
				return;

			string expr = ip.Value as string;
			ExecuteIf (expr, ip, dp, e.Params);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.else")]
		public void magix_execute_else (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["else"]["raise"].Value = "magix.viewport.show-message";
				e.Params["else"]["raise"]["params"]["message"].Value = "Message...";
				e.Params["inspect"].Value = @"If no previous ""if"" statement,
or ""else-if"" statement has returned true, execute the underlaying nodes
as code through ""magix.execute"", expecting them to be keywords to
the execution engine.";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;

			if (ip.Parent != null &&
			    ip.Parent[ip.Parent.Count - 1] != null &&
			    ip.Parent[ip.Parent.Count - 1].Get<bool>())
				return;

			if (ip.Parent != null)
				ip.Parent[ip.Parent.Count - 1].Value = null;

			RaiseEvent ("magix.execute", e.Params);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.raise")]
		public void magix_execute_raise (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["OR"]["Data"]["message"].Value = "Message from context";
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["params"]["message"].Value = "Either directly embedded 'params'...";
				e.Params["raise"]["context"].Value = "[OR][Data]";
				e.Params["inspect"].Value = @"Will raise the current node's Value
as an active event, passing in either the ""context"" Node 
Expression as the parameters to the active event, or the 
""params"" child collection. Functions as a ""magix.execute""
keyword.";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;
			if (ip.Contains ("context"))
			{
				dp = Expressions.GetExpressionValue (ip["context"].Get<string>(), dp, ip) as Node;
			}
			else if (ip.Contains ("params"))
			{
				dp = ip["params"];
			}
			else
			{
				dp = ip;
			}
			RaiseEvent (ip.Get<string>(), dp);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.for-each")]
		public void magix_execute_for_each (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["Items"]["Item1"]["message"].Value = "Howdy World!";
				e.Params["Data"]["Items"]["Item2"]["message"].Value = "Howdy World!";
				e.Params["for-each"].Value = "[Data][Items]";
				e.Params["for-each"]["raise"].Value = "magix.viewport.show-message";
				e.Params["for-each"]["raise"]["context"].Value = "[.]";
				e.Params["inspect"].Value = @"Will loop through all the given 
node's Value Expression, and execute the underlaying code, once for 
all nodes in the returned expression, with the Data-Pointer pointing
to the index node currently being looped through. Is a ""magix.execute""
keyword.";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

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
		 */
		[ActiveEvent(Name = "magix.execute.set")]
		public void magix_execute_set (object sender, ActiveEventArgs e)
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
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();

			if (ip.Contains ("value"))
			{
				string right = ip["value"].Get<string>();
				if (right == "null")
				{
					Expressions.Empty (left, dp, ip);
				}
				else if (!string.IsNullOrEmpty (right))
				{
					Expressions.SetNodeValue (left, right, dp, ip);
				}
				else
				{
					Expressions.SetNodeValue(left, ip["value"]);
				}
			}
			else
				throw new ArgumentException("No value passed into magix.execute.set");
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.remove")]
		public void magix_execute_remove (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["NodeToRemove"].Value = "Will be removed ...";
				e.Params["remove"].Value = "[Data][NodeToRemove]";
				e.Params["inspect"].Value = @"Removes the node pointed to
in the Value of the ""remove"" node, which is expected to be a Node Expression,
returning a node list. Functions as a ""margix.executor"" keyword.";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			string path = ip.Get<string>();

			Expressions.Remove (path, dp, ip);
		}

		[ActiveEvent(Name = "magix.core._transform-node-2-code")]
		public void Magix_Samples__TransformNodeToCode (object sender, ActiveEventArgs e)
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

		private string ParseNodes (int indent, Node node)
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

		[ActiveEvent(Name = "magix.core._transform-code-2-node")]
		public void magix_samples__transform_code_2_node (object sender, ActiveEventArgs e)
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

