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
	}
}
