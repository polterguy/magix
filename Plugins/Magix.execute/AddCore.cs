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
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"creates a deep copy of the node returned by the 
expression in the [value] node, and appends it into the 
node-expression found in [add].&nbsp;&nbsp;
the entire node will be copied, with its children and sub-nodes.&nbsp;&nbsp;thread safe";
				e.Params["_copy"].Value = null;
				e.Params["_data"]["children"].Value = "original";
				e.Params["_data"]["children1"].Value = "will be copied";
				e.Params["_data"]["children1"]["item1"].Value = "will be copied too";
				e.Params["_data"]["children1"]["item2"].Value = "will also be copied";
				e.Params["add"].Value = "[_copy]";
				e.Params["add"]["value"].Value = "[_data]";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.add] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			if (!ip.Contains("value"))
				throw new ArgumentException("cannot add a null node in [magix.execute.add], need [value] node to declare which node to add");

			string right = ip["value"].Get<string>();

			string left = ip.Get<string>();

			Node leftNode = Expressions.GetExpressionValue(left, dp, ip, true) as Node;
			Node rightNode = Expressions.GetExpressionValue(right, dp, ip, false) as Node;

			if (leftNode == null)
				throw new ArgumentException("both [add] and [value] must return an existing node-list, [add] value returned null, expression was; " + left);

			if (rightNode == null)
				throw new ArgumentException("both [add] and [value] must return an existing node-list, [value] node returned null, expression was; " + right);

			leftNode.Add(rightNode.Clone());
		}

        /**
         * add hyper lisp keyword
         */
        [ActiveEvent(Name = "magix.execute.add-children")]
        public static void magix_execute_add_children(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params.Clear();
                e.Params["event:magix.execute"].Value = null;
                e.Params["inspect"].Value = @"creates a deep copy of all children from the node returned by the 
expression in the [value] node, and appends the children into the 
node-expression found in [add].&nbsp;&nbsp;
thread safe";
                e.Params["_copy"].Value = null;
                e.Params["_data"]["children"].Value = "original";
                e.Params["_data"]["children1"].Value = "will be copied";
                e.Params["_data"]["children1"]["item1"].Value = "will be copied too";
                e.Params["_data"]["children1"]["item2"].Value = "will also be copied";
                e.Params["add-children"].Value = "[_copy]";
                e.Params["add-children"]["value"].Value = "[_data]";
                return;
            }

            if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
                throw new ArgumentException("you cannot raise [magix.execute.add-children] directly, except for inspect purposes");

            Node ip = e.Params["_ip"].Value as Node;

            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("value"))
                throw new ArgumentException("cannot add a null node in [magix.execute.add-children], need [value] node to declare which node to add children from");

            string right = ip["value"].Get<string>();

            string left = ip.Get<string>();

            Node leftNode = Expressions.GetExpressionValue(left, dp, ip, true) as Node;
            Node rightNode = Expressions.GetExpressionValue(right, dp, ip, false) as Node;

            if (leftNode == null)
                throw new ArgumentException("both [add-children] and [value] must return an existing node-list, [add-children] value returned null, expression was; " + left);

            if (rightNode == null)
                throw new ArgumentException("both [add-children] and [value] must return an existing node-list, [value] node returned null, expression was; " + right);

            foreach (Node idx in rightNode)
            {
                leftNode.Add(idx.Clone());
            }
        }
    }
}

