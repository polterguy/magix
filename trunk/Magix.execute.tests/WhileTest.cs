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
	public class WhileTest : ActiveController
	{
		/**
		 * Tests to see if "while", works
		 */
		[ActiveEvent(Name = "magix.test.execute.while")]
		public void magix_test_execute_while(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["msg1"]["message"].Value = "msg1";
			tmp["Data"]["msg2"]["message"].Value = "msg1";
			tmp["Data"]["msg3"]["message"].Value = "msg1";
			tmp["while"].Value = "[Data].Count!=0";
			tmp["while"]["set"].Value = "[Data][0]";

			if (e.Params.Contains("inspect"))
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

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Data"].Count != 0)
			{
				throw new ApplicationException(
					"Failure of executing while statement");
			}
		}
	}
}

