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
	 * Contains Unit Tests for "magix.execute.while" active event
	 */
	public class ReplaceTest : ActiveController
	{
		/**
		 * Tests to see if "replace", works
		 */
		[ActiveEvent(Name = "magix.test.execute.replace")]
		public static void magix_test_execute_replace(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"].Value = "msg1-msg2";
			tmp["replace"].Value = "[_data].Value";
			tmp["replace"]["what"].Value = "-msg2";
			tmp["replace"]["with"].Value = "xxx";
			tmp["replace", 1].Value = "[_data].Name";
			tmp["replace", 1]["what"].Value = "_da";
			tmp["replace", 1]["with"].Value = "_DA";
			tmp["replace", 2].Value = "[_DAta].Value";
			tmp["replace", 2]["what"].Value = "msg1";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [replace] 
behaves correctly";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_DAta"].Get<string>() != "xxx")
			{
				throw new ApplicationException(
					"Failure of executing replace statement");
			}
		}
	}
}

