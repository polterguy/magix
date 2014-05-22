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
                    "[magix.execute.for-each-dox].Value");
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

			Node tmp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
			if (tmp != null)
			{
                object oldDp = e.Params["_dp"].Value;
                try
				{
					for (int idxNo = 0; idxNo < tmp.Count; idxNo++)
					{
						e.Params["_dp"].Value = tmp[idxNo];
						RaiseActiveEvent(
							"magix.execute", 
							e.Params);
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

