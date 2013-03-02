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
		[ActiveEvent(Name = "magix.test.data.save-by-key")]
		public void magix_test_data_save_by_key(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"].Value = "data-save-test";
			tmp["execute"]["magix.data.load"].Value = "data-save-test";
			tmp["execute"]["if"].Value = "[execute][magix.data.load][object]";
			tmp["execute"]["if"]["throw"].Value = "Object didn't delete";
			tmp["Buffer"].Value = null;
			tmp["magix.data.save"].Value = "data-save-test";
			tmp["magix.data.save"]["object"]["Value"].Value = "thomas";
			tmp["magix.data.save"]["object"]["Value1"].Value = "thomas1";
			tmp["magix.data.save"]["object"]["Value2"].Value = "thomas2";
			tmp["magix.data.save"]["object"]["gokk"]["Value"].Value = "thomas3";
			tmp["magix.data.save"]["object"]["gokk"]["Value"].Value = "thomas4";
			tmp["magix.data.save"]["object"]["gokk"]["gokk2"]["Value"].Value = "thomas5";
			tmp["magix.data.save"]["object"]["gokk"]["gokk2"]["Value"].Value = "thomas6";
			tmp["magix.data.save"]["object"]["tmp"]["Value"].Value = "thomas7";
			tmp["magix.data.save"]["object222222"]["Value"].Value = "thomas8";
			tmp["magix.data.save"]["object"]["object"]["object"]["object"]["object"]["Value"].Value = "thomas9";
			tmp["magix.data.save"]["object"]["object"]["object"]["Value"].Value = "thomas0";
			tmp["magix.data.save"]["object"]["Value"]["Value"]["Value"]["Value"]["Value"]["Value"].Value = "thomas2";
			tmp["magix.data.save"]["object5"]["Value"].Value = "thomas3";
			tmp["magix.data.save"]["object4"]["Value"].Value = "thomas|";
			tmp["magix.data.save"]["object"]["Value7"].Value = "thomash";
			tmp["magix.data.save"]["object"]["Value3"].Value = "thomasx";
			tmp["magix.data.load"].Value = "data-save-test";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that saving and loading
by id, or key, behaves as it should";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (!tmp["magix.data.load"]["object"].Equals (tmp["magix.data.save"]["object"]))
			{
				throw new ApplicationException(
					"Failure of executing data-save/load statement with big object");
			}
		}
	}
}

