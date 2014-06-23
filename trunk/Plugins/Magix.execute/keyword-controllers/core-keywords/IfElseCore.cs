/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using System.Globalization;

namespace Magix.execute
{
	/*
	 * if/else-if/else hyperlisp active events
	 */
	public class IfElseCore : ActiveController
	{
		/*
		 * if implementation
		 */
		[ActiveEvent(Name = "magix.execute.if")]
		public static void magix_execute_if(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.if-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.if-sample]");
                return;
			}

            IfElseIfImplementation(
                e.Params,
                "magix.execute.if");
		}

		/*
		 * else-if implementation
		 */
		[ActiveEvent(Name = "magix.execute.else-if")]
		public static void magix_execute_else_if(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.else-if-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.else-if-sample]");
                return;
			}
			
            Node previous = ip.Previous();
            if (previous == null || (previous.Name != "if" && previous.Name != "magix.execute.if" &&
                previous.Name != "else-if" && previous.Name != "magix.execute.else-if"))
                throw new ArgumentException("you cannot have an [else-if] statement without a matching if");

            if (!CheckState(e.Params))
            {
                IfElseIfImplementation(
                    e.Params,
                    "magix.execute.else-if");
            }
            else
            {
                Node next = ip.Next();
                if (next == null || (next.Name != "else-if" && next.Name != "magix.execute.else-if"
                    && next.Name != "else" && next.Name != "magix.execute.else"))
                {
                    PopState(e.Params, ip);
                }
            }
		}
		
		/*
		 * else implementation
		 */
		[ActiveEvent(Name = "magix.execute.else")]
		public static void magix_execute_else(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.else-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.else-sample]");
                return;
			}

            Node previous = ip.Previous();
            if (previous == null || (previous.Name != "if" && previous.Name != "magix.execute.if" &&
                previous.Name != "else-if" && previous.Name != "magix.execute.else-if"))
                throw new ArgumentException("you cannot have an [else] statement without a matching if");

			// Checking to see if a previous "if" or "else-if" statement has returned true
            bool state = CheckState(e.Params);
            PopState(e.Params, ip);

            if (!state)
                RaiseActiveEvent(
                    "magix.execute",
                    e.Params);
        }

        /*
         * helper for executing [if]/[else-if]
         */
		private static void IfElseIfImplementation(Node pars, string evt)
		{
            Node ip = Ip(pars);
            Node dp = Dp(pars);

            if (!ip.Contains("code"))
                throw new ArgumentException("you must supply a [code] node for your [" + evt + "] expressions");

            if (!ip.Contains("lhs"))
                throw new ArgumentException("you must supply an [lhs] node for your [" + evt + "] expressions");

			string oper = ip.Get<string>();

			if (string.IsNullOrEmpty(oper))
                throw new ArgumentException("you must supply an operator for your [" + evt + "] expressions as Value of [" + evt + "]");

            bool shouldExecuteCode = StatementHelper.CheckExpressions(ip, dp);
            if (shouldExecuteCode)
            {
                pars["_ip"].Value = ip["code"];
                RaiseActiveEvent(
                    "magix.execute",
                    pars);

                Node next = ip.Next();
                if (next != null && (next.Name == "else-if" || next.Name == "magix.execute.else-if"
                    || next.Name == "else" || next.Name == "magix.execute.else"))
                {
                    PushState(pars, ip);
                }
            }
        }

        /*
         * checks to see if a previous [if]/[else-if] has evaluated to true
         */
        private static bool CheckState(Node pars)
        {
            Node ip = Ip(pars);
            string currentScopeDna = ip.Parent.Dna;
            if (pars.Contains("_state_if") && pars["_state_if"].Contains(currentScopeDna))
                return true;
            return false;
        }

        private static void PushState(Node pars, Node ip)
        {
            string currentScopeDna = ip.Parent.Dna;
            pars["_state_if"][currentScopeDna].Value = null;
        }

        private static void PopState(Node pars, Node ip)
        {
            string currentScopeDna = ip.Parent.Dna;
            pars["_state_if"][currentScopeDna].UnTie();
            if (pars["_state_if"].Count == 0)
                pars["_state_if"].UnTie();
        }
    }
}
