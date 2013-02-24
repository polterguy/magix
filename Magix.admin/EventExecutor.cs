/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.admin
{
	/**
	 * Controller logic for the Active Event Executor. Helps load the executor, and 
	 * other support functions
	 */
	public class EventExecutor : ActiveController
	{
		/**
		 * Returns the Active Events registered in the system. This includes
		 * also the Active events which are temporarily loaded, due to Active
		 * Modules and similar which are registering events. Returns
		 * all Active Events as a list of Node in the "ActiveEvent" child node
		 */
		[ActiveEvent(Name = "magix.admin.get-active-events")]
		public static void magix_admin__get_active_events(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["all"].Value = false;
				e.Params["begins-with"].Value = "magix.execute.";
				e.Params["inspect"].Value = @"Will return all the Active Events currently registered
within the system. Notice that this can CHANGE as the system runs, due to Event Overriding.
Will return a list of events within the ""ActiveEvents"" node where the Value is the name
of the Active Event you can raise. Underneath each event, it will contain a ""CSS"" node
which can have the value of ""description-error"", ""description"" and ""description-important"". Error means it's an overridden
System event, at which case you should be on the look-out, and alert. notice means it's an
overridden and dynamically created event and description means it's a System event, an event in 
Code that is. In addition it will also have a ""Tooltip"" node, in case it's an overridden 
event, where the ""ToolTip"" will contain the original event name. Takes no parameters";
				return;
			}
			bool takeAll = false;
			if (e.Params.Contains("all"))
				takeAll = e.Params["all"].Get<bool>();

			string beginsWith = null;
			if (e.Params.Contains("begins-with"))
				beginsWith = e.Params["begins-with"].Get<string>();

			Node node = e.Params;
			int idxNo = 0;
			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (!takeAll && string.IsNullOrEmpty (beginsWith) && idx.StartsWith("magix.test."))
					continue;

				if (idx.Contains("."))
				{
					string[] splits = idx.Split ('.');
					if (!takeAll && splits[splits.Length - 1].StartsWith("_"))
						continue; // "Hidden" event ...
				}

				if (!string.IsNullOrEmpty (beginsWith) && !idx.StartsWith(beginsWith))
					continue;

				if (ActiveEvents.Instance.IsOverrideSystem (idx))
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "&nbsp;" : idx;
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "description-error";
					node["ActiveEvents"]["no_" + idxNo.ToString()]["ToolTip"].Value = 
						ActiveEvents.Instance.GetEventMappingValue (idx);
				}
				else if (ActiveEvents.Instance.IsOverride (idx))
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()].Value = idx;
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "description-notice";
					node["ActiveEvents"]["no_" + idxNo.ToString()]["ToolTip"].Value = 
						ActiveEvents.Instance.GetEventMappingValue (idx);
				}
				else if (idx != "" /* Skipping System Null Overrides */)
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()].Value = idx;
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "description";
				}
				idxNo += 1;
			}
			node["ActiveEvents"].Sort (
				delegate(Node left, Node right)
				{
					return ((string)left.Value).CompareTo (right.Value as String);
				});
		}

		/**
		 * Opens up the Event Sniffer, which allows you to spy
		 * on all events internally raised within the system
		 */
		[ActiveEvent(Name = "magix.admin.open-event-sniffer")]
		public void magix_admin_open_event_sniffer(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["container"].Value = "content1";
				e.Params["inspect"].Value = @"Will open the Event Sniffer
form, in ""container"" container in viewport, 
which will show all active events raised in 
the system, for every postback to the server,
allowing you to facilitate for debugging of
your server.";
				return;
			}
			LoadModule (
				"Magix.admin.EventSniffer", 
				e.Params["container"].Get<string>());
		}

		/**
		 * Loads the Active Event viewer in the given "container" Viewport Container.
		 * The Active Event viewer allows you to see all events in the system, and also
		 * execute arbitrary events with nodes as arguments
		 */
		[ActiveEvent(Name = "magix.admin.open-event-viewer")]
		public void magix_admin_open_event_viewer(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["container"].Value = "content1";
				e.Params["inspect"].Value = @"Will open the Active Event Executor, from which
you can run Active Events and inspect them and do other Meta/Admin Operations. Also
serves like a development environment from which you can start at. It will load
the module into the ""container"" viewport container.";
				return;
			}
			LoadModule (
				"Magix.admin.ExecutorForm", 
				e.Params["container"].Get<string>());

			RaiseEvent("magix.execute._event-overridden");
		}
	}
}
