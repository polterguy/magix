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

            string destinationExpression = ip.Get<string>();
            Node destinationNode = Expressions.GetExpressionValue(destinationExpression, dp, ip, true) as Node;
            if (destinationNode == null)
                throw new ArgumentException("[add] must return an existing node-list, [add] value returned null, expression was; " + destinationExpression);

            if (ip.Contains("value") && ip["value"].Value != null)
            {
                object sourceObject = Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false);

                if (sourceObject is Node)
                {
                    Node sourceNode = sourceObject as Node;
                    if (!ip.Contains("children-only") || !ip["children-only"].Get<bool>())
                        destinationNode.Add(sourceNode.Clone());
                    else
                        destinationNode.AddRange(sourceNode);
                }
                else
                {
                    string nodeName = sourceObject.ToString();

                    object nodeValue = null;
                    if (ip["value"].Contains("value"))
                        nodeValue = Expressions.GetExpressionValue(ip["value"]["value"].Get<string>(), dp, ip, false);
                    destinationNode.Add(new Node(nodeName, nodeValue));
                }
            }
            else
            {
                foreach (Node idx in ip)
                {
                    destinationNode.Add(idx.Clone());
                }
            }
		}
    }
}

