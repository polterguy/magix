/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * iterate-tree hyper lisp keyword
	 */
	public class IterateTreeCore : ActiveController
	{
        /**
         * iterate-tree hyper lisp keyword
         */
        [ActiveEvent(Name = "magix.execute.iterate-tree")]
		public static void magix_execute_iterate_tree(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>loops through all the nodes in the given 
node-list expression, flattening the hierarchy, setting the data-pointer to the currently 
processed item</p><p>your code will execute once for every single node you have in your 
return expression.&nbsp;&nbsp;use the [.] expression to de-reference the currently iterated 
node</p><p>thread safe</p>";
                e.Params["_data"]["items"]["message1"].Value = "howdy world 1.0";
				e.Params["_data"]["items"]["sub-1"]["message2"].Value = "howdy world 2.0";
                e.Params["_data"]["items"]["sub-1"]["message3"].Value = "howdy world 3.0";
                e.Params["_data"]["items"]["sub-2"]["message4"].Value = "howdy world 4.0";
                e.Params["_data"]["items"]["sub-2"]["sub-3"]["message5"].Value = "howdy world 5.0";
                e.Params["_data"]["items"]["sub-3"]["message6"].Value = "howdy world 6.0";
				e.Params["_data"]["items"]["message7"].Value = "howdy world 7.0";
                e.Params["iterate-tree"].Value = "[_data][items]";
                e.Params["iterate-tree"]["if"].Value = "exist";
                e.Params["iterate-tree"]["if"]["lhs"].Value = "[.].Value";
                e.Params["iterate-tree"]["if"]["code"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
                e.Params["iterate-tree"]["if"]["code"]["set"]["value"].Value = "[.].Value";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [for-each] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [for-each]");

			Node rootExpressionNode = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
			if (rootExpressionNode != null)
			{
                object oldDp = e.Params["_dp"].Value;
                try
				{
                    IterateNode(e.Params, rootExpressionNode);
				}
                catch (Exception err)
                {
                    while (err.InnerException != null)
                        err = err.InnerException;

                    if (err is StopCore.HyperLispStopException)
                        return; // do nothing, execution stopped

                    // re-throw all other exceptions ...
                    throw;
                }
                finally
				{
					e.Params["_dp"].Value = oldDp;
				}
			}
		}

        private static void IterateNode(Node pars, Node currentlyIteratedNode)
        {
            for (int idxNo = 0; idxNo < currentlyIteratedNode.Count; idxNo++)
            {
                Node current = currentlyIteratedNode[idxNo];
                pars["_dp"].Value = current;
                RaiseActiveEvent(
                    "magix._execute",
                    pars);

                // child nodes
                IterateNode(pars, current);
            }
        }
	}
}

