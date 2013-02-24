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
		[ActiveEvent(Name = "magix.test.transform-node-code")]
		public void magix_test_transform_node_code (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();
			tmp["Data"]["date"].Value = DateTime.Now.Date.AddHours (1).AddMinutes (4).AddSeconds (54);
			tmp["Data"]["HELLO"]["bool"].Value = true;
			tmp["decimal"].Value = 1.54M;
			tmp["int"].Value = 123456;
			tmp["string"].Value = @"asdfooh dsfoih sdfoih sdf
sdafih sdfoiih sdoifh sadf"" dsfouh sdfouh sdfouuh sdf"" sadfoih
sadfpijsdfpijsdfpoijsdafopijsdfoij!!!!!!!";

			Node input = new Node();
			input["JSON"].Value = tmp.Clone();

			RaiseEvent(
				"magix.admin._transform-node-2-code",
				input);

			string code = input["code"].Get<string>();

			Node tmp2 = new Node();
			tmp2["code"].Value = code;

			RaiseEvent(
				"magix.admin._transform-code-2-node",
				tmp2);

			if (!tmp.Equals(tmp2["JSON"].Get<Node>()))
				throw new ApplicationException("Couldn't successfully reproduce a Node through converting it to Code and back again ...");
		}
	}
}

