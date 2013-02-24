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
		[ActiveEvent(Name = "magix.test.fork")]
		public void magix_test_fork(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"].Value = "fork-test-buffer-object";
			tmp["fork"]["Buffer"].Value = null;
			tmp["fork"]["magix.data.save"].Value = "fork-test-buffer-object";
			tmp["fork"]["magix.data.save"]["object"]["Value"].Value = "thomas";
			tmp["sleep"].Value = 500;
			tmp["magix.data.load"].Value = "fork-test-buffer-object";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if creating a new thread
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["magix.data.load"]["object"]["Value"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing fork statement");
			}
		}
	}
}

