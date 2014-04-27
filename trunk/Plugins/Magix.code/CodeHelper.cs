/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Magix.Core;

namespace Magix.code
{
	/**
	 * Helps you to transform Nodes back and forth between code syntax, which
	 * is being used in the Active Event Executor
	 */
	public class CodeHelper : ActiveController
	{
		/**
		 * transforms from code to node
		 */
		[ActiveEvent(Name = "magix.code.node-2-code")]
		public static void magix_code_node_2_code(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.code.node-2-code"].Value = null;
				e.Params["inspect"].Value = @"will transform the [json] node to 
code syntax, and return in [code].&nbsp;&nbsp;code returned will be the 
textual representation of the original tree hierarchy, such that 
two spaces ' ' opens up the child collection.&nbsp;&nbsp;=> separates 
name and value of node, name first.&nbsp;&nbsp;code returned might also
contain type information for types of int, decimal, datetime and bool.&nbsp;&nbsp;thread safe";
				e.Params["json"]["something"].Value =  "something-else";
				return;
			}

            if (!Ip(e.Params).Contains("json"))
			{
				throw new ArgumentException("no [json] passed into node-2-code");
			}
			Node node = null;
            if (Ip(e.Params)["json"].Value != null)
                node = Ip(e.Params)["json"].Value as Node;
			else
                node = Ip(e.Params)["json"].Clone();
			string txt = ParseNodes(0, node);
            Ip(e.Params)["code"].Value = txt;
		}

		private static string ParseNodes(int indent, Node node)
		{
			string retVal = "";
			foreach (Node idx in node)
			{
				for (int idxNo = 0; idxNo < indent * 2; idxNo ++)
				{
					retVal += " ";
				}
				string value = "";
				if (idx.Value != null)
				{
					if (idx.Value.GetType () != typeof(string))
					{
						switch (idx.Value.GetType ().FullName)
						{
						case "System.Int32":
							value += "=(int)>" + idx.Get<string>();
							break;
						case "System.Boolean":
							value += "=(bool)>" + idx.Get<string>();
							break;
						case "System.Decimal":
							value += "=(dec)>" + idx.Get<decimal>().ToString(CultureInfo.InvariantCulture);
							break;
						case "System.DateTime":
							value += "=(date)>" + idx.Get<DateTime>().ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
							break;
						case "Magix.Core.Node":
						{
							string nodeS = "";
							if (!string.IsNullOrEmpty(idx.Get<Node>().Name))
								nodeS += idx.Get<Node>().Name;
							if (idx.Get<Node>().Value != null)
								nodeS += "=>" + idx.Get<Node>().Get<string>();
							value += @"=(node)>@""" + nodeS + ParseNodes(string.IsNullOrEmpty(nodeS) ? 0 : 1, idx.Get<Node>()).Trim() + @"""";
						} break;
						}
					}
					else
					{
						if (idx.Get<string>().Contains("\n") || 
						    idx.Get<string>().StartsWith("\"") ||
						    idx.Get<string>().StartsWith(" "))
						{
							string nValue = idx.Get<string>();
							nValue = nValue.Replace ("\"", "\"\"");
							value += "=>" + "@\"" + nValue + "\"";
						}
						else
							value += "=>" + idx.Get<string>("").Replace ("\r\n", "\\n").Replace ("\n", "\\n");
					}
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
		 * transforms from code to node
		 */
		[ActiveEvent(Name = "magix.code.code-2-node")]
		public static void magix_code_code_2_node(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.code.code-2-node"].Value = null;
				e.Params["inspect"].Value = @"will transform the [code] node to 
a node tree.&nbsp;&nbsp;the code will be returned in [json]
as node structure, according to indentation.&nbsp;&nbsp;two spaces open up child 
collection, => assings to value, and first parts are name of node.&nbsp;&nbsp;
also supports =(int)>, =(datetime)>, =(decimal)> and =(bool)> to assign 
specific type to value.&nbsp;&nbsp;thread safe";
				e.Params["code"].Value =  @"
code
  goes
    here";
				return;
			}

            if (!Ip(e.Params).Contains("code"))
				throw new ArgumentException("No code node passed into _transform-code-2-node");

            string txt = Ip(e.Params)["code"].Get<string>();
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

					if (line.Trim() == "")
						continue; // Skipping white lines
					if (line.Trim().StartsWith("//"))
						continue;

					// Skipping "white lines"
					if (line.Trim().Length == 0)
						continue;

					// Skipping "commenting lines"
					if (line.Trim().IndexOf("//") == 0)
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
						throw new ArgumentException("Only even number of indents allowed in json code syntax");
					currentIndents = currentIndents / 2; // Number of nodes inwards/outwards

					string name = "";
					object value = null;

					string tmp = line.TrimStart();
					if (!tmp.Contains("="))
					{
						name = tmp;
					}
					else
					{
                        if (tmp[0] == '=')
                            name = ""; // empty name ...
                        else
    						name = tmp.Split(new string[]{"="}, StringSplitOptions.RemoveEmptyEntries)[0];
						switch (tmp.Substring(name.Length).Split('>')[0] + ">")
						{
						case "=>":
							if (!tmp.Substring(name.Length + 2).TrimStart().StartsWith("@"))
							{
								value = tmp.Substring(name.Length + 2).TrimStart();
							}
							else
							{
								string tmpLine = tmp.Substring(name.Length + 2).TrimStart();
								tmpLine = tmpLine.Substring(2);
								while (true)
								{
									int noFnut = 0;
									for (int idxNo = tmpLine.Length - 1; idxNo >= 0; idxNo--)
									{
										if (tmpLine[idxNo] == '"')
											noFnut += 1;
										else
											break;
									}

									value += tmpLine.Replace("\"\"", "\"");

									if (noFnut % 2 != 0)
									{
										value = ((string)value).TrimEnd().Substring(0, ((string)value).Length - 1);
										break;
									}

									value += "\n";
									tmpLine = reader.ReadLine();

									if (tmpLine == null)
										throw new ArgumentException("Unfinished string literal: " + value);
								}
							} break;
						case "=(int)>":
							value = int.Parse (tmp.Substring (name.Length + 7).Trim());
							break;
						case "=(date)>":
							value = DateTime.ParseExact(tmp.Substring (name.Length + 8).Trim(), "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
							break;
						case "=(bool)>":
							value = bool.Parse(tmp.Substring (name.Length + 8).Trim());
							break;
						case "=(dec)>":
							value = decimal.Parse(tmp.Substring (name.Length + 7).Trim(), CultureInfo.InvariantCulture);
							break;
                        case "=(node)>":
							string tmpLine2 = tmp.Substring(name.Length + 8).TrimStart();
							tmpLine2 = tmpLine2.Substring(2);
							while (true)
							{
								int noFnut = 0;
								for (int idxNo = tmpLine2.Length - 1; idxNo >= 0; idxNo--)
								{
									if (tmpLine2[idxNo] == '"')
										noFnut += 1;
									else
										break;
								}

								value += tmpLine2.Replace("\"\"", "\"");

								if (noFnut % 2 != 0)
								{
									value = ((string)value).TrimEnd().Substring(0, ((string)value).Length - 1);
									break;
								}

								value += "\n";
								tmpLine2 = reader.ReadLine();

								if (tmpLine2 == null)
									throw new ArgumentException("Unfinished string literal: " + value);
							}

                            Node tmpNode = new Node();
                            tmpNode["code"].Value = value.ToString().Trim();
                            RaiseActiveEvent(
                                "magix.code.code-2-node",
                                tmpNode);
                            value = tmpNode["json"].Value;
                            break;
						}
					}

					if (currentIndents == indents)
					{
						Node xNode = new Node(name, value);
						idxNode.Add(xNode);
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
            Ip(e.Params)["json"].Value = ret;
		}

		/**
		 * transforms from file to node
		 */
		[ActiveEvent(Name = "magix.code.file-2-node")]
		public static void magix_code_file_2_node(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.code.file-2-code"].Value = "some-path/to-some/hyper-lisp/file.hl";
				e.Params["inspect"].Value = @"will transform the given file as value of node to 
node tree, and return in [json].&nbsp;&nbsp;thread safe";
				return;
			}

            if (string.IsNullOrEmpty(Ip(e.Params).Get<string>()))
			{
				throw new ArgumentException("no file passed into file-2-code");
			}

            string file = Ip(e.Params).Get<string>();

			Node fn = new Node();
			fn.Value = file;

			RaiseActiveEvent(
				"magix.file.load",
				fn);

			Node code = new Node();
			code["code"].Value = fn["value"].Get<string>();

			RaiseActiveEvent(
				"magix.code.code-2-node",
				code);

            Ip(e.Params)["json"].ReplaceChildren(code["json"].Get<Node>());
		}
	}
}














