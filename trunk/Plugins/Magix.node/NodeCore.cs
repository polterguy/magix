/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * helper for manipulating nodes
	 */
	public class NodeCore : ActiveController
	{
		/**
		 * replaces node's contents
		 */
		[ActiveEvent(Name = "magix.execute.replace")]
		public void magix_execute_replace(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["_expression"].Value = "some value to be replaced";
				e.Params["inspect"].Value = @"replaces the [what] value/expression with [with] value/expression in 
the value of the [replace] node's expression.&nbsp;&nbsp;
both [what] and [with] can be either constants or expression.&nbsp;&nbsp;
[with] is optional, and if not given, the [what] parts of the value of 
[replace] will simply be removed.&nbsp;&nbsp;
the value of [replace] must be an expression, and cannot be a constant.&nbsp;&nbsp;
thread safe";
				e.Params["replace"].Value = "[_expression].Value";
				e.Params["replace"]["what"].Value = "some";
				e.Params["replace"]["with"].Value = "some other";
				return;
			}

            if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.replace] directly, except for inspect purposes");

            Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

			if (!ip.Contains("what") || string.IsNullOrEmpty(ip["what"].Get<string>()))
				throw new ArgumentException("magix.nodes.replace needs [what] parameter");

			string expr = ip.Get<string>();

			if (string.IsNullOrEmpty(expr))
				throw new ArgumentException("[replace] needs an expression as value to know where to perform replacement operation");

			string exprNode = Expressions.GetExpressionValue(expr, dp, ip, false) as string;

			if (exprNode == null)
				throw new ArgumentException("couldn't find the node in [replace], expression was; " + expr);

			string replNode = Expressions.GetExpressionValue(ip["what"].Get<string>(), dp, ip, false) as string;

			string withNode = ip.Contains("with") ? 
                Expressions.GetExpressionValue(ip["with"].Get<string>(""), dp, ip, false) as string : 
                "";

			Expressions.SetNodeValue(
				expr, 
				exprNode.Replace(replNode, withNode), 
				dp, 
				ip, 
				false);
		}
	}
}

