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
     * with hyperlisp keyword
     */
    public class WithCore : ActiveController
    {
        /*
         * with hyperlisp keyword
         */
        [ActiveEvent(Name = "magix.execute.with")]
        public static void magix_execute_with(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.with-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.with-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // retrieving nodes to execute inner code with as data-pointer
            Node withNodeList = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, true);
            if (withNodeList == null)
                throw new HyperlispExecutionErrorException("your [with] keyword needs to return an existing node-list, the expression was; '" + ip.Get<string>() + "'");

            // executes with
            ExecuteWith(e.Params, withNodeList);
        }

        /*
         * actual implementation of [with]
         */
        private static void ExecuteWith(Node pars, Node withNodeList)
        {
            object oldDp = pars["_dp"].Value;
            try
            {
                pars["_dp"].Value = withNodeList;
                RaiseActiveEvent(
                    "magix.execute",
                    pars);
            }
            finally
            {
                pars["_dp"].Value = oldDp;
            }
        }
    }
}

