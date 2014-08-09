/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * hyperlisp add logic
	 */
	public class SortCore : ActiveController
	{
		/*
		 * add hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.sort")]
		public static void magix_execute_sort(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sort-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sort-sample]");
                return;
			}

			Node dp = Dp(e.Params);

            Node nodes = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, true);
            if (nodes == null)
                throw new ArgumentException("[sort] must return an existing node-list, [sort] value returned null, expression was; " + ip.Get<string>());

            if (nodes != null && nodes.Count > 0)
            {
                try
                {
                    nodes.Sort(
                        delegate(Node lhs, Node rhs)
                        {
                            Node oldIp = ip.Clone();
                            ip["_first"].Add(lhs.Clone());
                            ip["_second"].Add(rhs.Clone());

                            RaiseActiveEvent(
                                "magix.execute",
                                e.Params);

                            int retVal = 0;
                            if (ip.Contains("_result"))
                            {
                                if (ip["_result"][0][0].Equals(lhs))
                                    retVal = -1;
                                else
                                    retVal = 1;
                            }
                            ip.Clear();
                            ip.AddRange(oldIp);
                            return retVal;
                        });
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
            }
        }
    }
}

