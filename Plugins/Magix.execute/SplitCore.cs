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
	 * hyper lisp split keyword
	 */
	public class SplitCore : ActiveController
	{
		/**
		 * hyepr lisp split keyword
		 */
		[ActiveEvent(Name = "magix.execute.split")]
		public static void magix_execute_split(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["_data"].Value = "some text which will be split for every space";
				e.Params["split"].Value = "[_data].Value";
				e.Params["split"]["what"].Value = " ";
                e.Params["inspect"].Value = @"splits the given expression in the value
of [split] according to the [what] content or [where] child nodes, and puts the result 
into the [result] return node as children nodes.&nbsp;&nbsp;
if [what] is given but has null or empty value, then string 
will be split for every single character in it.&nbsp;&nbsp;
if [where] is used, then either value of [where] is expected to 
be an integer telling at what index the string should be split, 
or a list of child nodes containing indexes for where to split the string.&nbsp;&nbsp;
both [what] and [where] value or nodes can be either a constant, 
or expression(s).&nbsp;&nbsp;
if you use [what] to split your string, then the parts 
which matches the [what] expression in your string, 
will be removed from the result set.&nbsp;&nbsp;
thread safe";
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.split] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = ip;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("what") && !ip.Contains("where"))
                throw new ArgumentException("[split] needs a [what] or a [where] child to understand how to split the expression");

            if (ip.Contains("what") && ip.Contains("where"))
                throw new ArgumentException("only either [what] or [where] can be submitted to [magix.execute.split]");

            string whatToSplit = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
            if (whatToSplit == null)
                throw new ArgumentException("couldn't make '" + ip.Get<string>() + "' into a string in [magix.execute.split]");

            if (ip.Contains("what"))
            {
                string what = null;
                if (ip.Contains("what"))
                {
                    what = Expressions.GetExpressionValue(ip["what"].Get<string>(), dp, ip, false) as string;
                }

                if (string.IsNullOrEmpty(what))
                {
                    foreach (char idx in whatToSplit)
                    {
                        ip["result"].Add(new Node("", idx.ToString()));
                    }
                }
                else
                {
                    string[] splits = whatToSplit.Split(new string[] { what }, StringSplitOptions.RemoveEmptyEntries);

                    foreach (string idx in splits)
                    {
                        ip["result"].Add(new Node("", idx));
                    }
                }
            }
            else
            {
                // where
                if (ip["where"].Value != null && ip["where"].Count > 0)
                    throw new ArgumentException("either supply an integer as value of [where] or supply a list of integer values as children of [where], not both");
                
                List<int> ints = new List<int>();
                if (ip["where"].Value != null)
                    ints.Add(
                        int.Parse(Expressions.GetExpressionValue(ip["where"].Get<string>(), dp, ip, false) as string, 
                        CultureInfo.InvariantCulture));
                foreach (Node idx in ip["where"])
                {
                    ints.Add(
                        int.Parse(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false) as string,
                        CultureInfo.InvariantCulture));
                }

                int idxNo = 0;
                foreach (int idx in ints)
                {
                    ip["result"].Add(new Node("", whatToSplit.Substring(idxNo, idx - idxNo)));
                    idxNo = idx;
                }
                ip["result"].Add(new Node("", whatToSplit.Substring(idxNo)));
            }
		}
	}
}

