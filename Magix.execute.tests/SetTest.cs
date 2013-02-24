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
	 * Contains Unit Tests for "magix.execute.set" active event
	 */
	public class SetTest : ActiveController
	{
		/**
		 * Tests to see if "set", works with Node-Lists
		 */
		[ActiveEvent(Name = "magix.test.set-node-list")]
		public void magix_test_set_node_list(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer][Copy]";
			tmp["set"]["value"].Value = "[Data]";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if copying of 
lists of Nodes is functioning as it should, 
by copying the contents from ""Data"" to
""Buffer"". Then checks to see if the ""Buffer""
Node contains a copy of the original Node-set.";
				e.Params.AddRange(tmp);
				return;
			}

			Node original = tmp["Data"].Clone();

			RaiseEvent(
				"magix.execute",
				tmp);

			if (!tmp["Buffer"]["Data"].HasNodes(original))
			{
				throw new ApplicationException("The 'Buffer' Node didn't equal the original Node-list in the 'Data' Node as were expected");
			}
		}

		/**
		 * Tests to see if "set", works with Node-Lists
		 */
		[ActiveEvent(Name = "magix.test.set-node-list-null")]
		public void magix_test_set_node_list_null(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Items"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Items"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Items"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Items"]["Item4"]["Description"].Value = "desc4";
			tmp["set"].Value = "[Data][Items]";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if copying of 
lists of Nodes is functioning as it should, 
by setting the Data/Items to null, which
should remove the Data/Items node.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Data"].Contains("Items"))
			{
				throw new ApplicationException("The set operation didn't make the Node-list become null");
			}
		}

		/**
		 * Tests to see if "set", works with Values
		 */
		[ActiveEvent(Name = "magix.test.set-value")]
		public void magix_test_set_value(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data][Item1][Description].Value";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if copying of 
Node values is functioning as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "desc1")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"desc1",
						tmp["Buffer"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with null Values
		 */
		[ActiveEvent(Name = "magix.test.set-value-null")]
		public void magix_test_set_value_null(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["set"].Value = "[Data][Item1][Description].Value";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if setting a Node's
Value functions as it should with null values.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"]["Item1"]["Description"].Value != null)
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"desc1",
						tmp["Buffer"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with null Values
		 */
		[ActiveEvent(Name = "magix.test.set-name-null-throws")]
		public void magix_test_set_name_null_throws(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["try"].Value = null;
			tmp["try"]["code"]["set"].Value = "[/][Data][Item1][Description].Name";
			tmp["try"]["catch"]["set"].Value = "[/][Data].Value";
			tmp["try"]["catch"]["set"]["value"].Value = "success";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if setting a Node's
Name functions as it should with null values. Expected to throw!";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Data"].Get<string>() != "success")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"desc1",
						tmp["Data"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with Name
		 */
		[ActiveEvent(Name = "magix.test.set-get-name")]
		public void magix_test_set_get_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data][Item1][0].Name";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "Description123")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"Description123",
						tmp["Buffer"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with Name
		 */
		[ActiveEvent(Name = "magix.test.set-name")]
		public void magix_test_set_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = null;
			tmp["Buffer"]["DataTmp"].Value = null;
			tmp["set"].Value = "[Buffer][DataTmp].Name";
			tmp["set"]["value"].Value = "[Data][Item1][0].Name";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"][0].Name != "Description123")
			{
				throw new ApplicationException(
					string.Format(
						"Expected {0}, got {1}",
						"Description123",
						tmp["Buffer"][0].Name));
			}
		}

		/**
		 * Tests to see if ""set"" ing a Value to a Node-list throws an exception
		 */
		[ActiveEvent(Name = "magix.test.assure-set-lists-become-values")]
		public void magix_test_assure_set_lists_become_values(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data]";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if setting Node Value
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}
			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<Node>() == null || 
			    tmp["Buffer"].Get<Node>()["Item4"]["Description"].Get<string>() != "desc4" ||
			    tmp["Buffer"].Get<Node>().Count != 4)
			{
				throw new ApplicationException(
					"Couldn't get the right Node value of of a node's Value");
			}
		}

		/**
		 * Tests to see if ""set"" ing a Name to a Node-list throws an exception
		 */
		[ActiveEvent(Name = "magix.test.assure-set-lists-dont-become-names")]
		public void magix_test_assure_set_lists_dont_become_names(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"]["Item"].Value = null;
			tmp["set"].Value = "[Buffer][Item].Name";
			tmp["set"]["value"].Value = "[Data][Item1]";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should, by demanding an exception to
declare as success.";
				e.Params.AddRange(tmp);
				return;
			}

			bool hasException = false;
			try
			{
				RaiseEvent(
					"magix.execute",
					tmp);
			}
			catch
			{
				hasException = true;
			}

			if (!hasException)
			{
				throw new ApplicationException(
					"Expected exception due to assigning a Node-list to a Name, but didn't get one ...?");
			}
		}
	}
}

