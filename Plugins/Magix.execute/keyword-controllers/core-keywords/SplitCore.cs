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

            if (!ip.Contains("what") && !ip.Contains("where"))
                throw new ArgumentException("[split] needs a [what] or a [where] child to understand how to split the expression");

            if (ip.Contains("what") && ip.Contains("where"))
                throw new ArgumentException("only either [what] or [where] can be submitted to [split]");

            Node dp = Dp(e.Params);
            string whatToSplit = Expressions.GetExpressionValue<string>(ip.Get<string>(), dp, ip, false);
            if (whatToSplit == null)
                throw new ArgumentException("couldn't make '" + ip.Get<string>() + "' into a string in [split]");

            if (ip.ContainsValue("trim") && ip["trim"].Get<bool>())
                whatToSplit = whatToSplit.Trim();

            ip["result"].UnTie();

            if (ip.Contains("what"))
            {
                string what = null;
                if (ip.Contains("what"))
                    what = Expressions.GetExpressionValue<string>(ip["what"].Get<string>(), dp, ip, false);

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
                        Expressions.GetExpressionValue<int>(ip["where"].Get<string>(), dp, ip, false));
                foreach (Node idx in ip["where"])
                {
                    ints.Add(Expressions.GetExpressionValue<int>(idx.Get<string>(), dp, ip, false));
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

