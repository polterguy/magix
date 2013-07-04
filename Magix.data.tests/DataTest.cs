/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
		[ActiveEvent(Name = "magix.test.data.save-by-id")]
		public static void magix_test_data_save_by_id(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"]["id"].Value = "data-save-test";
			tmp["execute"]["magix.data.load"]["id"].Value = "data-save-test";
			tmp["execute"]["if"].Value = "[execute][magix.data.load][object]";
			tmp["execute"]["if"]["throw"].Value = "object didn't delete";
			tmp["_buffer"].Value = null;
			tmp["magix.data.save"]["id"].Value = "data-save-test";
			tmp["magix.data.save"]["object"]["value"].Value = "thomas";
			tmp["magix.data.save"]["object"]["v_alue1"].Value = "thomas1";
			tmp["magix.data.save"]["object"]["_value2"].Value = "thomas2";
			tmp["magix.data.save"]["object"]["gok"]["value"].Value = "thomas3";
			tmp["magix.data.save"]["object"]["gokk"]["Value"].Value = "thomas4";
			tmp["magix.data.save"]["object"]["gokk"]["gokk2"]["Value"].Value = "thomas5";
			tmp["magix.data.save"]["object"]["gokk"]["gokk2"]["Value"].Value = "thomas6";
			tmp["magix.data.save"]["object"]["tmp"]["Value"].Value = "thomas7";
			tmp["magix.data.save"]["object222222"]["value"].Value = "thomas8";
			tmp["magix.data.save"]["object"]["object"]["object"]["object"]["object"]["Value"].Value = "thomas9";
			tmp["magix.data.save"]["object"]["object"]["object"]["Value"].Value = "thomas0";
			tmp["magix.data.save"]["object"]["Value"]["Value"]["Value"]["Value"]["Value"]["Value"].Value = "thomas2";
			tmp["magix.data.save"]["object5"]["Value"].Value = "thomas3";
			tmp["magix.data.save"]["object4"]["Value"].Value = "thomas|";
			tmp["magix.data.save"]["object"]["Value7"].Value = "thomash";
			tmp["magix.data.save"]["object"]["Value3"].Value = "thomasx£#$¤%&/()[]}±?";
			tmp["magix.data.load"]["id"].Value = "data-save-test";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that saving and loading
by [id], behaves as it should";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (!tmp["magix.data.load"]["objects"][0].HasNodes(tmp["magix.data.save"]["object"]))
			{
				throw new ApplicationException(
					"Failure of executing data-save/load statement with big object");
			}
		}

		/**
		 * Tests to see if "magix.data.save/load", works
		 */
		[ActiveEvent(Name = "magix.test.data.load-multiple-objects")]
		public static void magix_test_data_load_multiple_objects(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"]["id"].Value = "data-save-test1";
			tmp["magix.data.remove"]["id"].Value = "data-save-test2";
			tmp["magix.data.remove"]["id"].Value = "data-save-test3";
			tmp["_buffer"].Value = null;

			tmp["magix.data.save"]["id"].Value = "data-save-test1";
			tmp["magix.data.save"]["object"]["x_value"].Value = "thomas";
			tmp["magix.data.save"]["object"]["v_alue1"].Value = "thomas1";
			tmp["magix.data.save"]["object"]["_value2"].Value = "thomas2";

			tmp["magix.data.save2"]["id"].Value = "data-save-test2";
			tmp["magix.data.save2"]["object"]["x_value"].Value = "thomas";
			tmp["magix.data.save2"]["object"]["gokk"]["Value"].Value = "thomas4";
			tmp["magix.data.save2"]["object"]["gokk"]["gokk2"]["Value"].Value = "thomas5";
			tmp["magix.data.save2"]["object"]["gokk"]["gokk2"]["Value"].Value = "thomas6";
			tmp["magix.data.save2"]["object"]["tmp"]["Value"].Value = "thomas7";
			tmp["magix.data.save2"]["object222222"]["value"].Value = "thomas8";

			tmp["magix.data.save3"]["id"].Value = "data-save-test3";
			tmp["magix.data.save3"]["object"]["x_value"].Value = "thomas";
			tmp["magix.data.save3"]["object"]["object"]["object"]["Value"].Value = "thomas0";
			tmp["magix.data.save3"]["object"]["Value"]["Value"]["Value"]["Value"]["Value"]["Value"].Value = "thomas2";
			tmp["magix.data.save3"]["object5"]["Value"].Value = "thomas3";
			tmp["magix.data.save3"]["object4"]["Value"].Value = "thomas|";
			tmp["magix.data.save3"]["object"]["Value7"].Value = "thomash";
			tmp["magix.data.save3"]["object"]["Value3"].Value = "thomasx£#$¤%&/()[]}±?";
			tmp["magix.data.load"]["prototype"]["x_value"].Value = "thomas";

			// simplicity ...
			tmp["magix.data.save2"].Name = "magix.data.save";
			tmp["magix.data.save3"].Name = "magix.data.save";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that saving and loading
by [prototype], behaves as it should";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["magix.data.load"]["objects"].Count != 3)
			{
				throw new ApplicationException(
					"Failure of executing data-save/load statement with big object");
			}

			if (tmp["magix.data.load"]["objects"]["data-save-test3"]["Value3"].Get<string>() != "thomasx£#$¤%&/()[]}±?")
			{
				throw new ApplicationException(
					"Failure of executing data-save/load statement with big object");
			}
		}
	}
}

