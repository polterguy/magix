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
	/**
	 * hyper lisp index-of keyword
	 */
	public class IndexOfCore : ActiveController
	{
		/**
		 * hyepr lisp index-of keyword
		 */
		[ActiveEvent(Name = "magix.execute.index-of")]
		public static void magix_execute_index_of(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["_data"].Value = "this will return the position of all 'r' characters in string";
				e.Params["index-of"].Value = "[_data].Value";
				e.Params["index-of"]["what"].Value = "r";
                e.Params["inspect"].Value = @"returns the index of all occurences of 
the [what] constant or expression as nodes containing 
the integer value of where match was found 
underneath the [result] return node.&nbsp;&nbsp;
thread safe";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.index-of] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("what"))
                throw new ArgumentException("[index-of] needs a [what] child to understand what to search for");

            string whatToSplit = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
            if (whatToSplit == null)
                throw new ArgumentException("couldn't make '" + ip.Get<string>() + "' into a string in [magix.execute.index-of]");

            string what = Expressions.GetExpressionValue(ip["what"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(what))
                throw new ArgumentException("no value in [what] expression in [magix.execute.index-of]");

            int idxNo = 0;
            while (true)
            {
                idxNo = whatToSplit.IndexOf(what, idxNo);
                if (idxNo == -1)
                    break;
                ip["result"].Add(new Node("", idxNo++));
            }
		}
	}
}

