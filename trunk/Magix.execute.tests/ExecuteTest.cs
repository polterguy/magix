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
			tmp["Data"].Value = "not-set";
			tmp["if"].Value = "[Data].Value==not-set";
			tmp["if"]["set"].Value = "[Data].Value";
			tmp["if"]["set"]["value"].Value = "new-value";

			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"Tests to see if basic magix.execute
functionality works, specifically ""set"" on a 
Data node in the Node tree. Throws an exception 
unless the ""set"" operation executed successfully.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["Data"].Get<string>() != "new-value")
				throw new ApplicationException(
					string.Format(
						"Set didn't update as supposed to, expected {0}, got {1}",
						"new-value",
					tmp["Data"].Get<string>()));
		}

		/**
		 * Tests to see if "magix.execute" works
		 */
		[ActiveEvent(Name = "magix.test.execute-context")]
		public void magix_test_execute_context(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();
			tmp["Data"].Value = "not-set";
			tmp["Data"]["set"].Value = "[Data].Value";
			tmp["Data"]["set"]["value"].Value = "new-value";
			tmp["execute"].Value = "[Data]";

			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"Tests to see if basic magix.execute
functionality works, specifically ""set"" on a 
Data node in the Node tree. Throws an exception 
unless the ""set"" operation executed successfully.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["Data"].Get<string>() != "new-value")
				throw new ApplicationException(
					string.Format(
						"Set didn't update as supposed to, expected {0}, got {1}",
						"new-value",
					tmp["Data"].Get<string>()));
		}

		/**
		 * Tests to see if "magix.execute" works with slightly more complex code
		 */
		[ActiveEvent(Name = "magix.test.execute-complex-statement-1")]
		public void magix_test_execute_complex_statement_1(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"].Value = "not-set";
			tmp["if"].Value = "[Data].Value==not-set";
			tmp["if"]["event"].Value = "foo.bar";
			tmp["if"]["event"]["code"]["set"].Value = "[/][P][Data].Value";
			tmp["if"]["event"]["code"]["set"]["value"].Value = "new-value";
			tmp["if"]["foo.bar"].Value = null;
			tmp.Add (new Node("event", "foo.bar"));


			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"Tests to see if basic magix.execute
functionality works, specifically ""set"" on a 
Data node in the Node tree. Throws an exception 
unless the ""set"" operation executed successfully.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["if"]["foo.bar"]["Data"].Get<string>() != "new-value")
				throw new ApplicationException(
					string.Format(
						"Set didn't update as supposed to, expected {0}, got {1}",
						"new-value",
					tmp["Data"].Get<string>()));
		}
	}
}

