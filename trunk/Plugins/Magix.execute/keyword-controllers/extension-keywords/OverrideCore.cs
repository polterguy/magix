/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * override hyperlisp keyword
	 */
	public class OverrideCore : ActiveController
	{
		/*
		 * override hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.override")]
		public static void magix_execute_override(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.override-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.override-sample]");
                return;
			}

            if (!ip.ContainsValue("name"))
                throw new ArgumentException("missing [name] parameter in [override]");
            string activeEvent = ip["name"].Get<string>();

            if (ip.Contains("with"))
            {
                string newActiveEvent = ip["with"].Get<string>();
                if (newActiveEvent == null)
                    throw new ArgumentException("you cannot [override] an active event with a null string");

                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    DataBaseRemoval.Remove(activeEvent, "magix.execute.override", e.Params);

                    Node saveNode = new Node("magix.data.save");
                    saveNode["id"].Value = Guid.NewGuid().ToString();
                    saveNode["value"]["event"].Value = activeEvent;
                    saveNode["value"]["type"].Value = "magix.execute.override";
                    saveNode["value"]["with"].Value = newActiveEvent;

                    Node oldIp = e.Params["_ip"].Get<Node>();
                    Node oldDp = e.Params["_dp"].Get<Node>();
                    e.Params["_root-only-execution"].Value = true;
                    try
                    {
                        e.Params["_ip"].Value = saveNode;
                        e.Params["_dp"].Value = saveNode;
                        RaiseActiveEvent(
                            "magix.execute",
                            e.Params);
                    }
                    finally
                    {
                        e.Params["_ip"].Value = oldIp;
                        e.Params["_dp"].Value = oldDp;
                        e.Params["_root-only-execution"].UnTie();
                    }
                }
                ActiveEvents.Instance.CreateEventMapping(activeEvent, newActiveEvent);
            }
            else
            {
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                    DataBaseRemoval.Remove(activeEvent, "magix.execute.override", e.Params);

                ActiveEvents.Instance.RemoveMapping(activeEvent);
            }
		}
	}
}

