/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Globalization;
using System.IO;
using System.Threading;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Helps you to transform Nodes back and forth between code syntax, which
	 * is being used in the Active Event Executor
	 */
	public class CodeHelper : ActiveController
	{
		/**
		 * Transforms a given "JSON" node into 'code syntax'
		 */
		[ActiveEvent(Name = "magix.admin._transform-node-2-code")]
		public static void magix_admin__transform_node_2_code(object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains("JSON"))
			{
				throw new ArgumentException("No node JSON passed into transform-node-2-code");
			}
			string txt = "";
			Node node = e.Params["JSON"].Value as Node;
			txt += ParseNodes(0, node);
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
							value += "=(dec)>" + idx.Get<decimal>().ToString (CultureInfo.InvariantCulture);
							break;
						case "System.DateTime":
							value += "=(date)>" + idx.Get<DateTime>().ToString ("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
							break;
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
		 * Transforms the given "code" node into a node structure, according to
		 * spaces which indents the code
		 */
		[ActiveEvent(Name = "magix.admin._transform-code-2-node")]
		public static void magix_admin__transform_code_2_node(object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains("code"))
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
					if (line.Trim ().StartsWith("//"))
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
					object value = null;

					string tmp = line.TrimStart ();
					if (!tmp.Contains("="))
					{
						name = tmp;
					}
					else
					{
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
							value = bool.Parse (tmp.Substring (name.Length + 8).Trim());
							break;
						case "=(dec)>":
							value = decimal.Parse(tmp.Substring (name.Length + 7).Trim(), CultureInfo.InvariantCulture);
							break;
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

