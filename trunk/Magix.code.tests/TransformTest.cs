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
	 * Contains Unit Tests for Magix.code project
	 */
	public class TransformTest : ActiveController
	{
		/**
		 * Tests to see if a conversion of a complex Node tree, between code, and 
		 * back again, can re-create the original Node-List, also with different types
		 */
		[ActiveEvent(Name = "magix.test.code.node-2-code")]
		public void magix_test_code_code_2_node(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["date"].Value = DateTime.Now.Date.AddHours (1).AddMinutes (4).AddSeconds (54);
			tmp["_data"]["hello"]["bool"].Value = true;
			tmp["decimal"].Value = 1.54M;
			tmp["int"].Value = 123456;
			tmp["string1"].Value = @"asdfooh dsfoih sdfoih sdf
sdafih sdfoiih sdoifh sadf"" dsfouh sdfouh sdfouuh sdf"" sadfoih
sadfpijsdfpijsdfpoijsdafopijsdfoij""";
			tmp["string2"].Value = @"asdfooh dsfoih sdfoih sdf";
			tmp["string3"].Value = @"asdfooh dsfoih sdfoih sdf""";
			tmp["string4"].Value = @"asdfooh dsfoih sdfoih sdf""""""""";
			tmp["string5"].Value = @"asdfooh dsfoih sdfoih sdf




THOMAS""""""""";
			tmp["expr"]["deep"]["deep1"]["deep2"].Value = @"[tjobing]";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that a node serialize and
de-serialize down to a value, or string, behaves flawlessly, by comparing 
the before and after results";
				e.Params.AddRange(tmp);
				return;
			}

			Node input = new Node();
			input["json"].Value = tmp.Clone();

			RaiseActiveEvent(
				"magix.code.node-2-code",
				input);

			string code = input["code"].Get<string>();

			Node tmp2 = new Node();
			tmp2["code"].Value = code;

			RaiseActiveEvent(
				"magix.code.code-2-node",
				tmp2);

			if (!tmp.Equals(tmp2["json"].Get<Node>()))
				throw new ApplicationException("Couldn't successfully reproduce a Node through converting it to Code and back again ...");
		}
	}
}

