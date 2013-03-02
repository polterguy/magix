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
		[ActiveEvent(Name = "magix.test.execute.set-node-list")]
		public void magix_test_execute_set_node_list(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item1"]["abc"].Value = null;
			tmp["Data"]["Item1"]["abc"]["aaa"].Value = 5;
			tmp["Data"]["Item1"]["abc"]["bbb"].Value = @"hello,
hello ""hello"" hello ....";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer][Copy]";
			tmp["set"]["value"].Value = "[Data]";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that using [set] to copy 
one node-list, and put it into another node, behaves correctly, and retain
the original correctly, and the new list is an exact replica";
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

			if (!tmp["Data"].HasNodes(original))
			{
				throw new ApplicationException(
					"The 'Data' Node didn't keep its original nodes");
			}
		}

		/**
		 * Tests to see if "set", works with Node-Lists
		 */
		[ActiveEvent(Name = "magix.test.execute.set-node-null")]
		public void magix_test_execute_set_node_null(object sender, ActiveEventArgs e)
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
				e.Params["inspect"].Value = @"verifies that setting a node to null, 
behaves correctly, and deletes the entire node, with its children";
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
		[ActiveEvent(Name = "magix.test.execute.set-value")]
		public void magix_test_execute_set_value(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data][Item1][Description].Value";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that setting a node's value
from another node's value, behaves correctly";
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
		[ActiveEvent(Name = "magix.test.execute.set-value-null")]
		public void magix_test_execute_set_value_null(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["set"].Value = "[Data][Item1][Description].Value";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that setting a node's value
to null, behaves correctly";
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
		[ActiveEvent(Name = "magix.test.execute.set-name-null")]
		public void magix_test_execute_set_name_null(object sender, ActiveEventArgs e)
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
				e.Params["inspect"].Value = @"verifies that attempting to set a node's name
to a null value, throws an exception, as it should";
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
		[ActiveEvent(Name = "magix.test.execute.get-name")]
		public void magix_test_execute_get_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data][Item1][0].Name";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that name is extracted 
correctly by setting another node's value to 
the name of a node";
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
		[ActiveEvent(Name = "magix.test.execute.set-name")]
		public void magix_test_execute_set_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = null;
			tmp["Buffer"]["DataTmp"].Value = null;
			tmp["set"].Value = "[Buffer][DataTmp].Name";
			tmp["set"]["value"].Value = "[Data][Item1][0].Name";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that setting a node's name 
behaves correctly, when set to another node's name";
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
		[ActiveEvent(Name = "magix.test.execute.value-2-list")]
		public void magix_test_execute_value_2_list(object sender, ActiveEventArgs e)
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
				e.Params["inspect"].Value = @"verifies that setting a value to a node-list
behaves correctly";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<Node>().Name != "Data" || 
			    tmp["Buffer"].Get<Node>() == null || 
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
		[ActiveEvent(Name = "magix.test.execute.name-2-list")]
		public void magix_test_execute_name_2_list(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Buffer"]["Item"].Value = null;
			tmp["set"].Value = "[Buffer][Item].Name";
			tmp["set"]["value"].Value = "[Data][Item1]";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that attempting to set a node's 
name to a node-list throws an exception";
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

