/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Globalization;
using System.Collections.Generic;

namespace Magix.Core
{
    /*
     * Implementer of Expression logic, such that nodes can be retrieved
     * and manipulated using expressions, such as e.g. [Data][Name].Value, which will
     * traverse the Data node's Name node, and return its Value
     */
	public class Expressions
	{
        /*
         * Returns the value of the given expression, which might return a string, 
         * list of nodes, or any other object your node tree might contain
         */
        public static object GetExpressionValue(string expression, Node dp, Node ip, bool createPath)
        {
            if (expression == null)
                return null;

            // Checking to see if this is an escaped expression
            if (expression.StartsWith("\\") && expression.Length > 1 && expression[1] == '[')
                return expression.Substring(1);

            if (!expression.TrimStart().StartsWith("["))
                return expression;

            string lastEntity = "";
            Node x = GetNode(expression, dp, ip, ref lastEntity, createPath);

            if (x == null)
                return null;

            object retVal = null;

            if (lastEntity.StartsWith(".Value"))
                retVal = x.Value;
            else if (lastEntity.StartsWith(".Name"))
                retVal = x.Name;
            else if (lastEntity.StartsWith(".Count"))
                retVal = x.Count;
            else if (lastEntity == "")
                retVal = x;

            return retVal;
        }

		/*
		 * Sets the given exprDestination to the valuer of exprSource. If
		 * exprSource starts with a '[', it is expected to be a reference to another
		 * expression, else it will be assumed to be a static value
		 */
		public static void SetNodeValue(
			string destinationExpression, 
			string sourceExpression, 
			Node source, 
			Node ip,
			bool noRemove)
		{
			object valueToSet = GetExpressionValue(sourceExpression, source, ip, false);

			// checking to see if this is a string.Format expression
			if (ip.Contains("value") && ip["value"].Count > 0)
			{
				object[] arrs = new object[ip["value"].Count];
				int idxNo = 0;
				foreach (Node idx in ip["value"])
				{
					arrs[idxNo++] = Expressions.GetExpressionValue(idx.Get<string>(), source, ip, false);
				}
				valueToSet = string.Format(valueToSet.ToString(), arrs);
			}

            if (valueToSet == null && !noRemove)
            {
                // Removing node or value
                string lastEntity = "";
                Node destinationNode = GetNode(destinationExpression, source, ip, ref lastEntity, false);
                if (destinationNode == null)
                    return;

                if (lastEntity == ".Value")
                    destinationNode.Value = null;
                else if (lastEntity == ".Name")
                    destinationNode.Name = "";
                else if (lastEntity == "")
                    destinationNode.UnTie();
                else
                    throw new ArgumentException("couldn't understand the last parts of your expression '" + lastEntity + "'");
            }
            else
            {
                string lastEntity = ".Value";
                Node destinationNode = ip;
                
                if (destinationExpression != null)
                    destinationNode = GetNode(destinationExpression, source, ip, ref lastEntity, true);

                if (lastEntity.StartsWith(".Value"))
                {
                    if (valueToSet is Node)
                    {
                        valueToSet = (valueToSet as Node).Clone(); // to make sure we can add nodes as values into the same nodes, recursively ...
                        
                        // we must create a root node, to conform with the api for serializing nodes
                        // such that when we assign a node's value to be another node, we actually add a 
                        // non-existing parent node, and not the node itself in fact
                        Node tmpNode = new Node();
                        tmpNode.Add(valueToSet as Node);
                        valueToSet = tmpNode;
                    }
                    destinationNode.Value = valueToSet;
                }
                else if (lastEntity.StartsWith(".Name"))
                {
                    if (!(valueToSet is string))
                        throw new ArgumentException("Cannot set the Name of a node to something which is not a string literal");
                    destinationNode.Name = valueToSet.ToString();
                }
                else if (lastEntity == "")
                {
                    if (!(valueToSet is Node))
                        throw new ArgumentException("you can only set a node-list to another node-list, and not a string or some other constant value");

                    Node clone = (valueToSet as Node).Clone();
                    destinationNode.ReplaceChildren(clone);
                    destinationNode.Name = clone.Name;
                    destinationNode.Value = clone.Value;
                }
                else
                    throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
            }
		}

		// Helper for finding nodes
		private static Node GetNode(
			string expr, 
			Node source, 
			Node ip, 
			ref string lastEntity, 
			bool forcePath)
		{
			Node x = source;

            bool isInside = false;
            string bufferNodeName = null;
            lastEntity = null;

            for (int idx = 0; idx < expr.Length; idx++)
            {
                char tmp = expr[idx];
                if (isInside)
                {
					if (tmp == '[' && string.IsNullOrEmpty(bufferNodeName))
					{
						// Nested statement
						// Spooling forward to end of nested statement
						string entireSubStatement = "";
						int braceIndex = 0;
						for (; idx < expr.Length; idx++)
						{
							if (expr[idx] == '[')
								braceIndex += 1;
							else if (expr[idx] == ']')
								braceIndex -= 1;
							if (braceIndex == -1)
								break;
							entireSubStatement += expr[idx];
						}

						object innerVal = GetExpressionValue(entireSubStatement, source, ip, false);

						if (innerVal == null)
							throw new ArgumentException("subexpression failed, expression was; " + entireSubStatement);

						tmp = ']'; // to sucker into ending of logic
						bufferNodeName = innerVal.ToString();
					}
                    if (tmp == ']')
                    {
                        if (string.IsNullOrEmpty(bufferNodeName))
							throw new ArgumentException("Opps, empty node name/index ...");

                        lastEntity = "";

                        bool allNumber = true;
                        if (bufferNodeName == "..")
                        {
							// One up!
                            if (x.Parent == null)
								return null;
                            x = x.Parent;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName == "/")
                        {
                            x = x.RootNode();
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
						else if (bufferNodeName == "$")
						{
							x = ip.RootNode()["$"];
							isInside = false;
							continue;
						}
						else if (bufferNodeName == ".ip")
						{
							x = ip;
							bufferNodeName = "";
							isInside = false;
							continue;
						}
						else if (bufferNodeName == "@")
						{
							x = ip.Parent;
							bufferNodeName = "";
							isInside = false;
							continue;
						}
                        else if (bufferNodeName == ".")
                        {
							x = source;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName.StartsWith("**"))
                        {
                            // deep wildcard search
                            string searchValue = null;
                            string searchName = null;
                            if (bufferNodeName.Contains("=>"))
                            {
                                if (bufferNodeName.IndexOf("=>") > 2)
                                    searchName = bufferNodeName.Substring(2).Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries)[0];
                                else
                                    searchName = "";
                                if (bufferNodeName.IndexOf("=>") < bufferNodeName.Length - 2)
                                    searchValue = bufferNodeName.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries)[1];
                                else
                                    searchValue = "";
                            }
                            else
                            {
                                if (bufferNodeName.Length > 2)
                                    searchName = bufferNodeName.Substring(2);
                                else
                                    bufferNodeName = "";
                            }

                            Node searchNode = FindNode(x, searchName, searchValue);
                            if (searchNode == null && forcePath)
                            {
                                if (searchName == "?")
                                    searchName = "";
                                if (searchValue == "?")
                                    searchValue = "";
                                x.Add(new Node(searchName ?? "", searchValue));
                                x = x[x.Count - 1];
                            }
                            else if (searchNode == null)
                                throw new ArgumentException("couldn't find node in expression");
                            else
                                x = searchNode;

                            bufferNodeName = "";
                            isInside = false;
                        }
                        else if (bufferNodeName.StartsWith("?"))
                        {
                            // wildcard search
                            string searchValue = null;
                            if (bufferNodeName.Contains("=>"))
                            {
                                string[] tmpSearch = bufferNodeName.Split(new string[] { "=>" }, StringSplitOptions.RemoveEmptyEntries);
                                if (tmpSearch.Length == 1)
                                    searchValue = "";
                                else
                                    searchValue = tmpSearch[1];
                            }
                            bool found = false;
                            foreach (Node idxNode in x)
                            {
                                if (searchValue == idxNode.Get<string>())
                                {
                                    found = true;
                                    x = idxNode;
                                    break;
                                }
                            }
                            if (!found && forcePath)
                            {
                                x.Add(new Node("", searchValue));
                                x = x[x.Count - 1];
                            }
                            else if (!found)
                                throw new ArgumentException("couldn't find node in expression");

                            bufferNodeName = "";
                            isInside = false;
                        }
                        else if (bufferNodeName.Contains("=>"))
                        {
                            string[] splits = bufferNodeName.Split(new string[] { "=>" }, StringSplitOptions.None);
                            string name = splits[0];
                            string value = splits[1];

                            foreach (Node idxNode in x)
                            {
                                if (idxNode.Name == name && idxNode.Get<string>() == value)
                                {
                                    x = idxNode;
                                    bufferNodeName = "";
                                    break;
                                }
                            }
                            if (!string.IsNullOrEmpty(bufferNodeName))
                            {
                                // didn't find criteria
                                if (!forcePath)
                                    return null;

                                // creating node with given value
                                x.Add(new Node(name, value));
                                x = x[x.Count - 1];
                            }
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName.Contains(":"))
                        {
                            int idxNo = int.Parse(bufferNodeName.Split(':')[1].TrimStart());
                            int curNo = 0;
                            int totIdx = 0;
                            bool found = false;
                            bufferNodeName = bufferNodeName.Split(':')[0].TrimEnd();

                            foreach (Node idxNode in x)
                            {
                                if (idxNode.Name == bufferNodeName)
                                {
                                    if (curNo++ == idxNo)
                                    {
                                        found = true;
                                        x = x[totIdx];
                                        break;
                                    }
                                }
                                totIdx += 1;
                            }
                            if (!found)
                            {
                                if (forcePath)
                                {
                                    while (idxNo >= curNo++)
                                    {
                                        x.Add(new Node(bufferNodeName));
                                    }
                                    x = x[x.Count - 1];
                                }
                                else
                                    return null;
                            }
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else
                        {
                            foreach (char idxC in bufferNodeName)
                            {
                                if (("0123456789").IndexOf(idxC) == -1)
                                {
                                    allNumber = false;
                                    break;
                                }
                            }
                            if (allNumber)
                            {
                                int intIdx = int.Parse(bufferNodeName);
                                if (x.Count > intIdx)
                                    x = x[intIdx];
                                else if (forcePath)
                                {
                                    while (x.Count <= intIdx)
                                    {
                                        x.Add(new Node("item"));
                                    }
                                    x = x[intIdx];
                                }
                                else
                                    return null;
                            }
                            else
                            {
                                if (x.Contains(bufferNodeName))
                                    x = x[bufferNodeName];
                                else if (forcePath)
                                {
                                    x = x[bufferNodeName];
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                    }
                    bufferNodeName += tmp;
                }
                else
                {
                    if (tmp == '[' && string.IsNullOrEmpty(lastEntity))
                    {
                        bufferNodeName = "";
                        isInside = true;
                        continue;
                    }
                    lastEntity += tmp;
                }
            }
            if (lastEntity.StartsWith(".Value") && lastEntity.Length > 6)
            {
                // this is a concatenated expression, returning a Node list, where we wish to directly 
                // access another node inside of the node by reference
                string innerLastReference = "";
                Node x2 = x.Value as Node;
                if (x2 == null)
                {
                    // value of node is probably a string, try to convert it to a node first
                    Node tmpNode = new Node();
                    tmpNode["code"].Value = x.Get<string>();
                    ActiveEvents.Instance.RaiseActiveEvent(
                        typeof (Expressions),
                        "magix.execute.code-2-node",
                        tmpNode);
                    x2 = tmpNode["node"].Clone();
                }
                x = GetNode(lastEntity.Substring(6), x2, x2, ref innerLastReference, forcePath);
                lastEntity = innerLastReference;
            }
			return x;
		}

        private static Node FindNode(Node currentNode, string searchName, string searchValue)
        {
            bool foundName = false;
            if (searchName == "?")
                foundName = true;
            else if (currentNode.Name == searchName)
                foundName = true;
            
            bool foundValue = false;
            if (searchValue == "?")
                foundValue = true;
            else if (searchValue == currentNode.Get<string>())
                foundValue = true;

            if (foundValue && foundName)
                return currentNode;

            foreach (Node idx in currentNode)
            {
                Node tmp = FindNode(idx, searchName, searchValue);
                if (tmp != null)
                    return tmp;
            }
            return null;
        }
	}
}

