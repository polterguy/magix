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
	 * Contains Unit Tests for "magix.execute.for-each" active event
	 */
	public class ForEachTest : ActiveController
	{
		/**
		 * Tests to see if "for-each", works
		 */
		[ActiveEvent(Name = "magix.test.execute.for-each")]
		public void magix_test_execute_for_each(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = "thomas1";
			tmp["_data"]["item2"].Value = "thomas2";
			tmp["_data"]["item3"].Value = "thomas3";
			tmp["_buffer"].Value = null;
			tmp["for-each"].Value = "[_data]";
			tmp["for-each"]["set"].Value = "[/][_buffer][[.].Name].Value";
			tmp["for-each"]["set"]["value"].Value = "[.].Value";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [for-each] iterates correctly
over a three items long list of items, by copying them
into another node, iteratively, and verify they were all 
copied correctly afterwards";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"]["item1"].Get<string>() != "thomas1" || 
			    tmp["_buffer"]["item2"].Get<string>() != "thomas2" ||
			    tmp["_buffer"]["item3"].Get<string>() != "thomas3")
			{
				throw new ApplicationException(
					"Failure of executing for-each statement");
			}
		}
	}
}

