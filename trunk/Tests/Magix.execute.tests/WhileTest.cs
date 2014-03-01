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
	 * Contains Unit Tests for "magix.execute.while" active event
	 */
	public class WhileTest : ActiveController
	{
		/**
		 * Tests to see if "while", works
		 */
		[ActiveEvent(Name = "magix.test.execute.while")]
		public static void magix_test_execute_while(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["msg1"]["message"].Value = "msg1";
			tmp["_data"]["msg2"]["message"].Value = "msg1";
			tmp["_data"]["msg3"]["message"].Value = "msg1";
			tmp["while"].Value = "[_data].Count!=0";
			tmp["while"]["set"].Value = "[_data][0]";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [while] behaves correctly, 
by removing zeroth element of node-list every iteration, until while 
statement returns false.&nbsp;&nbsp;verifies afterwards that no items 
are left in the node-list";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_data"].Count != 0)
			{
				throw new ApplicationException(
					"Failure of executing while statement");
			}
		}
	}
}

