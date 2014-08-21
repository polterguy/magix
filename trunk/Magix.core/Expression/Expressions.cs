/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Globalization;
using System.Collections.Generic;

namespace Magix.Core
{
    /*
     * implements expressions
     */
	public class Expressions
	{
        /*
         * returns expression value
         */
        public static T GetExpressionValue<T>(string expression, Node dp, Node ip, bool createPath)
        {
            object retVal = GetExpressionValue(expression, dp, ip, createPath);
            if (retVal == null)
                return default(T);

            if (typeof(T) != retVal.GetType())
            {
                switch (typeof(T).FullName)
                {
                    case "System.Int32":
                        return (T)(object)int.Parse(GetString(retVal), CultureInfo.InvariantCulture);
                    case "System.Boolean":
                        return (T)(object)bool.Parse(GetString(retVal));
                    case "System.Decimal":
                        return (T)(object)decimal.Parse(GetString(retVal), CultureInfo.InvariantCulture);
                    case "System.DateTime":
                        return (T)(object)DateTime.ParseExact(GetString(retVal), "yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                    case "System.String":
                        return (T)(object)GetString(retVal);
                    default:
                        if (retVal is T)
                            return (T)retVal;
                        throw new ArgumentException("cannot convert expression to given type; " + typeof(T).Name);
                }
            }

            return (T)retVal;
        }

        /*
         * helper for above
         */
        private static string GetString(object value)
        {
            switch (value.GetType().FullName)
            {
                case "System.Int32":
                    return value.ToString();
                case "System.Boolean":
                    return value.ToString();
                case "System.Decimal":
                    return ((decimal)value).ToString(CultureInfo.InvariantCulture);
                case "System.DateTime":
                    return ((DateTime)value).ToString("yyyy.MM.dd HH:mm:ss", CultureInfo.InvariantCulture);
                default:
                    return value.ToString();
            }
        }

        /*
         * returns expression value
         */
        private static object GetExpressionValue(string expression, Node dp, Node ip, bool createPath)
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

            if (lastEntity.StartsWith(".value"))
                retVal = x.Value;
            else if (lastEntity.StartsWith(".name"))
                retVal = x.Name;
            else if (lastEntity.StartsWith(".count"))
                retVal = x.Count;
            else if (lastEntity.StartsWith(".dna"))
                retVal = x.Dna;
            else if (lastEntity == "")
                retVal = x;

            return retVal;
        }

        /*
         * returns the formatted value of an expression, if the expression exist, or else defaultValue
         */
        public static string GetFormattedExpression(string childName, Node pars, string defaultValue)
        {
            Node ip = pars;
            if (pars.ContainsValue("_ip"))
                ip = pars["_ip"].Get<Node>();
            Node dp = pars;
            if (pars.ContainsValue("_dp"))
                dp = pars["_dp"].Get<Node>();
            return GetFormattedExpression(childName, dp, ip, defaultValue);
        }

        /*
         * returns the formatted value of an expression, if the expression exist, or else defaultValue
         */
        private static string GetFormattedExpression(string childName, Node dp, Node ip, string defaultValue)
        {
            if (ip.ContainsValue(childName))
            {
                string expressionValue = GetExpressionValue<string>(ip[childName].Get<string>(), dp, ip, false);
                if (ip[childName].Count > 0)
                {
                    return FormatString(dp, ip, ip[childName], expressionValue);
                }
                return expressionValue;
            }
            return defaultValue;
        }

        /*
         * formats a string with its children nodes
         */
        public static string FormatString(Node dp, Node ip, Node contentNode, string valueToSet)
        {
            object[] arrs = new object[contentNode.Count];
            int idxNo = 0;
            foreach (Node idx in contentNode)
            {
                if (idx.Count > 0)
                    arrs[idxNo++] = FormatString(dp, ip, idx, idx.Get<string>());
                else
                    arrs[idxNo++] = Expressions.GetExpressionValue<string>(idx.Get<string>(), dp, ip, false);
            }
            return string.Format(CultureInfo.InvariantCulture, valueToSet.ToString(), arrs);
        }

        /*
         * sets an expression
         */
		public static void SetNodeValue(
			string destinationExpression, 
			string sourceExpression, 
			Node dp, 
			Node ip,
			bool noRemove)
		{
			object valueToSet = GetExpressionValue(sourceExpression, dp, ip, false);

			// checking to see if this is a string.Format expression
			if (ip.Contains("value") && ip["value"].Count > 0)
			{
                valueToSet = FormatString(dp, ip, ip["value"], valueToSet.ToString());
			}

            if (valueToSet == null && !noRemove)
            {
                // Removing node or value
                string lastEntity = "";
                Node destinationNode = GetNode(destinationExpression, dp, ip, ref lastEntity, false);
                if (destinationNode == null)
                    return;

                if (lastEntity == ".value")
                    destinationNode.Value = null;
                else if (lastEntity == ".name")
                    destinationNode.Name = "";
                else if (lastEntity == "")
                    destinationNode.UnTie();
                else if (lastEntity == ".dna")
                    throw new ArgumentException("you cannot change a node's dna");
                else
                    throw new ArgumentException("couldn't understand the last parts of your expression '" + lastEntity + "'");
            }
            else
            {
                string lastEntity = ".value";
                Node destinationNode = ip;
                
                if (destinationExpression != null)
                    destinationNode = GetNode(destinationExpression, dp, ip, ref lastEntity, true);

                if (lastEntity.StartsWith(".value"))
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
                else if (lastEntity.StartsWith(".name"))
                {
                    if (!(valueToSet is string))
                        throw new ArgumentException("cannot set the name of a node to something which is not a string literal");
                    destinationNode.Name = valueToSet.ToString();
                }
                else if (lastEntity.StartsWith(".dna"))
                {
                    throw new ArgumentException("cannot change the dna of a node");
                }
                else if (lastEntity == "")
                {
                    if (!(valueToSet is Node))
                        throw new ArgumentException("you can only set a node-list to another node-list, and not a string or some other constant value");

                    Node clone = (valueToSet as Node).Clone();
                    destinationNode.Clear();
                    destinationNode.AddRange(clone);
                    destinationNode.Name = clone.Name;
                    destinationNode.Value = clone.Value;
                }
                else
                    throw new ArgumentException("couldn't understand the last parts of your expression '" + lastEntity + "'");
            }
		}

		// Helper for finding nodes
		private static Node GetNode(
			string expr, 
			Node dp, 
			Node ip, 
			ref string lastEntity, 
			bool forcePath)
		{
			Node idxNode = dp;
            bool isInside = false;
            int noBraces = 0;
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
						object innerVal = GetExpressionValue(entireSubStatement, dp, ip, false);
						if (innerVal == null)
							throw new ArgumentException("subexpression failed, expression was; " + entireSubStatement);
						tmp = ']'; // to sucker into ending of logic
						bufferNodeName = innerVal.ToString();
					}
                    if (tmp == '[')
                        noBraces += 1;
                    if (tmp == ']' && --noBraces == 0)
                    {
                        isInside = false;
                        lastEntity = "";
                        bool allNumber = !string.IsNullOrEmpty(bufferNodeName);
                        if (bufferNodeName == "..")
                        {
							// One up!
                            if (idxNode.Parent == null)
								return null;
                            idxNode = idxNode.Parent;
                        }
                        else if (bufferNodeName == "/")
                        {
                            idxNode = idxNode.RootNode();
                        }
                        else if (bufferNodeName == ">last")
                        {
                            idxNode = idxNode[idxNode.Count - 1];
                        }
                        else if (bufferNodeName == "$")
						{
                            if (forcePath)
                                idxNode = dp.RootNode()["$"];
                            else
                            {
                                if (dp.RootNode().Contains("$"))
                                    idxNode = dp.RootNode()["$"];
                                else
                                    return null;
                            }
						}
						else if (bufferNodeName == ".ip")
						{
							idxNode = ip;
						}
						else if (bufferNodeName == "@")
						{
							idxNode = ip.Parent;
						}
                        else if (bufferNodeName == ".")
                        {
							idxNode = dp;
                        }
                        else if (bufferNodeName.StartsWith(":>"))
                        {
                            // dna reference
                            string dna = bufferNodeName.Substring(2);
                            idxNode = idxNode.RootNode().FindDna(dna);
                        }
                        else if (bufferNodeName.StartsWith(":"))
                        {
                            // active event reference
                            string activeEvent = bufferNodeName.Substring(1);
                            Node newDataNode = null;
                            if (activeEvent.Contains("["))
                            {
                                string acNodeExpression = activeEvent.Substring(activeEvent.IndexOf('['));
                                newDataNode = GetExpressionValue(acNodeExpression, dp, ip, false) as Node;
                                if (newDataNode == null)
                                    throw new ArgumentException("cannot pass a null node into an expression active event");

                                newDataNode = newDataNode.Clone();

                                activeEvent = activeEvent.Substring(0, activeEvent.IndexOf('['));
                            }
                            Node exeNode = new Node(null, null);
                            if (newDataNode != null)
                                exeNode[activeEvent].AddRange(newDataNode);
                            else
                                exeNode[activeEvent].Value = null;
                            Node invokeNode = new Node();
                            invokeNode["_ip"].Value = exeNode;
                            invokeNode["_dp"].Value = exeNode;
                            ActiveEvents.Instance.RaiseActiveEvent(
                                typeof(Expressions),
                                "magix.execute",
                                invokeNode);
                            idxNode = exeNode[activeEvent];
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
                            }

                            Node searchNode = FindNode(idxNode, searchName, searchValue);
                            if (searchNode == null && forcePath)
                            {
                                if (searchName == "?")
                                    searchName = "";
                                if (searchValue == "?")
                                    searchValue = "";
                                idxNode.Add(new Node(searchName ?? "", searchValue));
                                idxNode = idxNode[idxNode.Count - 1];
                            }
                            else if (searchNode == null)
                                throw new ArgumentException("couldn't find node in expression");
                            else
                                idxNode = searchNode;
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
                            foreach (Node idxInnerNode in idxNode)
                            {
                                if (searchValue == idxInnerNode.Get<string>())
                                {
                                    found = true;
                                    idxNode = idxInnerNode;
                                    break;
                                }
                            }
                            if (!found && forcePath)
                            {
                                idxNode.Add(new Node("", searchValue));
                                idxNode = idxNode[idxNode.Count - 1];
                            }
                            else if (!found)
                                throw new ArgumentException("couldn't find node in expression");
                        }
                        else if (bufferNodeName.Contains("=>"))
                        {
                            string[] splits = bufferNodeName.Split(new string[] { "=>" }, StringSplitOptions.None);
                            string name = splits[0];
                            string value = splits[1];
                            bool foundNode = false;
                            foreach (Node idxInnerNode in idxNode)
                            {
                                if (idxInnerNode.Name == name && idxInnerNode.Get<string>() == value)
                                {
                                    idxNode = idxInnerNode;
                                    foundNode = true;
                                    break;
                                }
                            }
                            if (!foundNode)
                            {
                                // didn't find criteria
                                if (!forcePath)
                                    return null;

                                // creating node with given value
                                idxNode.Add(new Node(name, value));
                                idxNode = idxNode[idxNode.Count - 1];
                            }
                        }
                        else if (bufferNodeName.Contains(":"))
                        {
                            int idxNo = int.Parse(bufferNodeName.Split(':')[1].TrimStart());
                            int curNo = 0;
                            int totIdx = 0;
                            bool found = false;
                            bufferNodeName = bufferNodeName.Split(':')[0].TrimEnd();

                            foreach (Node idxInnerNode in idxNode)
                            {
                                if (idxInnerNode.Name == bufferNodeName)
                                {
                                    if (curNo++ == idxNo)
                                    {
                                        found = true;
                                        idxNode = idxNode[totIdx];
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
                                        idxNode.Add(new Node(bufferNodeName));
                                    }
                                    idxNode = idxNode[idxNode.Count - 1];
                                }
                                else
                                    return null;
                            }
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
                                if (idxNode.Count > intIdx)
                                    idxNode = idxNode[intIdx];
                                else if (forcePath)
                                {
                                    while (idxNode.Count <= intIdx)
                                    {
                                        idxNode.Add(new Node("item"));
                                    }
                                    idxNode = idxNode[intIdx];
                                }
                                else
                                    return null;
                            }
                            else
                            {
                                if (idxNode.Contains(bufferNodeName))
                                    idxNode = idxNode[bufferNodeName];
                                else if (forcePath)
                                {
                                    idxNode = idxNode[bufferNodeName];
                                }
                                else
                                {
                                    return null;
                                }
                            }
                        }
                        bufferNodeName = "";
                    }
                    else
                        bufferNodeName += tmp;
                }
                else
                {
                    if (tmp == '[' && string.IsNullOrEmpty(lastEntity))
                    {
                        if (++noBraces == 1)
                            isInside = true;
                        else
                            bufferNodeName += tmp;
                    }
                    else
                        lastEntity += tmp;
                }
            }
            if (lastEntity.StartsWith(".value") && lastEntity.Length > 6)
            {
                // this is a concatenated expression, returning a Node list, where we wish to directly 
                // access another node inside of the node by reference
                string innerLastReference = "";
                Node x2 = idxNode.Get<Node>();
                if (x2 == null)
                {
                    // value of node is probably a string, try to convert it to a node first
                    Node tmpNode = new Node();
                    tmpNode["code"].Value = idxNode.Get<string>();
                    ActiveEvents.Instance.RaiseActiveEvent(
                        typeof (Expressions),
                        "magix.execute.code-2-node",
                        tmpNode);
                    x2 = tmpNode["node"].Clone();
                }
                idxNode = GetNode(lastEntity.Substring(6), x2, x2, ref innerLastReference, forcePath);
                lastEntity = innerLastReference;
            }
			return idxNode;
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

