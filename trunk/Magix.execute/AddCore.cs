/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Contains active events for handling "add" keyword
	 */
	public class AddCore : ActiveController
	{
		/**
		 * Adds the given "value" Node to the Value Expression, which both must be a Node list
		 */
		[ActiveEvent(Name = "magix.execute.add")]
		public static void magix_execute_add(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"creates a copy of the node returned by the 
expression in the [value] node, and appends it into the 
node-expression found in [add].&nbsp;&nbsp;
the entire node will be copied,
with its children and sub-nodes";
				e.Params["Data"]["Children"].Value = "Original";
				e.Params["Data"]["Children1"].Value = "Will be copied";
				e.Params["Data"]["Children1"]["Item1"].Value = "Will be copied too";
				e.Params["Data"]["Children1"]["Item2"].Value = "Will also be copied";
				e.Params["add"].Value = "[Data][Children]";
				e.Params["add"]["value"].Value = "[Data][Children1]";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();

			if (!ip.Contains("value"))
				throw new ArgumentException("Cannot add a null node, need value Node to declare which node to add");

			string right = ip["value"].Get<string>();
			Node leftNode = Expressions.GetExpressionValue (left, dp, ip) as Node;
			Node rightNode = Expressions.GetExpressionValue (right, dp, ip) as Node;

			if (leftNode == null || rightNode == null)
				throw new ArgumentException("Both Value and 'value' must return an existing Node-List");

			leftNode.Add (rightNode.Clone ());
		}
	}
}

