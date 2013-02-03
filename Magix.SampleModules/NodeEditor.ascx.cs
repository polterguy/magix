/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.SampleModules
{
    /**
     */
	[ActiveModule]
    public class NodeEditor : ActiveModule
    {
		public Panel wrp;
		public TextArea txt;

		protected void Page_Load (object sender, EventArgs e)
		{
			if (this.FirstLoad)
			{
			}
		}

		void ParseNodes (int indent, Node node)
		{
			foreach (Node idx in node)
			{
				txt.Text += "\r\n";
				for (int idxNo = 0; idxNo < indent * 2; idxNo ++)
				{
					txt.Text += " ";
				}
				string value = "";
				if (idx.Get<string>("") != "")
					value += "=>" + idx.Get<string>("").Replace ("\r\n", "\\r\\n").Replace ("\n", "\\n");
				txt.Text += idx.Name + value;
				if (idx.Count > 0)
				{
					ParseNodes (indent + 1, idx);
				}
			}
		}

		[ActiveEvent(Name = "Magix.Samples.PopulateNodeEditor")]
		public void Magix_Samples_PopulateNodeEditor (object sender, ActiveEventArgs e)
		{
			txt.Text = "";
			Node node = e.Params["JSON"].Value as Node;
			int startIdx = 0;
			if (!string.IsNullOrEmpty (node.Name))
			{
				txt.Text += node.Name;
				if (node.Value != null)
					txt.Text += "=>" + node.Value.ToString ();
				startIdx += 1;
			}
			ParseNodes(startIdx, node);
		}

		protected void close_Click(object sender, EventArgs e)
		{
			RaiseEvent ("Magix.Samples.CloseNodeEditor");
		}

		private Node ParseJSON ()
		{
			Node ret = new Node();
			using (TextReader reader = new StringReader(txt.Text))
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
			return ret;
		}

		protected void closeSave_Click(object sender, EventArgs e)
		{
			Node tmp = new Node();
			tmp["JSON"].Value = ParseJSON();
			RaiseEvent ("Magix.Samples.CloseNodeEditorAndSave", tmp);
		}
	}
}
