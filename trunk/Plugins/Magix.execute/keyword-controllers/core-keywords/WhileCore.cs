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
	 * while keyword
	 */
	public class WhileCore : ActiveController
	{
		/*
		 * while keyword
		 */
		[ActiveEvent(Name = "magix.execute.while")]
		public static void magix_execute_while(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.while-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.while-sample]");
                return;
			}

            if (!ip.Contains("code"))
                throw new ArgumentException("you need to supply a [code] block to the [while] active event");

            Node dp = Dp(e.Params);
			while (StatementHelper.CheckExpressions(ip, dp))
			{
                e.Params["_ip"].Value = ip["code"];
                try
                {
                    Node oldIp = ip["code"].Clone();

                    RaiseActiveEvent(
                        "magix.execute",
                        e.Params);

                    ip["code"].Clear();
                    ip["code"].AddRange(oldIp);
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

