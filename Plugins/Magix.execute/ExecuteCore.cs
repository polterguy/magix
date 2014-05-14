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
	 * hyper lisp implementation
	 */
	public class ExecuteCore : ActiveController
	{
        /*
         * using keyword implementation
         */
        [ActiveEvent(Name = "magix.execute.using")]
        public void magix_execute_using(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>changes the default namespace 
for the current scope.&nbsp;&nbsp;this allows you to raise active events which 
you normally would have to raise with a period in their name, without the period, 
which allows active events which normally cannot access the entire execution tree, 
to do just that</p><p>thread safe</p>";
                e.Params["using"].Value = "magix.math";
                e.Params["using"]["add"][""].Value = 4;
                e.Params["using"]["add"]["", 1].Value = 1;
                return;
            }

            try
            {
			    if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				    throw new ArgumentException("you cannot raise [magix.execute.using] directly besides for inspect purposes");

			    if (!e.Params.Contains("_dp") || !(e.Params["_dp"].Value is Node))
				    throw new ArgumentException("you cannot raise [magix.execute.using] directly besides for inspect purposes");

			    Node ip = e.Params["_ip"].Value as Node;
                Node dp = e.Params["_dp"].Value as Node;
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
		 * hyper lisp implementation
		 */
        [ActiveEvent(Name = "magix.execute")]
        [ActiveEvent(Name = "magix.execute.execute")]
        public static void magix_execute(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>executes the incoming parameters 
as hyper lisp, meaning it will raise everything containing a period, while everything 
not starting with a '_' will be assumed to be a hyper lisp keyword, and appended 
behind 'magix.execute.', before that string is raised as an active event</p><p>a 
hyper lisp keyword will have access to the entire data tree, while a normal active 
event will only be able to modify the parts of the tree from underneath its own node
</p><p>thread safe</p>";
				e.Params["_data"]["value"].Value = "thomas";
				e.Params["if"].Value = "[_data][value].Value==thomas";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "hi thomas";
				e.Params["else"]["magix.viewport.show-message"].Value = null;
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "hi stranger";
				return;
			}

            if (!e.Params.Contains("_ip"))
            {
                try
                {
                    Node tmp = new Node("magix.execute", e.Params.Value);
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
                    int maxNumberOfLines = int.Parse(ConfigurationManager.AppSettings["magix.execute.maximum-execution-lines"]);
                    e.Params["_max-execution-lines"].Value = maxNumberOfLines;
                }

                if (!e.Params.Contains("_current-executed-lines"))
                {
                    e.Params["_current-executed-lines"].Value = 0;
                }

                Node ip = Ip(e.Params);
                Node dp = e.Params;
                if (e.Params.Contains("_dp"))
                    dp = e.Params["_dp"].Get<Node>();

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
					// this is an active event reference, and does not have access to entire tree
					Node parent = idx.Parent == null ? null : idx.Parent;
					idx.SetParent(null);
					try
					{
						RaiseActiveEvent(
							activeEvent,
							idx);
					}
					finally
					{
						idx.SetParent(parent);
					}
				}
				else
				{
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
	}
}

