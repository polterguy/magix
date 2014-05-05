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
	 * hyper lisp implementation
	 */
	public class ExecuteCore : ActiveController
	{
        /**
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
				    throw new ArgumentException("you cannot raise magix._execute directly besides for inspect purposes");

			    if (!e.Params.Contains("_dp") || !(e.Params["_dp"].Value is Node))
				    throw new ArgumentException("you cannot raise magix._execute directly besides for inspect purposes");

			    Node ip = e.Params["_ip"].Value as Node;
                Node dp = e.Params["_dp"].Value as Node;
                e.Params["_namespaces"].Add(new Node("item", ip.Get<string>()));

                Execute(ip, dp, e.Params);
            }
            finally
            {
                e.Params["_namespaces"][e.Params["_namespaces"].Count - 1].UnTie();
                if (e.Params["_namespaces"].Count == 0)
                    e.Params["_namespaces"].UnTie();
            }
        }

		/**
		 * hyper lisp implementation
		 */
		[ActiveEvent(Name = "magix.execute")]
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

			try
			{
                if (e.Params.Value != null)
                    throw new ArgumentException("you cannot pass in a path to [magix.execute] when using the fully qualified namespace");
				Execute(e.Params, e.Params, new Node(e.Params.Name, e.Params.Value));
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

		/**
		 * internally used event
		 */
		[ActiveEvent(Name = "magix._execute")]
		[ActiveEvent(Name = "magix.execute.execute")]
		public static void magix_execute_internal(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>executes the incoming parameters 
as hyper lisp, meaning it will raise everything containing a '.' as an active event, 
while everything not starting with a '_', or containing a period, will be assumed to 
be a hyper lisp keyword, and appended behind 'magix.execute.', or the currently used 
namespace, before that string is raised as an active event</p><p>a hyper lisp keyword 
will have access to the entire data tree, while a normal active event will get a deep 
copy of the tree underneath its own node.&nbsp;&nbsp;if the [execute] has a value, it 
will be expected to be an expression referencing another node, which will be executed 
instead of the incoming parameter nodes</p><p>thread safe</p>";
				e.Params["_data"]["value"].Value = "thomas";
				e.Params["if"].Value = "equals";
                e.Params["if"]["lhs"].Value = "[_data][value].Value";
                e.Params["if"]["rhs"].Value = "thomas";
                e.Params["if"]["code"]["magix.viewport.show-message"]["message"].Value = "hi thomas";
				e.Params["else"]["magix.viewport.show-message"].Value = null;
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "hi stranger";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise magix._execute directly besides for inspect purposes");

			if (!e.Params.Contains("_dp") || !(e.Params["_dp"].Value is Node))
				throw new ArgumentException("you cannot raise magix._execute directly besides for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;
			Node dp = e.Params["_dp"].Value as Node;

			if (ip.Name == "execute" && ip.Value != null && ip.Get<string>().StartsWith("["))
			{
				Node newIp = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node; // lambda execute expression

				if (newIp == null)
					throw new ArgumentException("nothing to [execute]");

				Execute(newIp, dp, e.Params);
			}
			else
				Execute(ip, dp, e.Params);
		}

		/*
		 * helper method for above ...
		 */
		private static void Execute(Node ip, Node dp, Node state)
		{
			// looping through all keywords/active-events in the child collection
			for (int idxNo = 0; idxNo < ip.Count; idxNo++)
			{
				Node idx = ip[idxNo];
				string activeEvent = idx.Name;

				// checking to see if this is just a data buffer ...
                if (activeEvent.StartsWith("_") || activeEvent == "inspect" || activeEvent == "$")
					continue;

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
					object oldIp = state.Contains("_ip") ? state["_ip"].Value : null;

					// this is a keyword, and have access to the entire tree, and also needs to have magix.execute. 
                    // prepended in front of it before being raised
                    if (state.Contains("_namespaces") && state["_namespaces"].Count > 0)
                        activeEvent = state["_namespaces"][state["_namespaces"].Count - 1].Get<string>() + "." + activeEvent;
                    else
    					activeEvent = "magix.execute." + activeEvent;

					state["_ip"].Value = idx;

					if (!state.Contains("_dp"))
						state["_dp"].Value = ip;

					try
					{
						RaiseActiveEvent(
							activeEvent,
							state);
					}
					finally
					{
						if (oldIp != null)
							state["_ip"].Value = oldIp;
					}
				}
			}
		}
	}
}

