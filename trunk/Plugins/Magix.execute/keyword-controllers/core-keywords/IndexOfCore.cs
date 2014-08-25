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
     * hyperlisp index-of keyword
     */
    public class IndexOfCore : ActiveController
    {
        /*
         * hyperlisp index-of keyword
         */
        [ActiveEvent(Name = "magix.execute.index-of")]
        public static void magix_execute_index_of(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.index-of-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.index-of-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            // retrieving the string to search through
            string stringToSearch = Expressions.GetExpressionValue<string>(ip.Get<string>(), dp, ip, false);
            if (stringToSearch == null)
                throw new HyperlispExecutionErrorException("couldn't make '" + ip.Get<string>() + "' into a string in [index-of]");

            // retrieving what to search for in our string
            string whatToSearchFor = Expressions.GetFormattedExpression("what", e.Params, null);
            if (whatToSearchFor == null)
                throw new HyperlispExecutionErrorException("no value in [what] expression in [index-of]");

            // checking to see if we're supposed to search case insensitive
            bool caseSensitive = ip.GetValue("case", true);

            // return results back to caller
            RetrieveResults(ip, stringToSearch, whatToSearchFor, caseSensitive);
        }

        /*
         * actual implementation of index-of
         */
        private static void RetrieveResults(Node ip, string stringToSearch, string whatToSearchFor, bool caseSensitive)
        {
            int idxNo = 0;
            while (true)
            {
                if (!caseSensitive)
                    idxNo = stringToSearch.IndexOf(whatToSearchFor, idxNo, StringComparison.OrdinalIgnoreCase);
                else
                    idxNo = stringToSearch.IndexOf(whatToSearchFor, idxNo);
                if (idxNo == -1)
                    break;
                ip["result"].Add(new Node("", idxNo++));
            }
        }
    }
}

