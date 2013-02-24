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
		[ActiveEvent(Name = "magix.test.for-each")]
		public void magix_test_for_each(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = "thomas1";
			tmp["Data"]["Item2"].Value = "thomas2";
			tmp["Data"]["Item3"].Value = "thomas3";
			tmp["Buffer"].Value = null;
			tmp["for-each"].Value = "[Data]";
			tmp["for-each"]["set"].Value = "[/][Buffer][[.].Name].Value";
			tmp["for-each"]["set"]["value"].Value = "[.].Value";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if for-each
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"]["Item1"].Get<string>() != "thomas1" || 
			    tmp["Buffer"]["Item2"].Get<string>() != "thomas2" ||
			    tmp["Buffer"]["Item3"].Get<string>() != "thomas3")
			{
				throw new ApplicationException(
					"Failure of executing for-each statement");
			}
		}
	}
}

