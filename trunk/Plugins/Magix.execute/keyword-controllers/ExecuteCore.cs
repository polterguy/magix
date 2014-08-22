/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
    /*
     * hyperlisp implementation
     */
    public class ExecuteCore : ActiveController
    {
        /*
         * hyperlisp implementation
         */
        [ActiveEvent(Name = "magix.execute")]
        [ActiveEvent(Name = "magix.execute.execute")]
        public static void magix_execute(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.execute-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.execute-sample]");
                return;
            }

            if (!e.Params.Contains("_ip"))
                ExecuteOutermostBlock(e.Params);
            else
            {
                if (ip.Name == "execute" && ip.Value != null && ip.Get<string>().StartsWith("["))
                {
                    ip = Expressions.GetExpressionValue<Node>(ip.Get<string>(), Dp(e.Params), ip, false); // lambda execute expression
                    if (ip == null)
                        throw new ArgumentException("nothing to [execute]");
                }
                Execute(ip, e.Params);
            }
        }

        /*
         * executes outermost block of magix.execute, wrapping original node inside a [_ip], [_dp] pair, before calling self
         */
        private static void ExecuteOutermostBlock(Node pars)
        {
            try
            {
                Node exeNode = new Node(pars.Name, pars.Value);
                exeNode["_ip"].Value = pars;
                exeNode["_dp"].Value = pars;
                RaiseActiveEvent(
                    "magix.execute",
                    exeNode);
            }
            catch (Exception err)
            {
                while (err.InnerException != null)
                    err = err.InnerException;

                if (!(err is StopCore.HyperLispStopException)) // don't throw stop keywords
                    throw;
            }
        }

        /*
         * executes the given ip node
         */
        private static void Execute(Node ip, Node pars)
        {
            if (!pars.Contains("_max-execution-iterations"))
                pars["_max-execution-iterations"].Value = int.Parse(ConfigurationManager.AppSettings["magix.execute.maximum-execution-iterations"]);

            if (!pars.Contains("_current-executed-iterations"))
                pars["_current-executed-iterations"].Value = 0;

            if (pars.GetValue("_root-only-execution", false))
                ExecuteSingleNode(ip, pars, ip); // only executing actual ip node, not children as is default
            else
            {
                // looping through all keywords/active-events in the child collection
                for (int idxNo = 0; idxNo < ip.Count; idxNo++)
                {
                    ExecuteSingleNode(ip, pars, ip[idxNo]);
                }
            }
        }

        /*
         * executes one single node
         */
        private static void ExecuteSingleNode(Node ip, Node pars, Node executionNode)
        {
            string activeEvent = executionNode.Name;

            // checking to see if this is just a data/comment buffer ...
            if (!activeEvent.StartsWith("_") &&
                !activeEvent.StartsWith("//") &&
                activeEvent != "inspect" &&
                activeEvent != "$")
            {
                // checking to see if execution engine overflowed its number of execution lines, and incrementing number of executed lines
                HandleExecutionIterations(pars);

                // verifying we're allowed to execute current active event
                CheckSandbox(activeEvent, pars);

                if (activeEvent.Contains("."))
                    ExecuteGlobalActiveEvent(ip, pars, executionNode, activeEvent); // global active event
                else
                    ExecuteLocalActiveEvent(ip, pars, executionNode, activeEvent); // local active event
            }
        }

        /*
         * executes a single node, prepending the current namespace in front of it before raising the active event
         */
        private static void ExecuteLocalActiveEvent(Node ip, Node pars, Node executionNode, string activeEvent)
        {
            if (activeEvent == "using")
                activeEvent = "magix.execute." + activeEvent;
            else if (pars.Contains("_namespaces") && pars["_namespaces"].Count > 0)
                activeEvent = pars["_namespaces"][pars["_namespaces"].Count - 1].Get<string>() + "." + activeEvent;
            else
                activeEvent = "magix.execute." + activeEvent;

            pars["_ip"].Value = executionNode;
            RaiseActiveEvent(
                activeEvent,
                pars);
            pars["_ip"].Value = ip;
        }

        /*
         * executes a fully qualified active event
         */
        private static void ExecuteGlobalActiveEvent(Node ip, Node pars, Node executionNode, string activeEvent)
        {
            Node oldParent = executionNode.Parent;
            Node oldDp = pars["_dp"].Get<Node>();
            try
            {
                // this is an active event reference, and does not have access to entire tree
                executionNode.SetParent(null);
                pars["_ip"].Value = executionNode;
                pars["_dp"].Value = executionNode;
                RaiseActiveEvent(
                    activeEvent,
                    pars);
            }
            finally
            {
                executionNode.SetParent(oldParent);
                pars["_dp"].Value = oldDp;
                pars["_ip"].Value = ip;
            }
        }

        /*
         * increments and checks execution iterations
         */
        private static void HandleExecutionIterations(Node pars)
        {
            int noCurrentExecutedHyperLispWords = pars["_current-executed-iterations"].Get<int>();
            int maxExecutionLines = pars["_max-execution-iterations"].Get<int>();
            if (noCurrentExecutedHyperLispWords >= maxExecutionLines)
                throw new ApplicationException("execution engine overflowed");
            noCurrentExecutedHyperLispWords += 1;
            pars["_current-executed-iterations"].Value = noCurrentExecutedHyperLispWords;
        }

        /*
         * checks to see if active event is allowed according to sandbox definition, if any
         */
        private static void CheckSandbox(string activeEvent, Node pars)
        {
            if (pars.Contains("_whitelist"))
            {
                if (!pars["_whitelist"].Contains(activeEvent))
                    throw new ApplicationException("tried to execute an active event that was not in the [whitelist]");
            }
        }
    }
}

