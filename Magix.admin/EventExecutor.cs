/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.admin
{
	/**
	 */
	[ActiveController]
	public class EventExecutor : ActiveController
	{
		[ActiveEvent(Name = "magix.admin.get-active-events")]
		public void magix_admin__get_active_events (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
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
			Node node = e.Params;
			int idxNo = 0;
			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (idx.Contains ("."))
				{
					string[] splits = idx.Split ('.');
					if (splits[splits.Length - 1].StartsWith ("_"))
						continue; // "Hidden" event ...
				}
				node ["ActiveEvents"] ["no_" + idxNo.ToString()].Value = idx;
				if (ActiveEvents.Instance.IsOverrideSystem (idx))
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "description-error";
					node["ActiveEvents"]["no_" + idxNo.ToString()]["ToolTip"].Value = 
						ActiveEvents.Instance.GetEventMappingValue (idx);
				}
				else if (ActiveEvents.Instance.IsOverride (idx))
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "description-notice";
					node["ActiveEvents"]["no_" + idxNo.ToString()]["ToolTip"].Value = 
						ActiveEvents.Instance.GetEventMappingValue (idx);
				}
				else
				{
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

		[ActiveEvent(Name = "magix.admin.open-event-viewer")]
		public void magix_admin_open_event_viewer (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
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

			Node node = new Node();
			RaiseEvent ("magix.execute._event-overridden", node);
		}
	}
}
