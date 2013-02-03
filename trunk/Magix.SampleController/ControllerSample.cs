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
				Node node = new Node ();
				foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers) {
					node ["ActiveEvents"] [idx].Value = idx;
				}
				RaiseEvent ("Magix.Samples.PopulateEventViewer", node);

				if (e.Params["Container"].Get<string>() == "content1")
				{
					// By default, we empty content2 if viewport container is content1
					node = new Node ();
					node ["Container"].Value = "content2";
					RaiseEvent ("Magix.Core.ClearControls", node);
				}
			}
		}

		[ActiveEvent(Name = "Magix.Samples.LaunchJSONEditor")]
		public void Magix_Samples_LaunchJSONEditor (object sender, ActiveEventArgs e)
		{
			LoadModule ("Magix.SampleModules.NodeEditor", "content3");

			Node tmp = new Node ();
			if (e.Params.Contains ("JSON"))
			{
				tmp ["JSON"].Value = e.Params ["JSON"].Value;

				RaiseEvent (
				"Magix.Samples.PopulateNodeEditor",
				tmp);
			}
		}

		[ActiveEvent(Name = "Magix.Samples.CloseNodeEditor")]
		public void Magix_Samples_CloseNodeEditor (object sender, ActiveEventArgs e)
		{
			Node node = new Node();
			node["Container"].Value = "content3";
			RaiseEvent (
				"Magix.Core.ClearControls",
				node);
		}

		[ActiveEvent(Name = "Magix.Samples.CloseNodeEditorAndSave")]
		public void Magix_Samples_CloseNodeEditorAndSave (object sender, ActiveEventArgs e)
		{
			Node node = new Node();
			node["Container"].Value = "content3";
			RaiseEvent (
				"Magix.Core.ClearControls",
				node);

			Node node2 = new Node();
			node2["JSON"].Value = e.Params["JSON"].Value;
			RaiseEvent ("Magix.Samples.UpdateJSONEditor", node2);
		}
	}
}

