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
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>temporary changes the data pointer to the 
expression in its value</p><p>this is useful if you're handling several keywords in a row, 
where the expressions within them are all pointing deep into the execution tree somewhere</p>
<p>thread safe</p>";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message1"].Value = "howdy world 1.0";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message2"].Value = "howdy world 2.0";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message3"].Value = "howdy world 3.0";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message4"].Value = "howdy world 4.0";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message5"].Value = "howdy world 5.0";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message6"].Value = "howdy world 6.0";
                e.Params["_data"]["_somewhere"]["_over"]["items"]["message7"].Value = "howdy world 7.0";
                e.Params["with"].Value = "[_data][_somewhere][_over]";
                e.Params["with"]["for-each"].Value = "[items]";
                e.Params["with"]["for-each"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
                e.Params["with"]["for-each"]["set"]["value"].Value = "[.].Value";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [with] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [with]");

			Node tmp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
            if (tmp == null)
                throw new ArgumentException("the expression in your [with] statement returned nothing");

            object oldDp = e.Params["_dp"].Value;
            try
			{
				for (int idxNo = 0; idxNo < tmp.Count; idxNo++)
				{
					e.Params["_dp"].Value = tmp;
					RaiseActiveEvent(
						"magix._execute", 
						e.Params);
				}
			}
            finally
			{
				e.Params["_dp"].Value = oldDp;
			}
		}
	}
}

