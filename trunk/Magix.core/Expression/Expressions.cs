/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
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
                                if (x.Count >= intIdx)
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

		public static void Remove (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity, false);

			if (x == null)
				return;

            if (lastEntity == ".Value")
				throw new ArgumentException("Use set to remove a Value of a node");
            else if (lastEntity == ".Name")
				throw new ArgumentException("Cannot remove a Name of a node");
            else if (lastEntity == "")
                x.UnTie ();
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

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
            else if (lastEntity == ".Count")
				return x.Count > 0;
            else if (lastEntity == "")
                return true;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

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
               x.Value = valueToSet;
            else if (lastEntity == ".Name")
			{
				if (!(valueToSet is string))
					throw new ArgumentException("Cannot set the Name of a node to something which is not a string literal");
                x.Name = valueToSet.ToString ();
			}
            else if (lastEntity == "")
			{
				x.ReplaceChildren((valueToSet as Node).Clone ());
			}
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		public static void SetNodeValue (
			string exprDestination, 
			Node source)
		{
			string lastEntity = "";
			Node x = GetNode (exprDestination, source, source, ref lastEntity, true);

            if (lastEntity == ".Value")
				x.Value = source.Clone ();
            else if (lastEntity == ".Name")
			{
				throw new ArgumentException("Cannot set the Name of a node a node set");
			}
            else if (lastEntity == "")
			{
				x.ReplaceChildren(source.Clone ());
			}
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

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
