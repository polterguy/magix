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
	public class ExecuteTest : ActiveController
	{
		/**
		 * Tests to see if "if", "set" and "magix.execute" works
		 */
		[ActiveEvent(Name = "magix.test.execute")]
		public void magix_test_execute(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"].Value = "not-set";
			tmp["if"].Value = "[_data].Value==not-set";
			tmp["if"]["set"].Value = "[_data].Value";
			tmp["if"]["set"]["value"].Value = "new-value";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"tests to see if basic magix.execute
functionality works, by executing a simple [if] statement, and verify
that the result node set is manipulated as it should be";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["_data"].Get<string>() != "new-value")
				throw new ApplicationException(
					string.Format(
						"Set didn't update as supposed to, expected {0}, got {1}",
						"new-value",
					tmp["_data"].Get<string>()));
		}

		/**
		 * Tests to see if "magix.execute" works
		 */
		[ActiveEvent(Name = "magix.test.execute.goto")]
		public void magix_test_execute_goto(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();
			tmp["_data"].Value = "not-set";
			tmp["_data"]["set"].Value = "[_data].Value";
			tmp["_data"]["set"]["value"].Value = "new-value";
			tmp["execute"].Value = "[_data]";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that passing in a context into 
a magix.execute block of code correctly behaves by setting 
a node's value to new-value within the block being executed";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["_data"].Get<string>() != "new-value")
				throw new ApplicationException(
					string.Format(
						"Set didn't update as supposed to, expected {0}, got {1}",
						"new-value",
					tmp["_data"].Get<string>()));
		}
	}
}

