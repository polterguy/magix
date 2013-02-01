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
        public static object GetExpressionValue(string expression, Node source)
        {
            if (expression == null)
                return source;

			string expr = expression;

            Node x = source;

            bool isInside = false;
            string bufferNodeName = null;
            string lastEntity = null;

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
                                return null;
                            }
                            else
                            {
                                if (!x.Contains(bufferNodeName))
                                    return null;
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

