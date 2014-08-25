/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Globalization;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
    /*
     * hyperlisp split keyword
     */
    public class SplitCore : ActiveController
    {
        /*
         * hyper lisp split keyword
         */
        [ActiveEvent(Name = "magix.execute.split")]
        public static void magix_execute_split(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.split-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.split-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // verifying syntax is correct
            if (!ip.Contains("what") && !ip.Contains("where") && !ip.Contains("trim"))
                throw new HyperlispSyntaxErrorException("[split] needs a [what], [trim] or a [where] child to understand how to split the expression");
            if (ip.Contains("what") && ip.Contains("where"))
                throw new HyperlispSyntaxErrorException("only either [what] or [where] can be submitted to [split], and not both");

            string whatToSplit = Expressions.GetExpressionValue<string>(ip.Get<string>(), dp, ip, false);
            if (whatToSplit == null)
                throw new HyperlispExecutionErrorException("couldn't make '" + ip.Get<string>() + "' into a string in [split]");

            // running actual split operation
            Split(ip, dp, whatToSplit);
        }

        /*
         * actual split implementation
         */
        private static void Split(Node ip, Node dp, string whatToSplit)
        {
            // trimming first, if we should
            if (ip.ContainsValue("trim") && ip["trim"].Get<string>().ToLower() == "true")
                whatToSplit = whatToSplit.Trim();
            else if (ip.ContainsValue("trim"))
                whatToSplit = whatToSplit.Trim(ip["trim"].Get<string>().ToCharArray());

            if (ip.Contains("what"))
                SplitByStringValue(ip, dp, whatToSplit);
            else if (ip.Contains("where"))
                SplitByIndex(ip, dp, whatToSplit);
            else
                ip["result"].Add(new Node("", whatToSplit)); // probably just a trimming operation
        }

        /*
         * splits by string value
         */
        private static void SplitByStringValue(Node ip, Node dp, string whatToSplit)
        {
            string what = null;
            if (ip.Contains("what"))
                what = Expressions.GetExpressionValue<string>(ip["what"].Get<string>(), dp, ip, false);

            if (string.IsNullOrEmpty(what))
            {
                // splits every single character in string
                foreach (char idx in whatToSplit)
                {
                    ip["result"].Add(new Node("", idx.ToString()));
                }
            }
            else
            {
                // splits by string value
                string[] splits = whatToSplit.Split(new string[] { what }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string idx in splits)
                {
                    ip["result"].Add(new Node("", idx));
                }
            }
        }

        /*
         * splits by index
         */
        private static void SplitByIndex(Node ip, Node dp, string whatToSplit)
        {
            if (ip["where"].Value != null && ip["where"].Count > 0)
                throw new HyperlispSyntaxErrorException("either supply an integer as value of [where] or supply a list of integer values as children of [where], and not both");

            List<int> integerIndexes = new List<int>();
            if (ip["where"].Value != null)
                integerIndexes.Add(Expressions.GetExpressionValue<int>(ip["where"].Get<string>(), dp, ip, false));
            else
            {
                foreach (Node idx in ip["where"])
                {
                    integerIndexes.Add(Expressions.GetExpressionValue<int>(idx.Get<string>(), dp, ip, false));
                }
            }

            int idxNo = 0;
            foreach (int idxInteger in integerIndexes)
            {
                if (idxInteger >= whatToSplit.Length)
                {
                    // last occurrency
                    ip["result"].Add(new Node("", whatToSplit.Substring(idxNo)));
                    return;
                }
                ip["result"].Add(new Node("", whatToSplit.Substring(idxNo, idxInteger - idxNo)));
                idxNo = idxInteger;
            }
            ip["result"].Add(new Node("", whatToSplit.Substring(idxNo)));
        }
    }
}

