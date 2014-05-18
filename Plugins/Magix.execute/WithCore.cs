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
	 * with hyper lisp keyword
	 */
	public class WithCore : ActiveController
	{
		/**
		 * for-each hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.with")]
		public static void magix_execute_with(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                ip["inspect"].Value = @"<p>temporary changes the data pointer to the 
expression in its value</p><p>this is useful if you're handling several keywords in a row, 
where the expressions within them are all pointing deep into the execution tree somewhere</p>
<p>thread safe</p>";
                ip["_data"]["_somewhere"]["_over"]["items"]["message1"].Value = "howdy world 1.0";
                ip["_data"]["_somewhere"]["_over"]["items"]["message2"].Value = "howdy world 2.0";
                ip["_data"]["_somewhere"]["_over"]["items"]["message3"].Value = "howdy world 3.0";
                ip["_data"]["_somewhere"]["_over"]["items"]["message4"].Value = "howdy world 4.0";
                ip["_data"]["_somewhere"]["_over"]["items"]["message5"].Value = "howdy world 5.0";
                ip["_data"]["_somewhere"]["_over"]["items"]["message6"].Value = "howdy world 6.0";
                ip["_data"]["_somewhere"]["_over"]["items"]["message7"].Value = "howdy world 7.0";
                ip["with"].Value = "[_data][_somewhere][_over]";
                ip["with"]["for-each"].Value = "[items]";
                ip["with"]["for-each"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
                ip["with"]["for-each"]["set"]["value"].Value = "[.].Value";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [with] directly, except for inspect purposes");

            Node dp = Dp(e.Params);

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [with]");

			Node withExpression = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
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

