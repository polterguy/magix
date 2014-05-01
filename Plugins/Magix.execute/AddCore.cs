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
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>creates a deep copy of the node returned by the 
expression in the [value] node, and appends it into the expression found in [add].&nbsp;&nbsp;if 
no [value] node is found, or the value of the [value] node is not an expression, then all child 
nodes of [add] will be added.&nbsp;&nbsp;both the [add] node, and the [value] node if given, must 
have expressions returning node lists</p>if a [value] node is given, its value is an expression, 
and a [chilren-only] node exists, and its value is true, then only the children of the nodes 
returned by the expression in [value] wil be added to the nodes found in the expression of [add].
&nbsp;&nbsp;if no [children-only] node exists, or its value is false, but a [value] node exists, 
with an expression returning a node list, then the entire node tree will be copied, with its 
children nodes, including the node returned from [value] itself</p><p>thread safe</p>";
				e.Params["_copy"].Value = null;
				e.Params["_data"]["children"].Value = "original";
				e.Params["_data"]["children1"].Value = "will be copied";
				e.Params["_data"]["children1"]["item1"].Value = "will be copied too";
				e.Params["_data"]["children1"]["item2"].Value = "will also be copied";
				e.Params["add"].Value = "[_copy]";
                e.Params["add"]["value"].Value = "[_data]";
                e.Params["add"]["children-only"].Value = "true";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.add] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

            string left = ip.Get<string>();
            Node leftNode = Expressions.GetExpressionValue(left, dp, ip, true) as Node;
            if (leftNode == null)
                throw new ArgumentException("both [add] and [value] must return an existing node-list, [add] value returned null, expression was; " + left);

            if (ip.Contains("value") && ip["value"].Get<string>("").StartsWith("["))
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

