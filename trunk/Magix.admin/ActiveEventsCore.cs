/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
				e.Params["all"].Value = false;
				e.Params["open"].Value = false;
				e.Params["remoted"].Value = false;
				e.Params["overridden"].Value = false;
				e.Params["begins-with"].Value = "magix.execute.";
				e.Params["inspect"].Value = @"returns all active events 
within the system.&nbsp;&nbsp;add [all], [open], [remoted], [overridden] 
or [begins-with] to filter the events returned.&nbsp;&nbsp;active events 
are returned in [events].&nbsp;&nbsp;by default, unit tests and active events starting with _
as their name, will not be returned, unless [all] is true.&nbsp;&nbsp;
if [all], [open], [remoted] or [overridden] is defined, it will return all events 
fullfilling criteria, regardless of whether or not they are private events, tests or
don't match the [begins-with] parameter.&nbsp;&nbsp;thread safe";
				return;
			}

			bool takeAll = false;
			if (e.Params.Contains("all"))
				takeAll = e.Params["all"].Get<bool>();

			bool open = false;
			if (e.Params.Contains("open"))
				open = e.Params["open"].Get<bool>();

			bool remoted = false;
			if (e.Params.Contains("remoted"))
				remoted = e.Params["remoted"].Get<bool>();

			bool overridden = false;
			if (e.Params.Contains("overridden"))
				overridden = e.Params["overridden"].Get<bool>();

			string beginsWith = null;
			if (e.Params.Contains("begins-with"))
				beginsWith = e.Params["begins-with"].Get<string>();

			Node node = e.Params;
			int idxNo = 0;
			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (open)
				{
					if (ActiveEvents.Instance.IsAllowedRemotely(idx))
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					idxNo += 1;
					continue;
				}
				if (remoted)
				{
					if (ActiveEvents.Instance.RemotelyOverriddenURL(idx) != null)
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					idxNo += 1;
					continue;
				}
				if (overridden)
				{
					if (ActiveEvents.Instance.IsOverride(idx))
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					idxNo += 1;
					continue;
				}
				if (!takeAll && string.IsNullOrEmpty(beginsWith) && idx.StartsWith("magix.test."))
					continue;

				if (idx.Contains("."))
				{
					string[] splits = idx.Split ('.');
					if (!takeAll && splits[splits.Length - 1].StartsWith("_"))
						continue; // "Hidden" event ...
				}

				if (!string.IsNullOrEmpty(beginsWith) && !idx.StartsWith(beginsWith))
					continue;

				node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
				idxNo += 1;
			}

			node["events"].Sort (
				delegate(Node left, Node right)
				{
					return ((string)left.Value).CompareTo(right.Value as String);
				});
		}
	}
}
