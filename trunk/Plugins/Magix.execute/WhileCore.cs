/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * while keyword
	 */
	public class WhileCore : ActiveController
	{
		/**
		 * while keyword
		 */
		[ActiveEvent(Name = "magix.execute.while")]
		public static void magix_execute_while(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>creates a loop that executes the
underlaying code block repeatedly, as long as the statement in the value of [while] 
is true</p><p>the operator used to compare the [lhs] and the [rhs] nodes must be 
defined using the value of the [while] node.&nbsp;&nbsp;legal values for the operator 
type is 'exist', 'not-exist', 'equals', 'not-equals', 'less-than', 'more-than', 
'less-than-equals' and 'more-than-equals'</p><p>the engine will convert automatically 
between int, decimal, date and bool, or resort to string if no conversion is possible, 
to compare values of [lhs] and [rhs].&nbsp;&nbsp;the [lhs] and [rhs] nodes can be either 
an expression, or a hardcoded value.&nbsp;&nbsp;you can compare two node trees in [lhs] 
and [rhs], which means that the node trees will be compared deeply, comparing their name, 
value and children for equality</p><p>thread safe</p>";
				e.Params["_data"]["txt1"].Value = "hello world 1.0";
				e.Params["_data"]["txt2"].Value = "hello world 2.0";
				e.Params["_data"]["txt3"].Value = "hello world 3.0";
				e.Params["while"].Value = "not-equals";
                e.Params["while"]["lhs"].Value = "[_data].Count";
                e.Params["while"]["rhs"].Value = "0";
				e.Params["while"]["code"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
                e.Params["while"]["code"]["set"]["value"].Value = "[_data][0].Value";
                e.Params["while"]["code"].Add(new Node("set", "[_data][0]"));
                e.Params["while"]["code"]["magix.viewport.show-message"].Value = null;
                e.Params["while"]["code"]["magix.viewport.show-message"]["message"].Value = "to be changed";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [while] directly, except for inspect purposes");

            Node ip = Ip(e.Params);
			Node dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("code"))
                throw new ArgumentException("you need to supply a [code] block to the [while] active event");

			while (StatementHelper.CheckExpressions(ip, dp))
			{
                Node tmp = new Node();

                tmp["_ip"].Value = ip["code"];
                tmp["_dp"].Value = dp;

                try
                {
                    RaiseActiveEvent(
                        "magix._execute",
                        tmp);
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
            }
		}
	}
}

