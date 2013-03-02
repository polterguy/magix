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
				e.Params["event:magix.admin.get-active-events"].Value = null;
				e.Params["all"].Value = false;
				e.Params["open"].Value = false;
				e.Params["remoted"].Value = false;
				e.Params["overridden"].Value = false;
				e.Params["begins-with"].Value = "magix.execute.";
				e.Params["inspect"].Value = @"returns all public active events registered 
within the system.&nbsp;&nbsp;add [all], [open], [remoted], [overridden] 
or [begins-with] to filter the events returned.&nbsp;&nbsp;active events 
are returned in [events]";
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
					{
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					}
					continue;
				}
				if (remoted)
				{
					if (ActiveEvents.Instance.RemotelyOverriddenURL(idx) != null)
					{
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					}
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

				if (overridden)
				{
					if (ActiveEvents.Instance.IsOverride(idx))
					{
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					}
				}
				else
				{
					node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
				}
				idxNo += 1;
			}
			node["events"].Sort (
				delegate(Node left, Node right)
				{
					return ((string)left.Value).CompareTo(right.Value as String);
				});
		}

		/**
		 * Loads code into active event executor, and loads executor
		 */
		[ActiveEvent(Name = "magix.admin.load-executor-code")]
		public void magix_admin_load_executor_code(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.load-executor-code"].Value = null;
				e.Params["inspect"].Value = @"opens active event executor
in content with given [code] Value";
				e.Params["code"].Value = @"
event:magix.execute
Data=>thomas
if=>[Data].Value==thomas
  magix.viewport.show-message
    message=>howdy world";
				return;
			}

			if (!e.Params.Contains("code"))
				throw new ArgumentException("Cannot load-executor-code without code");

			Node node = new Node();
			node["container"].Value = "content";

			RaiseEvent(
				"magix.admin.open-event-executor",
				node);

			node = new Node();
			node["code"].Value = e.Params["code"].Get<string>();

			RaiseEvent(
				"magix.admin.set-code",
				node);
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
				e.Params["inspect"].Value = @"will open active event sniffer, 
allowing you to spy on all active events being raised in your system";
				e.Params["event:magix.admin.open-event-sniffer"].Value = null;
				e.Params["container"].Value = "content";
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
		[ActiveEvent(Name = "magix.admin.open-event-executor")]
		public void magix_admin_open_event_executor(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.admin.open-even-executor"].Value = null;
				e.Params["container"].Value = "content";
				e.Params["inspect"].Value = @"opens the active event executor in content";
				return;
			}

			LoadModule(
				"Magix.admin.ExecutorForm", 
				e.Params["container"].Get<string>());

			RaiseEvent(
				"magix.execute._event-overridden");
		}
	}
}
