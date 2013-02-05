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
	 * Level3: Controller that encapsulates execution engine
	 */
	[ActiveController]
	public class ExecuteCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "Magix.execute")]
		public void Magix_execute (object sender, ActiveEventArgs e)
		{
			if (e.Params.Count == 0)
			{
				e.Params["Data"].Value = "thomas";
				e.Params["if"].Value = "[Data].Value == \"thomas\"";
				e.Params["if"]["call"].Value = "Magix.Core.ShowMessage";
				e.Params["if"]["call"]["params"]["Message"].Value = "Hi Thomas!";
				e.Params["else"].Value = null;
				e.Params["else"]["call"].Value = "Magix.Core.ShowMessage";
				e.Params["else"]["call"]["params"]["Message"].Value = "Hi Stranger!";
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
						string eventName = "Magix.execute." + nodeName;

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
		[ActiveEvent(Name = "Magix.execute.if")]
		public void Magix_execute_if (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip")) {
				e.Params.Name = "if";
				e.Params.Value = "[Data][Item1].Value=\"thomas\"";
				e.Params ["call"].Value = "Magix.Core.ShowMessage";
				e.Params ["call"] ["params"] ["Message"].Value = "Message...";
				return;
			}
			Node dp = e.Params ["_dp"].Value as Node;
			Node ip = e.Params ["_ip"].Value as Node;

			if (ip.Parent != null)
				ip.Parent [ip.Parent.Count - 1].Value = null;

			string expr = ip.Value as string;
			if (string.IsNullOrEmpty (expr))
				throw new ArgumentException ("You cannot have an empty if statement");

			ExecuteIf (expr, ip, dp, e.Params);
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
				RaiseEvent ("Magix.execute", parms);
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.else-if")]
		public void Magix_execute_else_if (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "else-if";
				e.Params["call"].Value = "Magix.Core.ShowMessage";
				e.Params["call"]["params"]["Message"].Value = "Message...";
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
		[ActiveEvent(Name = "Magix.execute.else")]
		public void Magix_execute_else (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "else";
				e.Params["call"].Value = "Magix.Core.ShowMessage";
				e.Params["call"]["params"]["Message"].Value = "Message...";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;

			if (ip.Parent != null &&
			    ip.Parent[ip.Parent.Count - 1] != null &&
			    ip.Parent[ip.Parent.Count - 1].Get<bool>())
				return;

			if (ip.Parent != null)
				ip.Parent[ip.Parent.Count - 1].Value = null;

			RaiseEvent ("Magix.execute", e.Params);
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.call")]
		public void Magix_execute_call (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "call";
				e.Params.Value = "Magix.Core.ShowMessage";
				e.Params["params"]["Message"].Value = "Either directly embedded 'params'...";
				e.Params["context"].Value = "[./][OrSomeDataContainingArgs]";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;
			if (ip.Contains ("context"))
			{
				dp = Expressions.GetExpressionValue (ip["context"].Get<string>(), dp, ip) as Node;
			}
			else
			{
				dp = ip["params"];
			}
			RaiseEvent (ip.Get<string>(), dp);
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.for-each")]
		public void Magix_execute_for_each (object sender, ActiveEventArgs e)
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
					RaiseEvent ("Magix.execute", tmp2);
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.set")]
		public void Magix_execute_set (object sender, ActiveEventArgs e)
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
				right = right.Replace ("\\\"", "\"").Replace ("\\n", "\r\n");
			}
			Expressions.SetNodeValue (left, right, dp, ip);
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.remove")]
		public void Magix_execute_remove (object sender, ActiveEventArgs e)
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

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.empty")]
		public void Magix_execute_empty (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "empty";
				e.Params.Value = "[Data]";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			string path = ip.Get<string>();

			Expressions.Empty (path, dp, ip);
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.if-exist")]
		public void Magix_execute_if_exist (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("_ip"))
			{
				e.Params.Name = "if-exist";
				e.Params.Value = "[Data]";
				return;
			}
			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			if (Expressions.ExpressionExist (ip.Get<string>(), dp, ip))
			{
				foreach (Node idx in dp)
				{
					Node tmp2 = new Node();
					tmp2["_ip"].Value = ip;
					tmp2["_dp"].Value = dp;
					RaiseEvent ("Magix.execute", tmp2);
				}
			}
		}

		[ActiveEvent(Name = "Magix.Core._TransformNodeToCode")]
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
					txt += "=>" + node.Value.ToString ();
				startIdx += 1;
				txt += "\r\n";
			}
			txt += ParseNodes(startIdx, node).TrimEnd ();
			e.Params["Code"].Value = txt;
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
					value += "=>" + idx.Get<string>("").Replace ("\r\n", "\\r\\n").Replace ("\n", "\\n");
				retVal += idx.Name + value;
				retVal += "\r\n";
				if (idx.Count > 0)
				{
					retVal += ParseNodes (indent + 1, idx);
				}
			}
			return retVal;
		}

		[ActiveEvent(Name = "Magix.Core._TransformCodeToNode")]
		public void Magix_Samples__TransformCodeToNode (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Code"))
			{
				e.Params["Code"].Value = @"if=>[Data].Value = ""thomas""
  call=>Magix.Core.ShowMessage
    params
      Message=>Howdy dudes!!";
				return;
			}
			string txt = e.Params["Code"].Get<string>();
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
						value = tmp.Substring (name.Length + 2);
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

