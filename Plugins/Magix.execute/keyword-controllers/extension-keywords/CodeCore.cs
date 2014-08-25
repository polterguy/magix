/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Text;
using System.Globalization;
using Magix.Core;

namespace Magix.execute
{
    /*
     * transforms from node to code and vice versa
     */
    public class CodeCore : ActiveController
    {
        /*
         * transforms from node to code
         */
        [ActiveEvent(Name = "magix.execute.node-2-code")]
        public static void magix_exeute_node_2_code(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.node-2-code-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.node-2-code-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // verifying syntax
            if (!ip.Contains("node"))
                throw new HyperlispSyntaxErrorException("no [node] node passed into node-2-code");

            // transforms node to code
            ExecuteNode2Code(ip, dp);
        }

        /*
         * transforms from code to node
         */
        [ActiveEvent(Name = "magix.execute.code-2-node")]
        public static void magix_execute_code_2_node(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.code-2-node-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.code-2-node-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // verifying syntax of [code-2-node]
            if (!ip.ContainsValue("code") && !ip.ContainsValue("file"))
                throw new HyperlispSyntaxErrorException("no [code] or [file] node passed into [code-2-node]");

            // verifying syntax of [code-2-node]
            if (ip.Contains("code") && ip.Contains("file"))
                throw new ArgumentException("you cannot supply both a [file] node and a [code] node to [code-2-node]");

            // executing [code-2-node]
            ExecuteCode2Node(e.Params, dp, ip);
        }

        /*
         * actual implementation of [code-2-node]
         */
        private static void ExecuteCode2Node(Node pars, Node dp, Node ip)
        {
            // retrieving actual code
            string codeTextString = null;
            if (ip.Contains("code"))
                codeTextString = Expressions.GetExpressionValue<string>(ip["code"].Get<string>(), dp, ip, false);
            else
                codeTextString = LoadCodeFromFile(pars, ip);

            // converting from code to node
            ExecuteCode2NodeFromString(codeTextString, ip["node"]);
        }

        /*
         * transforms from code to node
         */
        private static void ExecuteCode2NodeFromString(string codeSource, Node resultNode)
        {
            // wrapping our code in a text reader for simplicity
            using (TextReader reader = new StringReader(codeSource))
            {
                int indents = 0;
                Node currentNode = resultNode;
                while (true)
                {
                    // retrieving next line from reader
                    string line = reader.ReadLine();

                    if (line == null)
                        break; // finished, eof

                    if (line.Trim() == string.Empty || line.Trim().StartsWith("//"))
                        continue; // skipping white lines and comments

                    // retrieving value of next node, and setting current node to the added node, changing indentations accordingly
                    currentNode = Code2NodeGetNextNode(reader, ref indents, currentNode, line);
                }
            }
        }

        /*
         * returns next node parsed from reader
         */
        private static Node Code2NodeGetNextNode(TextReader reader, ref int indents, Node idxNode, string line)
        {
            string name = "";
            object value = null;

            string trimmedLine = line.TrimStart();
            if (!trimmedLine.Contains("="))
                name = trimmedLine; // only name here, no value
            else
            {
                // here is probably some sort of both name and value, figuring out name first
                if (trimmedLine[0] == '=')
                    name = string.Empty; // empty name ...
                else
                    name = trimmedLine.Substring(0, trimmedLine.IndexOf("="));

                // then figuring out value
                value = Code2NodeGetNodeValue(reader, name, trimmedLine.Substring(name.Length));
            }

            // adding up node and updating indentations
            return Code2NodeAddNode(ref indents, idxNode, line, name, value);
        }

        /*
         * actually adds node to result collection and updates indentations and returns the newly created and added node
         */
        private static Node Code2NodeAddNode(ref int indents, Node idxNode, string line, string name, object value)
        {
            // figuring out nuber of indentations
            int currentIndents = GetNumberOfIndentations(line);
            if (currentIndents == indents)
            {
                // same level
                Node xNode = new Node(name, value);
                idxNode.Add(xNode);
            }
            else if (currentIndents < indents)
            {
                // decreasing indentations, moving 'upwards' in hierarchy
                while (currentIndents < indents)
                {
                    idxNode = idxNode.Parent;
                    indents -= 1;
                }
                idxNode.Add(new Node(name, value));
            }
            else
            {
                // increasing indentations, moving 'downwards' in hierarchy
                // verifying syntax
                if (currentIndents - indents > 1)
                    throw new ArgumentException("multiple indentations found in [code-2-node], without specifying child node name");
                idxNode = idxNode[idxNode.Count - 1];
                idxNode.Add(new Node(name, value));
                indents += 1;
            }

            // returning node just added
            return idxNode;
        }

        /*
         * figuring out node value
         */
        private static object Code2NodeGetNodeValue(TextReader reader, string name, string trimmedLineWithoutName)
        {
            object value = null;
            switch (trimmedLineWithoutName.Split('>')[0] + ">")
            {
                case "=>":
                    if (!trimmedLineWithoutName.StartsWith("=>@\""))
                        value = trimmedLineWithoutName.Substring(2); // simple string value
                    else
                        value = Code2NodeReadMultipleLines(reader, trimmedLineWithoutName.Substring(4));
                    break;
                case "=(int)>":
                    value = int.Parse(trimmedLineWithoutName.Substring(7));
                    break;
                case "=(date)>":
                    value = DateTime.ParseExact(
                        trimmedLineWithoutName.Substring(8),
                        "yyyy.MM.dd HH:mm:ss",
                        CultureInfo.InvariantCulture);
                    break;
                case "=(bool)>":
                    value = bool.Parse(trimmedLineWithoutName.Substring(8));
                    break;
                case "=(dec)>":
                    value = decimal.Parse(trimmedLineWithoutName.Substring(7), CultureInfo.InvariantCulture);
                    break;
                case "=(node)>":
                    value = Code2NodeReadMultipleLines(reader, trimmedLineWithoutName.Substring(10));

                    // converting our inner node value from code to node, recursively calling "self" to retrieve node value
                    Node tmpNode = new Node();
                    tmpNode["code"].Value = value.ToString().Trim();
                    RaiseActiveEvent(
                        "magix.execute.code-2-node",
                        tmpNode);
                    value = tmpNode["node"].Clone();
                    break;
            }
            return value;
        }

        /*
         * reads from the text reader until string literal is closed
         */
        private static object Code2NodeReadMultipleLines(TextReader reader, string tmpLine)
        {
            object value = null;
            while (true)
            {
                // looping until string is ended, which might span several lines, first figuring out if this is the last line
                int noFnut = 0;
                for (int idxNo = tmpLine.Length - 1; idxNo >= 0; idxNo--)
                {
                    if (tmpLine[idxNo] == '"')
                        noFnut += 1;
                    else
                        break;
                }

                // replacing all occurrencies of "" with "
                value += tmpLine.Replace("\"\"", "\"");

                if (noFnut % 2 != 0)
                {
                    // this was our last line of multi-line text, removing last " appended from current line before breaking while loop
                    value = ((string)value).TrimEnd().Substring(0, ((string)value).Length - 1);
                    break;
                }

                // adding carriage return
                value += "\n";
                tmpLine = reader.ReadLine();

                if (tmpLine == null)
                    throw new HyperlispSyntaxErrorException("unfinished string literal: " + value);
            }
            return value;
        }

        /*
         * returns number of indentations of current line from code
         */
        private static int GetNumberOfIndentations(string line)
        {
            // counting indents
            int indentations = 0;
            foreach (char idx in line)
            {
                if (idx != ' ')
                    break;
                indentations += 1;
            }
            if (indentations % 2 != 0)
                throw new HyperlispSyntaxErrorException("only even number of indents allowed in code syntax");
            return indentations / 2;
        }

        /*
         * loads code from file
         */
        private static string LoadCodeFromFile(Node pars, Node ip)
        {
            try
            {
                RaiseActiveEvent(
                    "magix.file.load",
                    pars);
                return ip["value"].Get<string>();
            }
            finally
            {
                ip["value"].UnTie();
            }
        }

        /*
         * implementation of [node-2-code]
         */
        private static void ExecuteNode2Code(Node ip, Node dp)
        {
            Node node = new Node();
            if (ip["node"].Value != null && ip["node"].Value is Node)
            {
                // node's value is a node in itself
                node = ip["node"].Get<Node>();
            }
            else if (ip["node"].Value != null)
            {
                // assuming it's an expression
                node.Add(Expressions.GetExpressionValue<Node>(ip["node"].Get<string>(), dp, ip, false));
            }
            else
            {
                // assuming nodes to convert is beneath [node] as children
                node = ip["node"].Clone();
            }

            // checking to see if we should remove root node
            if (ip.Contains("remove-root") && ip["remove-root"].Get<bool>())
                node = node[0];

            // parsing nodes, and returning value
            StringBuilder builder = new StringBuilder();
            ParseNodes(0, node, builder);
            ip["code"].Value = builder.ToString();
        }

        /*
         * actual parsing from nodes to code syntax
         */
        private static void ParseNodes(int indent, Node node, StringBuilder builder)
        {
            foreach (Node idx in node)
            {
                for (int idxNo = 0; idxNo < indent; idxNo++)
                {
                    builder.Append("  ");
                }
                string value = "";
                if (idx.Value != null)
                {
                    if (idx.Value.GetType() != typeof(string))
                    {
                        switch (idx.Value.GetType().FullName)
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

                                    StringBuilder builder2 = new StringBuilder();
                                    builder2.Append(@"=(node)>@""" + nodeS + "\r\n");
                                    ParseNodes(string.IsNullOrEmpty(nodeS) ? 0 : 1, idx.Get<Node>(), builder2);
                                    builder2.Append(@"""");
                                    value += builder2.ToString();
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
                            nValue = nValue.Replace("\"", "\"\"");
                            value += "=>" + "@\"" + nValue + "\"";
                        }
                        else
                            value += "=>" + idx.Get<string>("").Replace("\r\n", "\\n").Replace("\n", "\\n");
                    }
                }
                builder.Append(idx.Name + value + "\n");
                if (idx.Count > 0)
                    ParseNodes(indent + 1, idx, builder);
            }
        }
    }
}






