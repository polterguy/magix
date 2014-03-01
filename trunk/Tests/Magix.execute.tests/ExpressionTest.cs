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
	 */
	public class ExpressionTest : ActiveController
	{
		/**
		 * Tests to see if expressions behave
		 */
		[ActiveEvent(Name = "magix.test.expressions")]
		public static void magix_test_expressions(object sender, ActiveEventArgs e)
		{
			Node n = new Node();

			n["_data"]["value"]["item"]["car"]["make"].Value = "bmw";
			n["_data"]["value"].Add("item")["bike"]["make"].Value = "dbs";
			n["set"].Value = "[_data].Value";
			n["set"]["value"].Value = "[_data][value][item:1][bike][make].Value";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"tests to see if basic expressions behaves, 
trying out indexed de-referencing one node";
				e.Params.AddRange(n);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				n);

			if (n["_data"].Get<string>() != "dbs")
				throw new ArgumentException("expression didn't behave as expected");
		}

		/**
		 * Tests to see if expressions behave
		 */
		[ActiveEvent(Name = "magix.test.expressions-create-code")]
		public static void magix_test_expressions_create_code(object sender, ActiveEventArgs e)
		{
			Node n = new Node();

			n["set"].Value = "[set:2].Value";
			n["set"]["value"].Value = "\\[_data].Value";
			n.Add("set", "[set:2][value].Value");
			n[n.Count-1]["value"].Value = "success";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"tests to verify that it is possible 
to create code in front of the instruction pointer";
				e.Params.AddRange(n);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				n);

			if (n["_data"].Get<string>() != "success")
				throw new ArgumentException("expression didn't behave as expected");
		}
	}
}

