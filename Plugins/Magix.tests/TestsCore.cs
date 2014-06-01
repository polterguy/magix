/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.tests
{
	/*
     * controller for running unit tests
	 */
	public class TestsCore : ActiveController
	{
		/*
		 * runs all unit tests
		 */
		[ActiveEvent(Name = "magix.tests.run")]
		public static void magix_tests_run(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.tests",
                    "Magix.tests.hyperlisp.inspect.hl",
                    "[magix.tests.run-dox].Value");
				int av = 0;
				int tests = 0;
                ip["active-events"].Value = null; // to make sure it ends up at the top
                foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
				{
					av += 1;
                    if (idx == "magix.tests.run")
                        continue;
                    if (idx.StartsWith("magix.test."))
					{
						tests += 1;
                        ip["tests"][idx].Value = null;

						Node inspectTest = new Node();
						inspectTest["inspect"].Value = null;

						RaiseActiveEvent(
							idx, 
							inspectTest);

                        ip["tests"][idx].Add(inspectTest["inspect"]);
					}
				}
                ip["tests"].Value = tests;
                ip["active-events"].Value = av;
				return;
			}

            string lastTest = "";
            try
			{
				int idxNo = 0;
				DateTime start = DateTime.Now;

                List<string> tests = new List<string>(ActiveEvents.Instance.ActiveEventHandlers);
				foreach (string idxTest in tests)
				{
					if (idxTest.StartsWith("magix.test."))
					{
						idxNo += 1;
						lastTest = idxTest;
                        ip["tests"][idxTest]["success"].Value = false;

                        Node inspectTest = new Node();
                        inspectTest["inspect"].Value = null;
                        RaiseActiveEvent(
                            idxTest,
                            inspectTest);

                        bool expectsException = inspectTest.Contains("_expected-exception");
                        string expectedExceptionString = null;
                        if (expectsException)
                            expectedExceptionString = inspectTest["_expected-exception"].Get<string>();
                        try
                        {
                            RaiseActiveEvent(idxTest);
                            if (expectsException)
                                throw new ApplicationException("unit test expected an exception, which didn't occur");
                        }
                        catch(Exception err)
                        {
                            if (expectsException)
                            {
                                while (err.InnerException != null)
                                    err = err.InnerException;
                                if (!string.IsNullOrEmpty(expectedExceptionString))
                                    if (expectedExceptionString != err.Message)
                                        throw;
                            }
                            else
                                throw;
                        }
                        ip["tests"][idxTest]["success"].Value = true;
					}
				}

				DateTime end = DateTime.Now;

                ip["summary"]["no-tests"].Value = idxNo;
                ip["summary"]["success"].Value = true;

                int time = (int)(end - start).TotalMilliseconds;
                
                Node node = new Node();
				node["message"].Value = "success!<br /> " + idxNo + " tests successfully executed in " + time + " milliseconds";
				node["color"].Value = "LightGreen";
				node["time"].Value = 5000;

				RaiseActiveEvent(
					"magix.viewport.show-message",
					node);

				node = new Node();

				node["header"].Value = "unit tests executed successfully";
				node["body"].Value = "magix.tests.run was running successfully, " + 
                    idxNo + " tests executed successfully in " + time + " milliseconds";

				RaiseActiveEvent(
					"magix.log.append",
					node);
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

                ip["summary"]["success"].Value = false;
                ip["summary"]["description"].Value = err.Message;

				Node node = new Node();

				node["message"].Value = "failed!<br />while executing '" + lastTest + "', we got; '" + err.Message + "'";
				node["color"].Value = "Red";
				node["time"].Value = -1;

				RaiseActiveEvent(
					"magix.viewport.show-message",
					node);
				
				node = new Node();

                node["error"].Value = true;
                node["header"].Value = "unit tests executed with errors";
                node["body"].Value = "magix.tests.run did not execute successfully, message from system; " + err.Message;

				RaiseActiveEvent(
					"magix.log.append",
					node);
			}
		}
	}
}

