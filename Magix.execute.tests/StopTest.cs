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
	 * unit test for magix.execute.stop keyword
	 */
	public class StopTest : ActiveController
	{
		/**
		 * unit test for magix.execute.stop keyword
		 */
		[ActiveEvent(Name = "magix.test.execute.stop")]
		public static void magix_test_execute_stop(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["stop"].Value = null;
			tmp["throw"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [stop] behaves correctly, 
and throws";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);
		}
	}
}

