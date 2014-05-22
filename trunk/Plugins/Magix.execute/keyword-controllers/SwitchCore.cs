/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * hyperlisp switch logic
	 */
	public class SwitchCore : ActiveController
	{
		/**
		 * switch hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.switch")]
		public static void magix_execute_switch(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.switch-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.switch-sample]");
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [switch] directly, except for inspect purposes");

            Node dp = Dp(e.Params);

			if (!ip.Contains("case"))
				throw new ArgumentException("[switch] needs at least one [case] value");

			string value = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
            if (value == null)
                throw new ArgumentException("[switch] statement value was null");

            bool foundMatch = false;
            foreach (Node idx in ip)
            {
                if (idx.Name == "case")
                {
                    if (value == Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false).ToString())
                    {
                        foundMatch = true;
                        e.Params["_ip"].Value = idx;

                        RaiseActiveEvent(
                            "magix.execute",
                            e.Params);
                    }
                }
                else if (idx.Name != "default")
                    throw new ArgumentException("unknown keyword found in [switch] statement");
            }
            if (!foundMatch && ip.Contains("default"))
            {
                e.Params["_dp"].Value = dp;

                RaiseActiveEvent(
                    "magix.execute",
                    e.Params);
            }
		}
    }
}

