/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.SampleController
{
	[ActiveController]
	public class ControllerSample : ActiveController
	{
		[ActiveEvent(Name = "Magix.Core.PageLoad")]
		public void Magix_Core_PageLoad (object sender, ActiveEventArgs e)
		{
			// This is just a dummy PageLoad Event Handler. 
			// Replace with your OWN logic and Controller
			if (e.Params.Count == 0)
			{
				e.Params["IsPostBack"].Value = false;
			}
			else if(!e.Params["IsPostBack"].Get<bool>())
			{
				Node tmp = new Node();
				tmp["Container"].Value = "content1";
				RaiseEvent(
					"Magix.Samples.OpenEventViewer", 
					tmp);
			}
		}

		[ActiveEvent(Name = "Magix.Samples._GetActiveEvents")]
		public void Magix_Samples__GetActiveEvents (object sender, ActiveEventArgs e)
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
		}

		[ActiveEvent(Name = "Magix.Samples.OpenEventViewer")]
		public void Magix_Samples_OpenEventViewer (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Container"))
			{
				e.Params["Container"].Value = "content1";
			}
			else
			{
				LoadModule ("Magix.SampleModules.EventViewer", e.Params["Container"].Get<string>());
				Node node = new Node();
				RaiseEvent ("Magix.Samples._GetActiveEvents", node);
				RaiseEvent ("Magix.Samples._PopulateEventViewer", node);
			}
		}
	}
}

