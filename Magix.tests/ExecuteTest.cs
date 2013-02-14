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
	public class ExecuteTest : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.tests.execute-1")]
		public void magix_tests_execute_1 (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node("execute");
			tmp["Data"].Value = "not-set";
			tmp["if"].Value = "[Data].Value==not-set";
			tmp["if"]["set"].Value = "[Data].Value";
			tmp["if"]["set"]["value"].Value = "new-value";

			if (e.Params.Contains ("inspect"))
			{
				e.Params["inspect"].Value = @"Tests to see if basic magix.execute
functionality works, specifically ""set"" on a Data node in the Node tree.";
				e.Params.Add (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["Data"].Get<string>() != "new-value")
				throw new ApplicationException("Set didn't update as supposed to");
		}
	}
}

