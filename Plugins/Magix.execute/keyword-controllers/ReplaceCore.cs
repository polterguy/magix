/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
                    "[magix.execute.replace-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.replace-sample]");
                return;
			}

			if (!ip.Contains("what") || string.IsNullOrEmpty(ip["what"].Get<string>()))
				throw new ArgumentException("[replace] needs a [what] parameter");

			string destinationExpression = ip.Get<string>();
			if (string.IsNullOrEmpty(destinationExpression))
				throw new ArgumentException("[replace] needs an expression or a constant as a value to know where to replace");

            Node dp = Dp(e.Params);
			string sourceExpressionValue = Expressions.GetExpressionValue(destinationExpression, dp, ip, false) as string;
            if (sourceExpressionValue == null)
                return; // the string user wanted to replace was null

			string what = Expressions.GetExpressionValue(ip["what"].Get<string>(), dp, ip, false) as string;

			string with = ip.Contains("with") ? 
                Expressions.GetExpressionValue(ip["with"].Get<string>(""), dp, ip, false) as string : 
                "";

            if (ip.Get<string>().IndexOf('[') == 0)
            {
                Expressions.SetNodeValue(
                    destinationExpression,
                    sourceExpressionValue.Replace(what, with),
                    dp,
                    ip,
                    false);
            }
            else
            {
                ip.Value = sourceExpressionValue.Replace(what, with);
            }
		}
	}
}

