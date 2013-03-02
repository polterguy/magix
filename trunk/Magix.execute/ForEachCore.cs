/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Contains logic for "for-each" keyword
	 */
	public class ForEachCore : ActiveController
	{
		/**
		 * Will loop through all the given 
		 * node's Value Expression, and execute the underlaying code, once for 
		 * all nodes in the returned expression, with the Data-Pointer pointing
		 * to the index node currently being looped through. Is a "magix.execute"
		 * keyword. Functions as a "magix.execute" keyword
		 */
		[ActiveEvent(Name = "magix.execute.for-each")]
		public static void magix_execute_for_each(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["Data"]["Items"]["Item1"]["message"].Value = "Howdy World 1.0!";
				e.Params["Data"]["Items"]["Item2"]["message"].Value = "Howdy World 2.0!";
				e.Params["for-each"].Value = "[Data][Items]";
				e.Params["for-each"]["set"].Value = "[/][for-each][magix.viewport.show-message][message].Value";
				e.Params["for-each"]["set"]["value"].Value = "[.][message].Value";
				e.Params["for-each"]["magix.viewport.show-message"].Value = null;
				e.Params["inspect"].Value = @"loops through all the nodes
in the given node-list expression, setting the 
data-pointer to the currently processed item.&nbsp;&nbsp;
your code will execute once
for every single node you have in your return
expression.&nbsp;&nbsp;use the [.] expression to de-reference
the currently iterated node";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			if (Expressions.IsTrue(ip.Get<string>(), ip, dp))
			{
				Node tmp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip) as Node;

				// To make sure we can CHANGE the list of nodes, as we iterate them
				// and remove, add etc ...
				List<Node> tmpList = new List<Node>(tmp.Children);
				foreach (Node idx in tmpList)
				{
					Node tmp2 = new Node();
					tmp2["_ip"].Value = ip;
					tmp2["_dp"].Value = idx;

					RaiseEvent(
						"magix.execute", 
						tmp2);
				}
			}
		}
	}
}

