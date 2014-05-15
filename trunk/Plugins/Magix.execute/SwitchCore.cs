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
	 * hyper lisp switch logic
	 */
	public class SwitchCore : ActiveController
	{
		/**
		 * switch hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.switch")]
		public static void magix_execute_switch(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>[switch] creates a comparison between all the 
children [case] nodes, and executes the code within the [case] node who's value equals the content 
of the [switch] node</p><p>the [switch] node's value can be either a constant or an expression.
&nbsp;&nbsp;if no match is found, then [default] will be executed, if it exist.&nbsp;&nbsp;notice 
how both the [switch] node itself, and [case] nodes can be expressions</p><p>&nbsp;&nbsp;thread 
safe</p>";
                ip["_data"].Value = "3";
                ip["_success"].Value = "3";
                ip["switch"].Value = "[_data].Value";
                ip["switch"]["case", 0].Value = "1";
                ip["switch"]["case", 0]["magix.viewport.show-message"]["message"].Value = "ERROR!!";
                ip["switch"]["case", 1].Value = "2";
                ip["switch"]["case", 1]["magix.viewport.show-message"]["message"].Value = "ERROR!!";
                ip["switch"]["case", 2].Value = "[_success].Value";
                ip["switch"]["case", 2]["magix.viewport.show-message"]["message"].Value = "success!";
                ip["switch"]["default"]["magix.viewport.show-message"]["message"].Value = "ERROR!!";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [switch] directly, except for inspect purposes");

			Node dp = e.Params["_dp"].Value as Node;

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

