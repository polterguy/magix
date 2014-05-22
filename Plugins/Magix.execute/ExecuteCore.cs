/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
         * using keyword implementation
         */
        [ActiveEvent(Name = "magix.execute.using")]
        public void magix_execute_using(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.using-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.using-sample]");
                return;
            }

            try
            {
			    if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				    throw new ArgumentException("you cannot raise [magix.execute.using] directly besides for inspect purposes");

			    if (!e.Params.Contains("_dp") || !(e.Params["_dp"].Value is Node))
				    throw new ArgumentException("you cannot raise [magix.execute.using] directly besides for inspect purposes");

                Node dp = Dp(e.Params);
                e.Params["_namespaces"].Add(new Node("item", ip.Get<string>()));

                Execute(ip, e.Params);
            }
            finally
            {
                e.Params["_namespaces"][e.Params["_namespaces"].Count - 1].UnTie();
                if (e.Params["_namespaces"].Count == 0)
                    e.Params["_namespaces"].UnTie();
            }
        }

        /*
          * sanbox keyword implementation
          */
        [ActiveEvent(Name = "magix.execute.sandbox")]
        public void magix_execute_sandbox(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sandbox-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sandbox-sample]");
                return;
            }

            if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
                throw new ArgumentException("you cannot raise [sandbox] directly, except for inspect purposes");

            if (!ip.Contains("code"))
                throw new ArgumentException("you need to supply a [code] block to [sandbox] active event");
            if (!ip.Contains("whitelist"))
                throw new ArgumentException("you need to supply a [whitelist] block to [sandbox] active event");

            e.Params["_whitelist"].AddRange(ip["whitelist"].Clone());
            e.Params["_ip"].Value = ip["code"];
            RaiseActiveEvent(
                "magix.execute",
                e.Params);
        }

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
                    "[magix.execute.execute-dox].Value");
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
                if (!e.Params.Contains("_max-execution-lines"))
                {
                    int maxNumberOfLines = int.Parse(ConfigurationManager.AppSettings["magix.execute.maximum-execution-iterations"]);
                    e.Params["_max-execution-lines"].Value = maxNumberOfLines;
                }

                if (!e.Params.Contains("_current-executed-lines"))
                    e.Params["_current-executed-lines"].Value = 0;

                Node dp = Dp(e.Params);

                if (ip.Name == "execute" && ip.Value != null && ip.Get<string>().StartsWith("["))
                {
                    Node newIp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node; // lambda execute expression

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
            int maxExecutionLines = pars["_max-execution-lines"].Get<int>();

			// looping through all keywords/active-events in the child collection
			for (int idxNo = 0; idxNo < ip.Count; idxNo++)
			{
				Node idx = ip[idxNo];
				string activeEvent = idx.Name;
                
				// checking to see if this is just a data/comment buffer ...
                if (activeEvent.StartsWith("_") || activeEvent.StartsWith("//") || activeEvent == "inspect" || activeEvent == "$")
					continue;

                // checking to see if execution engine overflowed its number of execution lines
                int noCurrentExecutedHyperLispWords = pars["_current-executed-lines"].Get<int>();
                if (noCurrentExecutedHyperLispWords >= maxExecutionLines)
                    throw new ApplicationException("execution engine overflowed");

                noCurrentExecutedHyperLispWords += 1;
                pars["_current-executed-lines"].Value = noCurrentExecutedHyperLispWords;

                if (activeEvent.Contains("."))
				{
                    // verifying we're allowed to execute current active event
                    CheckSandbox(activeEvent, pars);

					// this is an active event reference, and does not have access to entire tree
					Node parent = idx.Parent;
					idx.SetParent(null);
					try
					{
                        Node executionNode = idx;
                        if (activeEvent.StartsWith("magix.execute"))
                        {
                            Node tmpExe = new Node();
                            tmpExe.AddRange(pars.Clone());
                            tmpExe["_ip"].Value = executionNode;
                            tmpExe["_dp"].Value = executionNode;
                            executionNode = tmpExe;
                        }
						RaiseActiveEvent(
							activeEvent,
                            executionNode);
					}
					finally
					{
						idx.SetParent(parent);
					}
				}
				else
				{
                    // verifying we're allowed to execute current active event
                    CheckSandbox(activeEvent, pars);

                    object oldIp = pars.Contains("_ip") ? pars["_ip"].Value : null;

					// this is a keyword, and have access to the entire tree, and also needs to have the default namespace 
                    // prepended in front of it before being raised
                    if (pars.Contains("_namespaces") && pars["_namespaces"].Count > 0)
                        activeEvent = pars["_namespaces"][pars["_namespaces"].Count - 1].Get<string>() + "." + activeEvent;
                    else
    					activeEvent = "magix.execute." + activeEvent;

                    pars["_ip"].Value = idx;

					if (!pars.Contains("_dp"))
						pars["_dp"].Value = ip;

					try
					{
						RaiseActiveEvent(
							activeEvent,
							pars);
					}
					finally
					{
						if (oldIp != null)
							pars["_ip"].Value = oldIp;
					}
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

