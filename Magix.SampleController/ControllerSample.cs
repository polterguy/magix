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
			if (e.Params.Count == 0)
			{
				e.Params["IsPostBack"].Value = false;
			}
			else if(!e.Params["IsPostBack"].Get<bool>())
			{
				LoadModule ("Magix.SampleModules.ButtonSample", "content1");
			}
		}

		[ActiveEvent(Name = "Magix.Samples.FirstEvent")]
		public void Magix_Samples_FirstEvent (object sender, ActiveEventArgs e)
		{
			if (e.Params.Count == 0)
			{
				e.Params["Message"].Value = "Message from caller...";
			}
			else
			{
				Node node = new Node();
				node["Message"].Value = e.Params["Message"].Get<string>("No Message ... :(");
				node["MessageFromController"].Value = "Controller says 'Hi' Too :)";
				LoadModule ("Magix.SampleModules.ComplexSample", "content2", node);
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
	}
}

