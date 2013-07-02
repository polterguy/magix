/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
		 * Will run all Unit Tests in the system
		 */
		[ActiveEvent(Name = "magix.tests.run")]
		public static void magix_tests_run(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"will raise all active events that
start with 'magix.test.', and treat them as unit tests.&nbsp;&nbsp;
if some parts of the active event throws 
an exception, then the test will be considered failing, and
a red message box will show.&nbsp;&nbsp;if no test fails, the message box 
will be green, and it will show the number of tests which have 
been running.&nbsp;&nbsp;to create a new unit test, create an active event 
handler, that is within the magix.test namespace, 
and it will automatically register as a part of the
test-suite, which is to be executed, as this active event
is raised.&nbsp;&nbsp;if it fails, it will abort all other tests,
and show you the name of the active event handler
that failed.&nbsp;&nbsp;thread safe";
				int av = 0;
				int tests = 0;
				foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
				{
					av += 1;
					if (idx.StartsWith("magix.test."))
					{
						tests += 1;
						e.Params[idx].Value = null;

						Node tmp = new Node();
						tmp["inspect"].Value = null;

						RaiseActiveEvent(
							idx, 
							tmp);
						e.Params[idx].Add (tmp["inspect"].UnTie());
					}
				}
				e.Params["tests"].Value = tests;
				e.Params["active-events"].Value = av;
				return;
			}
			string lastTest = "";
			try
			{
				int idxNo = 0;

				DateTime start = DateTime.Now;

				// Loops through all active events in the system, and raises
				// all event that starts with "magix.test."
				foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
				{
					if (idx.StartsWith("magix.test."))
					{
						// Assuming this is a test ...
						idxNo += 1;
						lastTest = idx;
						e.Params["tests"][idx]["success"].Value = false;
						RaiseActiveEvent(idx);
						e.Params["tests"][idx]["success"].Value = true;
					}
				}

				DateTime end = DateTime.Now;

				e.Params["summary"]["no-tests"].Value = idxNo;
				e.Params["summary"]["success"].Value = true;

				Node node = new Node();

				int time = (int)(end - start).TotalMilliseconds;

				node["message"].Value = "success!<br /> " + idxNo + " tests successfully executed in " + time + " milliseconds";
				node["color"].Value = "LightGreen";
				node["time"].Value = 5000;

				RaiseActiveEvent(
					"magix.viewport.show-message",
					node);

				node = new Node();

				node["header"].Value = "unit tests executed successfully";
				node["body"].Value = "magix.tests.run was running successfully, " + idxNo + " tests executed successfully";

				RaiseActiveEvent(
					"magix.log.append",
					node);
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				e.Params["summary"]["success"].Value = false;
				e.Params["summary"]["description"].Value = err.Message;

				Node node = new Node();

				node["message"].Value = "failed!<br />while executing '" + lastTest + "', we got; '" + err.Message + "'";
				node["color"].Value = "Red";
				node["time"].Value = -1;

				RaiseActiveEvent(
					"magix.viewport.show-message",
					node);
				
				node = new Node();

				node["header"].Value = "unit tests executed with errors";
				node["body"].Value = "magix.tests.run did not execute successfully, message from system; " + err.Message;

				RaiseActiveEvent(
					"magix.log.append",
					node);
			}
		}
	}
}

