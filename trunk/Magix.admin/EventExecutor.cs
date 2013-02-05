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
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "error";
					node["ActiveEvents"]["no_" + idxNo.ToString()]["ToolTip"].Value = 
						ActiveEvents.Instance.GetEventMappingValue (idx);
				}
				else if (ActiveEvents.Instance.IsOverride (idx))
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "notice";
					node["ActiveEvents"]["no_" + idxNo.ToString()]["ToolTip"].Value = 
						ActiveEvents.Instance.GetEventMappingValue (idx);
				}
				else
				{
					node["ActiveEvents"]["no_" + idxNo.ToString()]["CSS"].Value = "info";
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
			if (!e.Params.Contains ("container"))
			{
				e.Params["container"].Value = "content1";
			}
			else
			{
				LoadModule (
					"Magix.admin.ExecutorForm", 
					e.Params["container"].Get<string>());

				Node node = new Node();
				RaiseEvent ("magix.execute._event-overridden", node);
			}
		}
	}
}
