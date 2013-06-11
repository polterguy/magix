/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.node
{
	public class NodeCore : ActiveController
	{
		[ActiveEvent(Name = "magix.execute.replace")]
		public void magix_execute_replace_node_value(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["_expression"].Value = "some value to be replaced";
				e.Params["inspect"].Value = @"replaces [replace] value with [with] in 
the [where] node expression.&nbsp;&nbsp;
expression must end with .Value or .Name.&nbsp;&nbsp;thread safe";
				e.Params["replace"].Value = "[_expression].Value";
				e.Params["replace"]["replace"].Value = "some";
				e.Params["replace"]["with"].Value = "some other";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			if (!ip.Contains("replace") || string.IsNullOrEmpty(ip["replace"].Get<string>()))
				throw new ArgumentException("magix.nodes.replace needs [replace] parameter");

			string expr = ip.Get<string>();

			string exprNode = Expressions.GetExpressionValue(expr, dp, ip, false) as string;

			if (exprNode == null)
				throw new ArgumentException("couldn't find the node in [replace]");

			Expressions.SetNodeValue(
				expr, 
				exprNode.Replace(ip["replace"].Get<string>(), ip["with"].Get<string>("")), 
				dp, 
				ip, 
				false);
		}
	}
}

