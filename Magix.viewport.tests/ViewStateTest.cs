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
	 */
	public class ViewStateTest : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.test.viewstate")]
		public static void magix_test_viewstate(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event:magix.execute"].Value = null;
			tmp["inspect"].Value = @"verifies that [magix.viewport.set-viewstate] and get-viewstate
behaves correctly";
			tmp["magix.viewport.set-viewstate"]["id"].Value = "some-viewstate-id";
			tmp["magix.viewport.set-viewstate"]["value"]["node-hierarchy"]["hello"].Value = "jo!";
			tmp["magix.viewport.get-viewstate"]["id"].Value = "some-viewstate-id";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["magix.viewport.get-viewstate"]["value"]["node-hierarchy"]["hello"].Get<string>() != "jo!")
			{
				throw new ApplicationException(
					"failure of executing set/get-viewstate");
			}
		}
	}
}

