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
                    "[magix.execute.while-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.while-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // verifying syntax is correct
            if (!ip.Contains("code"))
                throw new HyperlispSyntaxErrorException("you need to supply a [code] block to the [while] keyword");

            // executing [while]
            WhileChecked(e.Params, ip, dp);
        }

        /*
         * wraps while in a try
         */
        private static void WhileChecked(Node pars, Node ip, Node dp)
        {
            try
            {
                WhileUnchecked(pars, ip, dp);
            }
            catch (Exception err)
            {
                while (err.InnerException != null)
                    err = err.InnerException;

                if (err is HyperlispStopException)
                    return; // do nothing, execution stopped

                // re-throw all other exceptions ...
                throw;
            }
        }

        /*
         * actual while implementation
         */
        private static void WhileUnchecked(Node pars, Node ip, Node dp)
        {
            // looping as long as statement is true
            while (StatementHelper.CheckExpressions(ip, dp))
            {
                Node oldIp = ip["code"].Clone();
                pars["_ip"].Value = ip["code"];

                RaiseActiveEvent(
                    "magix.execute",
                    pars);

                ip["code"].Clear();
                ip["code"].AddRange(oldIp);
            }
        }
    }
}

