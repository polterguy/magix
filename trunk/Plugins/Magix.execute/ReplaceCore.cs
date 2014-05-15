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
	/**
	 * helper for manipulating nodes
	 */
	public class ReplaceCore : ActiveController
	{
		/**
		 * replaces node's contents
		 */
		[ActiveEvent(Name = "magix.execute.replace")]
		public void magix_execute_replace(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                ip["inspect"].Value = @"<p>replaces the [what] value/expression with the [with] 
value/expression in the value of the [replace] node's expression</p><p>both [what] and [with] can be 
either constants or expressions.&nbsp;&nbsp;[with] is optional, and if not given, the [what] parts of 
the value of [replace] will simply be removed.&nbsp;&nbsp;the value of [replace] can be either a constant,
or an expression</p><p>thread safe</p>";
                ip["_expression"].Value = "some value to be replaced";
				ip["replace"].Value = "[_expression].Value";
				ip["replace"]["what"].Value = "some";
				ip["replace"]["with"].Value = "some other";
				return;
			}

            if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [replace] directly, except for inspect purposes");

			Node dp = e.Params["_dp"].Value as Node;
			if (!ip.Contains("what") || string.IsNullOrEmpty(ip["what"].Get<string>()))
				throw new ArgumentException("[replace] needs a [what] parameter");

			string destinationExpression = ip.Get<string>();
			if (string.IsNullOrEmpty(destinationExpression))
				throw new ArgumentException("[replace] needs an expression or a constant as a value to know where to replace");

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

