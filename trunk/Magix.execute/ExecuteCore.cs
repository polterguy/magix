/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
		 * hyper lisp implementation
		 */
		[ActiveEvent(Name = "magix.execute")]
		public static void magix_execute(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"executes the incoming parameters 
as hyper lisp, meaning it will raise everything containing a '.', 
while everything not starting with a '_' will be assumed to be a hyper lisp 
keyword, and appended behind 'magix.execute.', before that string is raised as an active event.&nbsp;&nbsp;
a hyper lisp keyword will have access to the entire data tree, while a normal active 
event will get a deep copy of the tree underneath its own node.&nbsp;&nbsp;
if the [magix.execute]/[execute]/root-node has a value, it will be expected to 
be an expression referencing another node, which will be executed instead of the 
incoming parameter nodes.&nbsp;&nbsp;
thread safe";
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
				Execute(e.Params, e.Params, new Node(e.Params.Name, e.Params.Value));
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				if (err is StopCore.HyperLispStopException)
					return; // do nothing, execution stopped

				// re-throw all other exceptions ...
				throw err;
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
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"executes the incoming parameters 
as hyper lisp, meaning it will raise everything containing a '.', 
while everything not starting with a '_' will be assumed to be a hyper lisp 
keyword, and appended behind 'magix.execute.', before that string is raised as an active event.&nbsp;&nbsp;
a hyper lisp keyword will have access to the entire data tree, while a normal active 
event will get a deep copy of the tree underneath its own node.&nbsp;&nbsp;
if the [magix.execute]/[execute]/root-node has a value, it will be expected to 
be an expression referencing another node, which will be executed instead of the 
incoming parameter nodes.&nbsp;&nbsp;
thread safe";
				e.Params["_data"]["value"].Value = "thomas";
				e.Params["if"].Value = "[_data][value].Value==thomas";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "hi thomas";
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

			if (ip.Name == "execute" && ip.Value != null)
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

					// this is a keyword, and have access to the entire tree, and also needs to have magix.execute. prepended in front of it before being raised
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

