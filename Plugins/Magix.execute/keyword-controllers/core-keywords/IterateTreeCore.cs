/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * iterate-tree hyperlisp keyword
	 */
	public class IterateTreeCore : ActiveController
	{
        /*
         * iterate-tree hyperlisp keyword
         */
        [ActiveEvent(Name = "magix.execute.iterate")]
		public static void magix_execute_iterate(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.iterate-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.iterate-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [iterate]");

			Node rootExpressionNode = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, false);
			if (rootExpressionNode != null)
			{
                object oldDp = e.Params["_dp"].Value;
                try
				{
                    IterateNode(e.Params, rootExpressionNode, ip);
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

        private static void IterateNode(Node pars, Node currentlyIteratedNode, Node ip)
        {
            if (currentlyIteratedNode.Count > 0)
            {
                Node curIdx = currentlyIteratedNode[0];
                while (curIdx != null)
                {
                    Node oldIp = ip.Clone();

                    pars["_dp"].Value = curIdx;

                    Node nextIdx = curIdx.Next();

                    RaiseActiveEvent(
                        "magix.execute",
                        pars);

                    ip.Clear();
                    ip.AddRange(oldIp);

                    // child nodes
                    IterateNode(pars, curIdx, ip);

                    curIdx = nextIdx;
                }
            }
        }
	}
}

