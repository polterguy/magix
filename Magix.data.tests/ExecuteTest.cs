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
	public class DataTest : ActiveController
	{
		/**
		 * Tests to see if "magix.data.save/load", works
		 */
		[ActiveEvent(Name = "magix.test.data-save-load-by-key")]
		public void magix_test_data_save_load_by_key (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"].Value = "data-save-test";
			tmp["execute"]["magix.data.load"].Value = "data-save-test";
			tmp["execute"]["if"].Value = "[execute][magix.data.load][object]";
			tmp["execute"]["if"]["throw"].Value = "Object didn't delete";
			tmp["Buffer"].Value = null;
			tmp["magix.data.save"].Value = "data-save-test";
			tmp["magix.data.save"]["object"]["Value"].Value = "thomas";
			tmp["magix.data.load"].Value = "data-save-test";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if creating a new thread
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["magix.data.load"]["object"]["Value"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing fork statement");
			}
		}
	}
}

