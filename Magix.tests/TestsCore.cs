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
	 */
	public class TestsCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.tests.run-tests")]
		public void magix_tests_run_tests (object sender, ActiveEventArgs e)
		{
			string lastTest = "";
			try
			{
				int idxNo = 0;
				foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
				{
					if (idx.StartsWith ("magix.tests.") && idx != "magix.tests.run-tests")
					{
						// Assuming this is a test ...
						idxNo += 1;
						lastTest = idx;
						RaiseEvent (idx);
					}
				}
				Node node = new Node();

				node["message"].Value = "SUCCESS!! " + idxNo + " tests successfully executed";
				node["color"].Value = "LightGreen";
				node["time"].Value = 5000;

				RaiseEvent (
					"magix.viewport.show-message",
					node);
			}
			catch(Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				Node node = new Node();

				node["message"].Value = "While executing '" + lastTest + "', we got; '" + err.Message + "'";
				node["color"].Value = "Red";
				node["time"].Value = 30000;

				RaiseEvent (
					"magix.viewport.show-message",
					node);
			}
		}
	}
}

