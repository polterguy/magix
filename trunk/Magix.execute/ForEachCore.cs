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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["_data"]["items"]["message1"].Value = "howdy world 1.0";
				e.Params["_data"]["items"]["message2"].Value = "howdy world 2.0";
				e.Params["_data"]["items"]["message3"].Value = "howdy world 3.0";
				e.Params["_data"]["items"]["message4"].Value = "howdy world 4.0";
				e.Params["_data"]["items"]["message5"].Value = "howdy world 5.0";
				e.Params["_data"]["items"]["message6"].Value = "howdy world 6.0";
				e.Params["_data"]["items"]["message7"].Value = "howdy world 7.0";
				e.Params["for-each"].Value = "[_data][items]";
				e.Params["for-each"]["set"].Value = "[/][for-each][magix.viewport.show-message][message].Value";
				e.Params["for-each"]["set"]["value"].Value = "[.].Value";
				e.Params["inspect"].Value = @"loops through all the nodes
in the given node-list expression, setting the 
data-pointer to the currently processed item.&nbsp;&nbsp;
your code will execute once
for every single node you have in your return
expression.&nbsp;&nbsp;use the [.] expression to de-reference
the currently iterated node.&nbsp;&nbsp;thread safe";
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
				Node tmp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;

				for (int idxNo = 0; idxNo < tmp.Count; idxNo++)
				{
					Node idx = tmp[idxNo];
					Node tmp2 = new Node();
					tmp2["_ip"].Value = ip;
					tmp2["_dp"].Value = idx;

					if (e.Params.Contains("_whitelist"))
						tmp2["_whitelist"].Value = e.Params["_whitelist"].Value;
					if (e.Params.Contains("_max-cycles"))
						tmp2["_max-cycles"].Value = e.Params["_max-cycles"].Value;

					RaiseActiveEvent(
						"magix.execute", 
						tmp2);

					// Checking to see if we need to halt execution
					if (ip.Contains("_state") && ip["_state"].Get<string>() == "stop")
						break;
				}
			}
		}
	}
}

