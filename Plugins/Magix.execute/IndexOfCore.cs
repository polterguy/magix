/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                ip["inspect"].Value = @"<p>returns the index of all occurences of 
the [what] constant or expression as nodes containing the integer value of where match 
was found underneath the [result] return node</p><p>thread safe</p>";
                ip["_data"].Value = "this will return the position of all 'r' characters in string";
				ip["index-of"].Value = "[_data].Value";
				ip["index-of"]["what"].Value = "r";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [index-of] directly, except for inspect purposes");

            Node dp = Dp(e.Params);

            if (!ip.Contains("what"))
                throw new ArgumentException("[index-of] needs a [what] child to understand what to search for");

            string whatToSearch = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
            if (whatToSearch == null)
                throw new ArgumentException("couldn't make '" + ip.Get<string>() + "' into a string in [index-of]");

            string whatToSearchFor = Expressions.GetExpressionValue(ip["what"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(whatToSearchFor))
                throw new ArgumentException("no value in [what] expression in [magix.execute.index-of]");

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

