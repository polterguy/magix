/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.SampleController
{
	/**
	 * Level2: Default Controller in Magix, loads up Event Viewer such
	 * that you can start developing immediately. Remove this Controller
	 * for your own projects, and override page-load yourself.
	 */
	[ActiveController]
	public class ControllerSample : ActiveController
	{
		[ActiveEvent(Name = "magix.core.page-load")]
		public void magix_core_page_load (object sender, ActiveEventArgs e)
		{
			// Loads up Event Viewer, or IDE
			Node tmp = new Node();
			tmp["Container"].Value = "content1";
			RaiseEvent(
				"magix.samples.open-event-viewer", 
				tmp);
		}

		[ActiveEvent(Name = "magix.samples._get-active-events")]
		public void magix_samples__get_active_events (object sender, ActiveEventArgs e)
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

		[ActiveEvent(Name = "magix.samples.open-event-viewer")]
		public void magix_samples_open_event_viewer (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Container"))
			{
				e.Params["Container"].Value = "content1";
			}
			else
			{
				LoadModule (
					"Magix.SampleModules.EventViewer", 
					e.Params["Container"].Get<string>());

				Node node = new Node();
				RaiseEvent (
					"magix.samples._get-active-events", 
					node);

				RaiseEvent (
					"magix.samples._populate-event-viewer", 
					node);
			}
		}
	}
}

