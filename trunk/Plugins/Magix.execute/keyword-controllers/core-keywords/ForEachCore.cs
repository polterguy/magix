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
	/**
	 * for-each hyperlisp keyword
	 */
	public class ForEachCore : ActiveController
	{
		/**
		 * for-each hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.for-each")]
		public static void magix_execute_for_each(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.for-each-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.for-each-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [for-each]");

			Node tmp = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, false);
			if (tmp != null && tmp.Count > 0)
			{
                object oldDp = e.Params["_dp"].Value;
                try
				{
                    Node curIdx = tmp[0];
					while (curIdx != null)
					{
                        Node oldIp = ip.Clone();
						e.Params["_dp"].Value = curIdx;

                        Node nextIdx = curIdx.Next();
                        
                        RaiseActiveEvent(
							"magix.execute", 
							e.Params);

                        ip.Clear();
                        ip.AddRange(oldIp);

                        curIdx = nextIdx;
					}
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
	}
}

