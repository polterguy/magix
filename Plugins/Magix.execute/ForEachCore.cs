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
	 * for-each hyper lisp keyword
	 */
	public class ForEachCore : ActiveController
	{
		/**
		 * for-each hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.for-each")]
		public static void magix_execute_for_each(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>loops through all the nodes in the given 
node-list expression, setting the data-pointer to the currently processed item</p><p>your 
code will execute once for every single node you have in your return expression.&nbsp;&nbsp;
use the [.] expression to de-reference the currently iterated node</p><p>thread safe</p>";
                e.Params["_data"]["items"]["message1"].Value = "howdy world 1.0";
				e.Params["_data"]["items"]["message2"].Value = "howdy world 2.0";
				e.Params["_data"]["items"]["message3"].Value = "howdy world 3.0";
				e.Params["_data"]["items"]["message4"].Value = "howdy world 4.0";
				e.Params["_data"]["items"]["message5"].Value = "howdy world 5.0";
				e.Params["_data"]["items"]["message6"].Value = "howdy world 6.0";
				e.Params["_data"]["items"]["message7"].Value = "howdy world 7.0";
				e.Params["for-each"].Value = "[_data][items]";
				e.Params["for-each"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
				e.Params["for-each"]["set"]["value"].Value = "[.].Value";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [for-each] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you must supply an expression to [for-each]");

			Node tmp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
			if (tmp != null)
			{
                object oldDp = e.Params["_dp"].Value;
                try
				{
					for (int idxNo = 0; idxNo < tmp.Count; idxNo++)
					{
						e.Params["_dp"].Value = tmp[idxNo];
						RaiseActiveEvent(
							"magix._execute", 
							e.Params);
					}
				}
                catch (Exception err)
                {
                    while (err.InnerException != null)
                        err = err.InnerException;

                    if (err is StopCore.HyperLispStopException)
                        return; // do nothing, execution stopped

                    // re-throw all other exceptions ...
                    throw;
                }
                finally
				{
					e.Params["_dp"].Value = oldDp;
				}
			}
		}
	}
}

