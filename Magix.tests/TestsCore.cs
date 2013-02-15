/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.tests
{
	/**
	 * Controller for Unit Tests logic. Helps run all Unit Tests within the system, as long as they're
	 * prefixed with "magix.tests." as the start of their active event name
	 */
	public class TestsCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.tests.run-tests")]
		public void magix_tests_run_tests (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["inspect"].Value = @"Will raise all active events that
start with ""magix.test."", and treat them as Unit Tests. 
Which means that if some parts of the active event throws 
an exception, then the test will be considered 'failing', and
a red message box will show. If no test fails, the message box 
will be green, and it will show the number of tests which have 
been running. To create a new Unit Test, create an active event 
handler, that is within the ""magix.test"" namespace, 
and it will automatically register as a part of the
test-suite, which is to be ran, as this active event
is raised. If it fails, it will abort all other tests,
and show you the name of the active event handler
that failed.";
				foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
				{
					if (idx.StartsWith ("magix.test."))
					{
						e.Params[idx].Value = null;

						Node tmp = new Node();
						tmp["inspect"].Value = null;

						RaiseEvent (
							idx, 
							tmp);
						e.Params[idx].Add (tmp["inspect"].UnTie ());
					}
				}
				return;
			}
			string lastTest = "";
			try
			{
				int idxNo = 0;

				// Loops through all active events in the system, and raises
				// all event that starts with "magix.test."
				foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
				{
					if (idx.StartsWith ("magix.test."))
					{
						// Assuming this is a test ...
						idxNo += 1;
						lastTest = idx;
						RaiseEvent (idx);
					}
				}
				Node node = new Node();

				node["message"].Value = "SUCCESS!!<br /> " + idxNo + " tests successfully executed";
				node["color"].Value = "LightGreen";
				node["time"].Value = 30000;

				RaiseEvent (
					"magix.viewport.show-message",
					node);
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				Node node = new Node();

				node["message"].Value = "FAILED!!! While executing '" + lastTest + "', we got; '" + err.Message + "'";
				node["color"].Value = "Red";
				node["time"].Value = 30000;

				RaiseEvent (
					"magix.viewport.show-message",
					node);
			}
		}
	}
}

