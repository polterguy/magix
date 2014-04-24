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
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"[switch] creates a comparison 
between all the children [case] nodes, and executes 
the code within the [case] node who's value equals 
the content of the [switch] node.&nbsp;&nbsp;
the [switch] node's value can be either a constant 
or an expression.&nbsp;&nbsp;
if no match is found, then [default] will be
executed, if it exist.&nbsp;&nbsp;
notice how both the [switch] node itself, 
and [case] nodes can be expressions.
&nbsp;&nbsp;thread safe";
                e.Params["_data"].Value = "3";
                e.Params["_success"].Value = "3";
                e.Params["switch"].Value = "[_data].Value";
                e.Params["switch"]["case", 0].Value = "1";
                e.Params["switch"]["case", 0]["magix.viewport.show-message"]["message"].Value = "ERROR!!";
                e.Params["switch"]["case", 1].Value = "2";
                e.Params["switch"]["case", 1]["magix.viewport.show-message"]["message"].Value = "ERROR!!";
                e.Params["switch"]["case", 2].Value = "[_success].Value";
                e.Params["switch"]["case", 2]["magix.viewport.show-message"]["message"].Value = "success!";
                e.Params["switch"]["default"]["magix.viewport.show-message"]["message"].Value = "ERROR!!";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.switch] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			if (!ip.Contains("case"))
				throw new ArgumentException("[switch] needs at least one [case] value");

			string value = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false).ToString();
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
                        Node tmp = new Node();
                        tmp["_ip"].Value = idx;
                        tmp["_dp"].Value = dp;

                        RaiseActiveEvent(
                            "magix._execute",
                            tmp);
                    }
                }
                else if (idx.Name != "default")
                    throw new ArgumentException("unknown keyword found in [switch] statement");
            }
            if (!foundMatch && ip.Contains("default"))
            {
                Node tmp = new Node();
                tmp["_ip"].Value = ip["default"];
                tmp["_dp"].Value = dp;

                RaiseActiveEvent(
                    "magix._execute",
                    tmp);
            }
		}
    }
}

