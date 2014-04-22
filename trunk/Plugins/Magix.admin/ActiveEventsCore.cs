/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using Magix.Core;

namespace Magix.admin
{
	/*
	 * admin controller
	 */
	public class ActiveEventsCore : ActiveController
	{
		/*
		 * get active events active event
		 */
		[ActiveEvent(Name = "magix.admin.get-active-events")]
		public static void magix_admin__get_active_events(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.get-active-events"].Value = null;
				e.Params["all"].Value = true;
				e.Params["open"].Value = false;
				e.Params["remoted"].Value = false;
				e.Params["overridden"].Value = false;
				e.Params["begins-with"].Value = "magix.execute.";
				e.Params["inspect"].Value = @"returns all active events 
within the system.&nbsp;&nbsp;add [all], [open], [remoted], [overridden] 
or [begins-with] to filter the events returned.&nbsp;&nbsp;active events 
are returned in [events].&nbsp;&nbsp;will not return unit tests and active events starting with _ 
if [all] is false.&nbsp;&nbsp;
if [all], [open], [remoted] or [overridden] is defined, it will return all events 
fullfilling criteria, regardless of whether or not they are private events, tests or
don't match the [begins-with] parameter.&nbsp;&nbsp;thread safe";
				return;
			}

            bool open = false;
            if (Ip(e.Params).Contains("open"))
                open = Ip(e.Params)["open"].Get<bool>();

            bool all = true;
            if (Ip(e.Params).Contains("all"))
                all = Ip(e.Params)["all"].Get<bool>();

            bool remoted = false;
            if (Ip(e.Params).Contains("remoted"))
                remoted = Ip(e.Params)["remoted"].Get<bool>();

			bool overridden = false;
            if (Ip(e.Params).Contains("overridden"))
                overridden = Ip(e.Params)["overridden"].Get<bool>();

			string beginsWith = null;
            if (Ip(e.Params).Contains("begins-with"))
                beginsWith = Ip(e.Params)["begins-with"].Get<string>();

            Node node = Ip(e.Params);
			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (open && !ActiveEvents.Instance.IsAllowedRemotely(idx))
					continue;
				if (remoted && string.IsNullOrEmpty(ActiveEvents.Instance.RemotelyOverriddenURL(idx)))
					continue;
                if (overridden && !ActiveEvents.Instance.IsOverride(idx))
                    continue;

                if (!all && !string.IsNullOrEmpty(beginsWith) && idx.Replace(beginsWith, "").Contains("_"))
                    continue;

                if (!all && idx.StartsWith("magix.test."))
                    continue;

                if (!string.IsNullOrEmpty(beginsWith) && !idx.StartsWith(beginsWith))
					continue;

				node["events"][string.IsNullOrEmpty(idx) ? "" : idx].Value = null;
			}

			node["events"].Sort (
				delegate(Node left, Node right)
				{
					return left.Name.CompareTo(right.Name);
				});
		}
	}
}
