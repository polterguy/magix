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
            {
                try
                {
                    Node tmp = new Node(e.Params.Name, e.Params.Value);
                    tmp["_ip"].Value = e.Params;
                    tmp["_dp"].Value = e.Params;

                    RaiseActiveEvent(
                        "magix.execute",
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
            else
            {
                if (!e.Params.Contains("_max-execution-iterations"))
                {
                    int maxNumberOfLines = int.Parse(ConfigurationManager.AppSettings["magix.execute.maximum-execution-iterations"]);
                    e.Params["_max-execution-iterations"].Value = maxNumberOfLines;
                }

                if (!e.Params.Contains("_current-executed-iterations"))
                    e.Params["_current-executed-iterations"].Value = 0;

                Node dp = Dp(e.Params);

                if (ip.Name == "execute" && ip.Value != null && ip.Get<string>().StartsWith("["))
                {
                    Node newIp = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, false); // lambda execute expression

                    if (newIp == null)
                        throw new ArgumentException("nothing to [execute]");

                    Execute(newIp, e.Params);
                }
                else
                    Execute(ip, e.Params);
            }
		}

		/*
		 * helper method for above ...
		 */
		private static void Execute(Node ip, Node pars)
		{
            if (pars.ContainsValue("_root-only-execution") && pars["_root-only-execution"].Get<bool>())
            {
                Node idxKeyword = ip;
                ExecuteSingleNode(ip, pars, idxKeyword);
            }
            else
            {
                // looping through all keywords/active-events in the child collection
                for (int idxNo = 0; idxNo < ip.Count; idxNo++)
                {
                    Node idx = ip[idxNo];
                    ExecuteSingleNode(ip, pars, idx);
                }
            }
		}

        private static void ExecuteSingleNode(Node ip, Node pars, Node executionNode)
        {
            string activeEvent = executionNode.Name;

            // checking to see if this is just a data/comment buffer ...
            if (!activeEvent.StartsWith("_") &&
                !activeEvent.StartsWith("//") &&
                activeEvent != "inspect" &&
                activeEvent != "$")
            {
                // checking to see if execution engine overflowed its number of execution lines
                int noCurrentExecutedHyperLispWords = pars["_current-executed-iterations"].Get<int>();
                int maxExecutionLines = pars["_max-execution-iterations"].Get<int>();
                if (noCurrentExecutedHyperLispWords >= maxExecutionLines)
                    throw new ApplicationException("execution engine overflowed");

                noCurrentExecutedHyperLispWords += 1;
                pars["_current-executed-iterations"].Value = noCurrentExecutedHyperLispWords;

                // verifying we're allowed to execute current active event
                CheckSandbox(activeEvent, pars);

                if (activeEvent.Contains("."))
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
                else
                {
                    // this is a keyword, and have access to the entire tree, and also needs to have the default namespace 
                    // prepended in front of it before being raised
                    // notice how [using] name of node always maps to "magix.execute.using", regardless of namespace stack
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
            }
        }

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

