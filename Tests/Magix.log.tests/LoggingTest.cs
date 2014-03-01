/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.tests
{
	/**
	 * Contains Unit Tests for "magix.log.append" active event
	 */
	public class LogTest : ActiveController
	{
		/**
		 * tests to see if magix.log.append functions
		 */
		[ActiveEvent(Name = "magix.test.append-2-log")]
		public static void magix_test_append_2_log(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["header"].Value = "test log item header";
			tmp["body"].Value = "test log item body";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.log.append"].Value = null;
				e.Params["inspect"].Value = @"verifies that the creation of a log item 
behaves as it should";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.log.append",
				tmp);
		}
	}
}

