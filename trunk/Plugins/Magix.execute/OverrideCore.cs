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
	 * override hyper lisp keyword
	 */
	public class OverrideCore : ActiveController
	{
		/**
		 * override hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.override")]
		public static void magix_execute_set(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>overrides the given active event from the 
[override] node's value with the active event from the [with] node's value.&nbsp;&nbsp;if 
no [with] is given, existing override is removed, if any exists</p><p>if you supply a 
[persist] parameter, and sets its value to false, then the override will not be persisted, 
and will only last until the application pool is restarted.&nbsp;&nbsp;this is useful 
sometimes, since it will execute much faster, especially for overides you will anyway re-run 
when application pool is restarted</p><p>thread safe</p>";
                e.Params["override"].Value = "namespace.foo";
				e.Params["override"]["with"].Value = "namespace.bar";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [override] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

            string activeEvent = ip.Get<string>();
            if (string.IsNullOrEmpty(activeEvent))
                throw new ArgumentException("you need to specify which active event you wish to override in [override]");

            if (ip.Contains("with"))
            {
                string newActiveEvent = ip["with"].Get<string>();
                if (newActiveEvent == null)
                    throw new ArgumentException("you cannot [override] an active event with a null string");

                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    // removing any previous similar events
                    Node removeNode = new Node();

                    removeNode["prototype"]["event"].Value = activeEvent;
                    removeNode["prototype"]["type"].Value = "magix.execute.override";

                    RaiseActiveEvent(
                        "magix.data.remove",
                        removeNode);

                    Node saveNode = new Node();

                    saveNode["id"].Value = Guid.NewGuid().ToString();
                    saveNode["value"]["event"].Value = activeEvent;
                    saveNode["value"]["type"].Value = "magix.execute.override";
                    saveNode["value"]["with"].Value = newActiveEvent;

                    RaiseActiveEvent(
                        "magix.data.save",
                        saveNode);
                }
                ActiveEvents.Instance.CreateEventMapping(activeEvent, newActiveEvent);
            }
            else
            {
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    Node removeNode = new Node();

                    removeNode["prototype"]["event"].Value = activeEvent;
                    removeNode["prototype"]["type"].Value = "magix.execute.override";

                    RaiseActiveEvent(
                        "magix.data.remove",
                        removeNode);
                }

                ActiveEvents.Instance.RemoveMapping(activeEvent);
            }
		}
	}
}

