/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * hyper lisp add logic
	 */
	public class AddCore : ActiveController
	{
		/**
		 * add hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.add")]
		public static void magix_execute_add(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>creates a deep copy of the node returned by the 
expression in the [value] node, and appends it into the expression found in [add].&nbsp;&nbsp;if 
no [value] node is found, or the value of the [value] node is not an expression, then all child 
nodes of [add] will be added.&nbsp;&nbsp;both the [add] node, and the [value] node if given, must 
have expressions returning node lists</p><p>if a [value] node is given, its value is an expression, 
and a [chilren-only] node exists, and its value is true, then only the children of the nodes 
returned by the expression in [value] wil be added to the nodes found in the expression of [add].
&nbsp;&nbsp;if no [children-only] node exists, or its value is false, but a [value] node exists, 
with an expression returning a node list, then the entire node tree will be copied, with its 
children nodes, including the node returned from [value] itself</p><p>if a [value] node exists, 
and it is a contant, or an expression returning anything but a node list, the another [value] 
node beneath the first [value] node will be expected.&nbsp;&nbsp;in this case, one node will be 
added to the node list in the [add] expression, with the name of the first [value] node's value, 
and the value of the [value] node's [value] expression</p><p>thread safe</p>";
                ip["_copy"].Value = null;
                ip["_data"]["children"].Value = "original";
                ip["_data"]["children1"].Value = "will be copied";
                ip["_data"]["children1"]["item1"].Value = "will be copied too";
                ip["_data"]["children1"]["item2"].Value = "will also be copied";
                ip["add"].Value = "[_copy]";
                ip["add"]["value"].Value = "[_data]";
                ip["add"]["children-only"].Value = "true";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.add] directly, except for inspect purposes");

			Node dp = Dp(e.Params);

            string left = ip.Get<string>();
            Node leftNode = Expressions.GetExpressionValue(left, dp, ip, true) as Node;
            if (leftNode == null)
                throw new ArgumentException("[add] must return an existing node-list, [add] value returned null, expression was; " + left);

            if (ip.Contains("value") && ip["value"].Value != null)
            {
                if (ip["value"].Contains("value"))
                {
                    string nodeNameExpression = ip["value"].Get<string>();
                    string nodeName = Expressions.GetExpressionValue(nodeNameExpression, dp, ip, false) as string;
                    if (nodeName == null)
                        throw new ArgumentException("cannot add a node, who's name is null");

                    string nodeValueExpression = ip["value"]["value"].Get<string>();
                    string nodeValue = Expressions.GetExpressionValue(nodeValueExpression, dp, ip, false) as string;
                    leftNode.Add(new Node(nodeName, nodeValue));
                }
                else
                {
                    string right = ip["value"].Get<string>();

                    Node rightNode = Expressions.GetExpressionValue(right, dp, ip, false) as Node;

                    if (rightNode == null)
                        throw new ArgumentException("both [add] and [value] must return an existing node-list, [value] node returned null, expression was; " + right);

                    if (!ip.Contains("children-only") || !ip["children-only"].Get<bool>())
                        leftNode.Add(rightNode.Clone());
                    else
                    {
                        foreach (Node idx in rightNode)
                        {
                            leftNode.Add(idx.Clone());
                        }
                    }
                }
            }
            else
            {
                foreach (Node idx in ip)
                {
                    leftNode.Add(idx.Clone());
                }
            }
		}
    }
}

