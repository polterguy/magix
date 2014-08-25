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
     * hyperlisp switch logic
     */
    public class SwitchCore : ActiveController
    {
        /*
         * switch hyperlisp keyword
         */
        [ActiveEvent(Name = "magix.execute.switch")]
        public static void magix_execute_switch(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.switch-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.switch-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.Contains("case"))
                throw new HyperlispSyntaxErrorException("[switch] needs at least one [case] value");

            string value = Expressions.GetExpressionValue<string>(ip.Get<string>(), dp, ip, false);
            ExecuteSwitch(e.Params, ip, dp, value);
        }

        /*
         * actual implementation of [switch]
         */
        private static void ExecuteSwitch(Node pars, Node ip, Node dp, string value)
        {
            // verifying all case blocks have unique values
            VerifyUniqueCase(dp, ip);

            // looping through all [case] keywords to look for a match
            bool foundMatch = false;
            foreach (Node idx in ip)
            {
                if (idx.Name == "case")
                {
                    if (value == Expressions.GetExpressionValue<string>(idx.Get<string>(), dp, ip, false))
                    {
                        // we have a match in one of our [case] keywords
                        foundMatch = true;
                        pars["_ip"].Value = idx;
                        RaiseActiveEvent(
                            "magix.execute",
                            pars);
                        break;
                    }
                }
                else if (idx.Name != "default")
                    throw new ArgumentException("unknown keyword found in [switch] statement");
            }
            if (!foundMatch && ip.Contains("default"))
            {
                // executing [default] since no case matched
                pars["_ip"].Value = ip["default"];
                RaiseActiveEvent(
                    "magix.execute",
                    pars);
            }
        }

        /*
         * verifies all [case] blocks have unique values
         */
        private static void VerifyUniqueCase(Node dp, Node ip)
        {
            List<string> caseValues = new List<string>();
            foreach (Node idx in ip)
            {
                if (idx.Name == "case")
                {
                    string valueOfCase = Expressions.GetExpressionValue<string>(idx.Get<string>(), dp, ip, false);
                    if (caseValues.Contains(valueOfCase))
                        throw new HyperlispExecutionErrorException("[switch] contained two or more similar [case] values");
                    caseValues.Add(valueOfCase);
                }
            }
        }
    }
}

