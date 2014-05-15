/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using System.Globalization;

namespace Magix.execute
{
	/*
	 * if/else-if/else hyper lisp active events
	 */
	public class IfElseCore : ActiveController
	{
		/*
		 * if implementation
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public static void magix_execute_if(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>executes the [code] block of nodes as an execution
block, but only if the [if] statement returns true</p><p>pair your [if] statements together with 
[else-if] and [else] to create branching and control of flow of your program.&nbsp;&nbsp;if an [if]
statement returns true, then no paired [else-if] or [else] statements will be executed</p><p>the 
operator used to compare the [lhs] and the [rhs] nodes must be defined using the value of the [if] 
node.&nbsp;&nbsp;legal values for the operator type is 'exist', 'not-exist', 'equals', 'not-equals', 
'less-than', 'more-than', 'less-than-equals' and 'more-than-equals'</p><p>the engine will convert 
automatically between int, decimal, date and bool, or resort to string if no conversion is possible.
&nbsp;&nbsp;the [lhs] and [rhs] nodes can be either an expression, or a hardcoded value.&nbsp;&nbsp;
you can compare two node trees in [lhs] and [rhs], which means that the node trees will be compared 
deeply, comparing their name, value and children for equality</p><p>thread safe</p>";
				ip["_data"]["item"].Value = "cache-object";
				ip["_data"]["cache"].Value = null;
                ip["if"].Value = "not-equals";
                ip["if"]["lhs"].Value = "[_data][item].Value";
                ip["if"]["rhs"].Value = "[_data][1].Name";
                ip["if"]["code"]["magix.viewport.show-message"].Value = null;
				ip["if"]["code"]["magix.viewport.show-message"]["message"].Value = "they are not the same";
				return;
			}

            if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
                throw new ArgumentException("you cannot raise [if] directly, except for inspect purposes");

            IfImplementation(
				e.Params, 
				"magix.execute.if");
		}

		/*
		 * else-if implementation
		 */
		[ActiveEvent(Name = "magix.execute.else-if")]
		public static void magix_execute_else_if(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>executes the underlaying [code] node,
but only if no previous [if] or [else-if] statement has returned true, and the statement 
inside the value of the [else-if] returns true</p><p>the operator used to compare the 
[lhs] and the [rhs] nodes must be defined using the value of the [else-if] node.&nbsp;
&nbsp;legal values for the operator type is 'exist', 'not-exist', 'equals', 'not-equals', 
'less-than', 'more-than', 'less-than-equals' and 'more-than-equals'</p><p>the engine will 
convert automatically between int, decimal, date and bool, or resort to string if no 
conversion is possible.&nbsp;&nbsp;the [lhs] and [rhs] nodes can be either an expression, 
or a hardcoded value.&nbsp;&nbsp;you can compare two node trees in [lhs] and [rhs], which 
means that the node trees will be compared deeply, comparing their name, value and children 
for equality</p><p>thread safe</p>";
				ip["_data"]["node"].Value = null;
                ip["if"].Value = "exist";
                ip["if"]["lhs"].Value = "[_data][node].Value";
                ip["if"]["code"]["magix.viewport.show-message"]["message"].Value = "darn it";
                ip["else-if"].Value = "exist";
                ip["else-if"]["lhs"].Value = "[_data][node]";
                ip["else-if"]["code"]["magix.viewport.show-message"]["message"].Value = "puuh";
				return;
			}
			
			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [else-if] directly, except for inspect purposes");

			// Checking to see if a previous "if" or "else-if" statement has returned true
            if (ip.Parent.Contains("_state_if") &&
                ip.Parent["_state_if"].Get<bool>())
				return;

			IfImplementation(
				e.Params, 
				"magix.execute.else-if");
		}
		
		/**
		 * else implementation
		 */
		[ActiveEvent(Name = "magix.execute.else")]
		public static void magix_execute_else(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>executes the underlaying code block,
but only if no paired [if] or [else-if] statement has returned true</p><p>thread safe
</p>";
				ip["if"].Value = "exist";
                ip["if"]["lhs"].Value = "[_not-existing-node]";
				ip["if"]["code"]["magix.viewport.show-message"]["message"].Value = "ohh crap";
				ip["else"]["magix.viewport.show-message"]["message"].Value = "yup, still sane";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [else] directly, except for inspect purposes");

			// Checking to see if a previous "if" or "else-if" statement has returned true
            if (ip.Parent.Contains("_state_if") &&
                ip.Parent["_state_if"].Get<bool>())
            {
                return;
            }

			RaiseActiveEvent(
				"magix.execute", 
				e.Params);
		}

		// Private helper, implementation for both "if" and "else-if" ...
		private static void IfImplementation(Node pars, string evt)
		{
			Node ip = pars["_ip"].Value as Node;

            Node dp = ip;
            if (pars.Contains("_dp"))
                dp = pars["_dp"].Value as Node;

            if (!ip.Contains("code"))
                throw new ArgumentException("you must supply a [code] node for your [" + evt + "] expressions");

            if (!ip.Contains("lhs"))
                throw new ArgumentException("you must supply an [lhs] node for your [" + evt + "] expressions");

			string oper = ip.Value as string;

			if (string.IsNullOrEmpty(oper))
                throw new ArgumentException("you must supply an operator for your [" + evt + "] expressions as Value of [" + evt + "]");

            bool shouldExecuteCode = StatementHelper.CheckExpressions(ip, dp);
            ExecuteCode(pars, ip, dp, shouldExecuteCode);
		}

        private static void ExecuteCode(Node pars, Node ip, Node dp, bool expressionIsTrue)
        {
            if (expressionIsTrue)
            {
                // Changing state, to signal any later [else-if] and [else] expressions that expression has executed
                ip.Parent["_state_if"].Value = true;
                pars["_ip"].Value = ip["code"];

                RaiseActiveEvent(
                    "magix.execute",
                    pars);
            }
            else
                ip.Parent["_state_if"].UnTie();
        }
	}
}
