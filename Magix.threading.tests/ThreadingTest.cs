/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.tests
{
	/**
	 * Contains Unit Tests for "magix.execute" active event
	 */
	public class ThreadingTest : ActiveController
	{
		/**
		 * Tests to see if "fork", works
		 */
		[ActiveEvent(Name = "magix.test.threads.fork")]
		public void magix_test_threads_fork(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"]["id"].Value = "fork-test-buffer-object";
			tmp["wait"].Value = null;
			tmp["wait"]["fork"]["magix.data.save"]["id"].Value = "fork-test-buffer-object";
			tmp["wait"]["fork"]["magix.data.save"]["object"]["value"].Value = "thomas";
			tmp["magix.data.load"]["id"].Value = "fork-test-buffer-object";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [fork] behaves correctly, 
by using magix.data.load and magix.data.save to signal between threads for success";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["magix.data.load"]["objects"][0]["value"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"failure of executing fork statement");
			}
		}

		/**
		 * Tests to see if "fork", works
		 */
		[ActiveEvent(Name = "magix.test.threads.fork-multiple-threads")]
		public void magix_test_threads_fork_multiple_threads(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove1"]["id"].Value = "fork-test-buffer-object1";
			tmp["magix.data.remove2"]["id"].Value = "fork-test-buffer-object2";
			tmp["magix.data.remove3"]["id"].Value = "fork-test-buffer-object3";
			tmp["magix.data.remove4"]["id"].Value = "fork-test-buffer-object4";
			tmp["magix.data.remove5"]["id"].Value = "fork-test-buffer-object5";
			tmp["magix.data.remove1"].Name = "magix.data.remove";
			tmp["magix.data.remove2"].Name = "magix.data.remove";
			tmp["magix.data.remove3"].Name = "magix.data.remove";
			tmp["magix.data.remove4"].Name = "magix.data.remove";
			tmp["magix.data.remove5"].Name = "magix.data.remove";

			tmp["wait"].Value = null;
			tmp["wait"]["fork1"]["magix.data.save"]["id"].Value = "fork-test-buffer-object1";
			tmp["wait"]["fork1"]["magix.data.save"]["object"]["value"].Value = "thomas1";
			tmp["wait"]["fork2"]["magix.data.save"]["id"].Value = "fork-test-buffer-object2";
			tmp["wait"]["fork2"]["magix.data.save"]["object"]["value"].Value = "thomas2";
			tmp["wait"]["fork3"]["magix.data.save"]["id"].Value = "fork-test-buffer-object3";
			tmp["wait"]["fork3"]["magix.data.save"]["object"]["value"].Value = "thomas3";
			tmp["wait"]["fork4"]["magix.data.save"]["id"].Value = "fork-test-buffer-object4";
			tmp["wait"]["fork4"]["magix.data.save"]["object"]["value"].Value = "thomas4";
			tmp["wait"]["fork5"]["magix.data.save"]["id"].Value = "fork-test-buffer-object5";
			tmp["wait"]["fork5"]["magix.data.save"]["object"]["value"].Value = "thomas5";
			tmp["wait"]["fork1"].Name = "fork";
			tmp["wait"]["fork2"].Name = "fork";
			tmp["wait"]["fork3"].Name = "fork";
			tmp["wait"]["fork4"].Name = "fork";
			tmp["wait"]["fork5"].Name = "fork";

			tmp["magix.data.load1"]["id"].Value = "fork-test-buffer-object1";
			tmp["magix.data.load2"]["id"].Value = "fork-test-buffer-object2";
			tmp["magix.data.load3"]["id"].Value = "fork-test-buffer-object3";
			tmp["magix.data.load4"]["id"].Value = "fork-test-buffer-object4";
			tmp["magix.data.load5"]["id"].Value = "fork-test-buffer-object5";

			tmp["magix.data.load1"].Name = "magix.data.load";
			tmp["magix.data.load2"].Name = "magix.data.load";
			tmp["magix.data.load3"].Name = "magix.data.load";
			tmp["magix.data.load4"].Name = "magix.data.load";
			tmp["magix.data.load5"].Name = "magix.data.load";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [fork] behaves correctly, 
by using magix.data.load and magix.data.save to signal between threads for success";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp[6]["objects"][0]["value"].Get<string>() != "thomas1")
			{
				throw new ApplicationException(
					"failure of executing fork statement");
			}

			if (tmp[7]["objects"][0]["value"].Get<string>() != "thomas2")
			{
				throw new ApplicationException(
					"failure of executing fork statement");
			}

			if (tmp[8]["objects"][0]["value"].Get<string>() != "thomas3")
			{
				throw new ApplicationException(
					"failure of executing fork statement");
			}

			if (tmp[9]["objects"][0]["value"].Get<string>() != "thomas4")
			{
				throw new ApplicationException(
					"failure of executing fork statement");
			}

			if (tmp[10]["objects"][0]["value"].Get<string>() != "thomas5")
			{
				throw new ApplicationException(
					"failure of executing fork statement");
			}
		}
	}
}

