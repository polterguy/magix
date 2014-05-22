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
	 * override hyperlisp keyword
	 */
	public class OverrideCore : ActiveController
	{
		/**
		 * override hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.override")]
		public static void magix_execute_set(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.override-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.override-sample]");
                return;
			}

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

