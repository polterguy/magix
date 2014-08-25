/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.execute
{
    /*
     * helper for manipulating nodes
     */
    public class ReplaceCore : ActiveController
    {
        /*
         * replaces node's contents
         */
        [ActiveEvent(Name = "magix.execute.replace")]
        public void magix_execute_replace(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.replace-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.replace-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // verifying a [what] is given, and that it has a value
            if (!ip.ContainsValue("what") || ip["what"].Get<string>().Length == 0)
                throw new HyperlispSyntaxErrorException("[replace] needs a [what] parameter");

            // where to put our updated string
            string destinationExpression = ip.Get<string>();
            if (string.IsNullOrEmpty(destinationExpression))
                throw new ArgumentException("[replace] needs an expression as a value to know which string to replace");

            // current value of string to replace
            string sourceValue = Expressions.GetExpressionValue<string>(destinationExpression, dp, ip, false);
            if (string.IsNullOrEmpty(sourceValue))
                return; // the string user wanted to replace was empty or null

            // doing actual replacement
            Replace(e.Params, ip, dp, destinationExpression, sourceValue);
        }

        /*
         * actual implementation being executed after verifying integrity of keyword
         */
        private static void Replace(Node pars, Node ip, Node dp, string destinationExpression, string sourceValue)
        {
            // what to replace in our source
            string what = Expressions.GetFormattedExpression("what", pars, "");
            what = what.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");

            // what to replace it with in our source
            string with = Expressions.GetFormattedExpression("with", pars, "");
            with = with.Replace("\\n", "\n").Replace("\\r", "\r").Replace("\\t", "\t");

            // the actual replacement operation
            Expressions.SetNodeValue(
                destinationExpression,
                sourceValue.Replace(what, with),
                dp,
                ip,
                false);
        }
    }
}

