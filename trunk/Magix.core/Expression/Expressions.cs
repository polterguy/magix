/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
     * Level3: Implementer of Expression logic, such that nodes can be retrieved
     * and manipulated using expressions, such as e.g. [Data][Name].Value, which will
     * traverse the Data node's Name node, and return its Value
     */
	public class Expressions
	{
		private static Node GetNode (
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
					if (tmp == '[')
					{
						// Nested statement
						if (!string.IsNullOrEmpty (bufferNodeName))
							throw new ArgumentException("Don't understand: " + bufferNodeName);

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

						string subValue = null;
						string lastSubEntity = "";
						Node subNode = GetNode (entireSubStatement, source, ip, ref lastSubEntity, forcePath);
						if (lastSubEntity == ".Value")
							subValue = subNode.Value.ToString ();
						else if (lastSubEntity == ".Name")
							subValue = subNode.Name;
						else if (lastSubEntity == ".Count")
							subValue = subNode.Count.ToString ();
						else if (lastSubEntity == "")
							throw new ArgumentException("Sub expressions cannot return node lists, but only Value and Name");
						else
							throw new ArgumentException("Don't know how to parse: " + lastSubEntity);
						bufferNodeName = subValue;
						bool allNumber = true;
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
									x.Add (new Node("item"));
								}
								x = x[intIdx];
							}
							else
								return null;
                        }
                        else
                        {
							if (x.Contains (bufferNodeName))
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
                        else if (bufferNodeName == ".ip")
                        {
                            x = ip;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName == ".")
                        {
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
								else
									return null;
                            }
                            else
                            {
								if (x.Contains (bufferNodeName))
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
                    if (tmp == '[')
                    {
                        bufferNodeName = "";
                        isInside = true;
                        continue;
                    }
                    lastEntity += tmp;
                }
            }
			return x;
		}

		/**
		 * Empties the given Value or child nodes of the given expression, e.g.
		 * [Data] will remove all child nodes of the Data node, while
		 * [Data].Value will set the Value of the Data node to null
		 */
		public static void Empty (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				return;

            if (lastEntity == ".Value")
				x.Value = null;
            else if (lastEntity == "")
				x.Clear ();
            else if (lastEntity == ".Name")
                throw new ArgumentException("Cannot empty Name parts");
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		/**
		 * Will remove the given node, e.g. if [Data][Item1] is passed, it
		 * will remove Data/Item1 from the Node tree
		 */
		public static void Remove (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				throw new ArgumentException("Cannot remove a none existing Node");

            if (lastEntity == ".Value")
				x.Value = null;
            else if (lastEntity == ".Name")
				throw new ArgumentException("Cannot remove a Name of a node");
            else if (lastEntity == "")
                x.UnTie ();
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		/**
		 * Returns true if the given expression already exists in the node tree
		 */
		public static bool ExpressionExist (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				return false;

            if (lastEntity == ".Value")
                return x.Value != null;
            else if (lastEntity == ".Name")
                return !string.IsNullOrEmpty (x.Name);
            else if (lastEntity == "")
                return true;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		/**
		 * Level3: Sets the given exprDestination to the valuer of exprSource. If
		 * exprSource starts with a '[', it is expected to be a reference to another
		 * expression, else it will be assumed to be a static value
		 */
		public static void SetNodeValue (
			string exprDestination, 
			string exprSource, 
			Node source, 
			Node ip)
		{
			object valueToSet = exprSource;

			if (exprSource.IndexOf ("[") == 0)
				valueToSet = GetExpressionValue (exprSource, source, ip);

			if (valueToSet == null || exprSource == "null")
			{
				Empty (exprDestination, source, ip);
				return;
			}

			string lastEntity = "";
			Node x = GetNode (exprDestination, source, ip, ref lastEntity, true);

            if (lastEntity == ".Value")
			{
               x.Value = valueToSet;
			}
            else if (lastEntity == ".Name")
			{
				if (!(valueToSet is string))
					throw new ArgumentException("Cannot set the Name of a node to something which is not a string literal");
                x.Name = valueToSet.ToString ();
			}
            else if (lastEntity == "")
			{
				Node clone = (valueToSet as Node).Clone ();
				x.ReplaceChildren(clone);
				x.Name = clone.Name;
				x.Value = clone.Value;
			}
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		/**
		 * Returns the value of the given expression, which might return a string, 
		 * list of nodes, or any other object your node tree might contain
		 */
        public static object GetExpressionValue(string expression, Node source, Node ip)
        {
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				return null;

            if (lastEntity == ".Value")
                return x.Value;
            else if (lastEntity == ".Name")
                return x.Name;
            else if (lastEntity == ".Count")
                return x.Count;
            else if (lastEntity == "")
                return x;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
        }
	}
}

