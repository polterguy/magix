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
	/**
	 */
	public class NodeCore : ActiveController
	{
		/**
		 *
		[ActiveEvent(Name = "magix.execute.subtract")]
		public void magix_execute_subtract(object sender, ActiveEventArgs e)
		{
			if (Inspect2(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"subtracts the values from [what] node-set, 
from the values of [from], being a node expression.&nbsp;&nbsp;use [?] to give wildcard 
node name in [what]";
				e.Params["_data"]["values"]["item1"].Value = "namespace.foo";
				e.Params["_data"]["values"]["item2"].Value = "namespace.bar";
				e.Params["subtract"]["what"]["values"]["?"].Value = "namespace.";
				e.Params["subtract"]["from"].Value = "[_data]";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			if (!ip.Contains("from") || string.IsNullOrEmpty(ip["from"].Get<string>()))
				throw new ArgumentException("magix.execute.subtract needs [from] parameter");

			if (!ip.Contains("what") || (ip["what"].Count == 0 && ip["what"].Value == null))
				throw new ArgumentException("magix.execute.subtract needs [what] parameter");

			Node node = Expressions.GetExpressionValue(
				ip["from"].Get<string>(), dp, ip) as Node;

			if (node == null)
				throw new ArgumentException("couldn't find the node in [from]");

			Subtract(node, ip["what"]);
		}

		void Subtract(Node from, Node what)
		{
			if (!string.IsNullOrEmpty(what.Get<string>()))
				from.Value = from.Get<string>().Replace(what.Get<string>(), "");

			foreach (Node idx in from)
			{
				if (what.Contains("?") || what.Contains(idx.Name))
					Subtract(idx, what.Contains(idx.Name) ? what[idx.Name] : what["?"]);
			}
		}*/
	}
}

