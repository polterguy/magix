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

            // checking syntax
            VerifySyntaxElseIf(ip);

            // making sure previous [if] or [else-if] didn't execute before we run comparison to see if we should execute body of [else-if]
            if (!CheckState(e.Params))
                IfElseIfImplementation(
                    e.Params,
                    "magix.execute.else-if");
            else
            {
                // checking to see if next keyword is [else] or [else-if], and if not, we remove signaling state ("_state_if") from state
                Node next = ip.Next();
                if (next == null || (next.Name != "else-if" && next.Name != "magix.execute.else-if"
                    && next.Name != "else" && next.Name != "magix.execute.else"))
                    PopState(e.Params, ip);
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

            // verifying an [else] is only followed by an [if] or an [else-if]
            VerifySyntaxElse(ip);

            // saving state before we pop it to see if we should execute [else] body
            bool state = CheckState(e.Params);

            // removing signaling state ("_state_if") from state
            PopState(e.Params, ip);

            // checking to see if previous [if] or [else-if] executed, before we execute [else]
            if (!state)
                RaiseActiveEvent(
                    "magix.execute",
                    e.Params);
        }

        /*
         * verifies that [else-if] only comes after [if] or another [else-if]
         */
        private static void VerifySyntaxElseIf(Node ip)
        {
            Node previous = ip.Previous();
            if (previous == null || (previous.Name != "if" && previous.Name != "magix.execute.if" &&
                previous.Name != "else-if" && previous.Name != "magix.execute.else-if"))
                throw new HyperlispSyntaxErrorException("you cannot have an [else-if] statement without a matching [if]");
        }

        /*
         * verifies syntax of [else] keyword
         */
        private static void VerifySyntaxElse(Node ip)
        {
            Node previous = ip.Previous();
            if (previous == null || (previous.Name != "if" && previous.Name != "magix.execute.if" &&
                previous.Name != "else-if" && previous.Name != "magix.execute.else-if"))
                throw new HyperlispSyntaxErrorException("you cannot have an [else] statement without a matching if");
        }

        /*
         * helper for executing [if]/[else-if]
         */
        private static void IfElseIfImplementation(Node pars, string evt)
        {
            Node ip = Ip(pars);
            Node dp = Dp(pars);

            // verifying [if] or [else-if] has a [code] block beneath itself
            if (!ip.Contains("code"))
                throw new HyperlispSyntaxErrorException("you must supply a [code] node for your [" + evt + "] expressions");

            // verifying there's at least an [lhs] node beneath [if] or [else-if]
            if (!ip.Contains("lhs"))
                throw new HyperlispSyntaxErrorException("you must supply an [lhs] node for your [" + evt + "] expressions");

            // verifying there's an operator on [if] or [else-if]
            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new HyperlispSyntaxErrorException("you must supply an operator for your [" + evt + "] expressions as Value of [" + evt + "]");

            // checking statement to see if it's true, before we execute [code] block
            if (StatementHelper.CheckExpressions(ip, dp))
            {
                // yup, we've got a match, executing [code] block
                pars["_ip"].Value = ip["code"];
                RaiseActiveEvent(
                    "magix.execute",
                    pars);

                // checking to see if we should add state ("_state_if") to state such that no followup [else] or [else-if] gets executed
                Node next = ip.Next();
                if (next != null && (next.Name == "else-if" || next.Name == "magix.execute.else-if"
                    || next.Name == "else" || next.Name == "magix.execute.else"))
                    PushState(pars, ip);
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

        /*
         * sets the state of the current scope of hyperlisp code to "executed", such that no following [else] or [else-if] executes
         */
        private static void PushState(Node pars, Node ip)
        {
            string currentScopeDna = ip.Parent.Dna;
            pars["_state_if"][currentScopeDna].Value = null;
        }

        /*
         * removes the state from the current scope of hyperlisp code
         */
        private static void PopState(Node pars, Node ip)
        {
            string currentScopeDna = ip.Parent.Dna;
            pars["_state_if"][currentScopeDna].UnTie();
            if (pars["_state_if"].Count == 0)
                pars["_state_if"].UnTie();
        }
    }
}
