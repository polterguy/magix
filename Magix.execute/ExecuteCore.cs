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
			if (e.Params.Count == 0)
			{
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["params"]["message"].Value = "Hi Thomas!";
			}
			else
			{
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
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public void magix_execute_if (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip")) {
				e.Params.Name = "if";
				e.Params.Value = "[Data][Item1].Value=\"thomas\"";
				e.Params ["raise"].Value = "magix.viewport.show-message";
				e.Params ["raise"] ["params"] ["message"].Value = "Message...";
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
			string[] sections = expr.Split (' ');
			string left = sections[0];
			string comparison = sections[1];
			string right = expr.Substring (left.Length + 2 + comparison.Length);
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
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "else-if";
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["params"]["message"].Value = "Message...";
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
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "else";
				e.Params["raise"].Value = "magix.viewport.show-message";
				e.Params["raise"]["params"]["message"].Value = "Message...";
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
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "raise";
				e.Params.Value = "magix.viewport.show-message";
				e.Params["params"]["message"].Value = "Either directly embedded 'params'...";
				e.Params["context"].Value = "[./][OrSomeDataContainingArgs]";
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
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "for-each";
				e.Params.Value = "[Data][Items]";
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
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "set";
				e.Params.Value = "[Data][Children]=[DataBackup][Children]";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>().Split ('=')[0].TrimEnd ();
			string right = ip.Get<string>().Substring (ip.Get<string>().IndexOf ("=") + 1).TrimStart ();

			if (right.IndexOf ("\"") == 0)
			{
				right = right.Substring (1, right.Length - 2);
				right = right.Replace ("\\\"", "\"").Replace ("\\n", "\r\n").Replace ("\\\"", "\"");
				Expressions.SetNodeValue (left, right, dp, ip);
			}
			else if (right == "null")
			{
				Expressions.Empty (left, dp, ip);
			}
			else
			{
				Expressions.SetNodeValue (left, right, dp, ip);
			}
		}

		// TODO: Is this not necessary, does set do the same ...?
		/**
		 */
		[ActiveEvent(Name = "magix.execute.remove")]
		public void magix_execute_remove (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "remove";
				e.Params.Value = "[Data][NodeToRemove]";
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
				e.Params["JSON"].Value = "JSON Node passes by reference of value here ...";
				return;
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
				e.Params["code"].Value = @"if=>[Data].Value = thomas
  call=>Magix.Core.ShowMessage
    params
      message=>@""Howdy dudes!!
this is """"Reggie Boy"""" Talking!!""";
				return;
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

