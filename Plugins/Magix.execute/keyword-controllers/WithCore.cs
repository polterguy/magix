/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * with hyperlisp keyword
	 */
	public class WithCore : ActiveController
	{
		/**
		 * for-each hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.with")]
		public static void magix_execute_with(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.with-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.with-sample]");
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [with] directly, except for inspect purposes");

            Node dp = Dp(e.Params);

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [with]");

			Node withExpression = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, true) as Node;
            if (withExpression == null)
                throw new ArgumentException("the expression in your [with] statement returned nothing");

            object oldDp = e.Params["_dp"].Value;
            try
			{
                e.Params["_dp"].Value = withExpression;
				RaiseActiveEvent(
					"magix.execute", 
					e.Params);
			}
            finally
			{
				e.Params["_dp"].Value = oldDp;
			}
		}
	}
}

