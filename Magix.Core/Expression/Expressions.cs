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
		private static Node GetNode (string expr, Node source, Node ip, ref string lastEntity)
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
                    if (tmp == ']')
                    {
                        if (string.IsNullOrEmpty(bufferNodeName))
							throw new ArgumentException("Opps, empty node name/index ...");

                        lastEntity = "";

                        bool allNumber = true;
                        if (bufferNodeName == "../")
                        {
							// One up!
                            if (x.Parent == null)
                                throw new NullReferenceException("Attempted at trying to traverse up to a level which doesn't exist. Parent Root Node reached, and found '../' still ...");
                            x = x.Parent;
                            bufferNodeName = "";
                            isInside = false;
                            continue;
                        }
                        else if (bufferNodeName == "./")
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
									throw new ArgumentException("Out of range in accessing node");
                            }
                            else
                            {
                                x = x[bufferNodeName];
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
			Node x = GetNode (expression, source, ip, ref lastEntity);

            if (lastEntity == ".Value")
				x.Value = null;
            else if (lastEntity == ".Name")
				x.Name = "";
            else if (lastEntity == "")
				x.Clear ();
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		public static void AddInteger (string expression, Node source, Node ip, int addition)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity);

            if (lastEntity == ".Value")
                x.Value = Convert.ToInt32 (x.Value.ToString ()) + addition;
            else if (lastEntity == ".Name")
				throw new ArgumentException("Cannot increment Name");
            else if (lastEntity == "")
                throw new ArgumentException("Cannot increment Node Set");
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		public static void Remove (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity);

            if (lastEntity == ".Value")
                x.Value = null;
            else if (lastEntity == ".Name")
                x.Name = "";
            else if (lastEntity == "")
                x.Parent.Remove (x);
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		public static bool ExpressionExist (string expression, Node source, Node ip)
		{
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity);

            if (lastEntity == ".Value")
                return x.Value != null;
            else if (lastEntity == ".Name")
                return x.Name != null;
            else if (lastEntity == "")
                return x != null;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

		public static void SetNodeValue (string exprDestination, string exprSource, Node source, Node ip)
		{
			object valueToSet = exprSource;

			if (exprSource.IndexOf ("[") == 0)
				valueToSet = Expressions.GetExpressionValue (exprSource, source, ip);

			string lastEntity = "";
			Node x = GetNode (exprDestination, source, ip, ref lastEntity);

            if (lastEntity == ".Value")
               x.Value = valueToSet;
            else if (lastEntity == ".Name")
                x.Name = valueToSet.ToString ();
            else if (lastEntity == "")
				x.ReplaceChildren(valueToSet as Node);
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
		}

        public static object GetExpressionValue(string expression, Node source, Node ip)
        {
			string lastEntity = "";
			Node x = GetNode (expression, source, ip, ref lastEntity);

            if (lastEntity == ".Value")
                return x.Value;
            else if (lastEntity == ".Name")
                return x.Name;
            else if (lastEntity == "")
                return x;
            else
                throw new ArgumentException("Couldn't understand the last parts of your expression '" + lastEntity + "'");
        }
	}
}

