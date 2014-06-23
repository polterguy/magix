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

            if (!ip.Contains("what"))
                throw new ArgumentException("[index-of] needs a [what] child to understand what to search for");

            Node dp = Dp(e.Params);
            string whatToSearch = Expressions.GetExpressionValue<string>(ip.Get<string>(), dp, ip, false);
            if (whatToSearch == null)
                throw new ArgumentException("couldn't make '" + ip.Get<string>() + "' into a string in [index-of]");

            string whatToSearchFor = Expressions.GetExpressionValue<string>(ip["what"].Get<string>(), dp, ip, false);
            if (string.IsNullOrEmpty(whatToSearchFor))
                throw new ArgumentException("no value in [what] expression in [index-of]");

            ip["result"].UnTie();

            int idxNo = 0;
            while (true)
            {
                idxNo = whatToSearch.IndexOf(whatToSearchFor, idxNo);
                if (idxNo == -1)
                    break;
                ip["result"].Add(new Node("", idxNo++));
            }
		}
	}
}

