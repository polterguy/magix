/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Threading;
using Magix.Core;

namespace Magix.execute
{
	/**
	 */
	public class SetCore : ActiveController
	{
		/**
		 * Sets given node to either the constant value
		 * of the Value of the child node called "value", a node expression found 
		 * in Value "value", or the children nodes of the "value" child, if the 
		 * left hand parts returns a node. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.set")]
		public static void magix_execute_set (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["Data"]["Children"].Value = "old value";
				e.Params["set"].Value = "[Data][Children].Value";
				e.Params["set"]["value"].Value = "new value";
				e.Params["inspect"].Value = @"Sets the given expression in the Value
of ""set"" to the Value of expression, or constant, in ""value"" node 
underneath ""set"". If you pass in no ""value"", the 
expression in Value will be nullified. Meaning, if
it's a Node-List it will be emptied and the Node
removed. If it's a Value, the value will become
null. The ""value""'s Value can be a constant,
the Value of ""set"" must be an expression.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();
			string right = null;

			if (ip.Contains ("value"))
				right = ip["value"].Get<string>();

			Expressions.SetNodeValue (left, right, dp, ip);
		}
	}
}

