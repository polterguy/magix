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

			tmp["_data"]["item1"]["description"].Value = "desc1";
			tmp["_data"]["item1"]["abc"].Value = null;
			tmp["_data"]["item1"]["abc"]["aaa"].Value = 5;
			tmp["_data"]["item1"]["abc"]["bbb"].Value = @"hello,
hello ""hello"" hello ....";
			tmp["_data"]["item2"]["description"].Value = "desc2";
			tmp["_data"]["item3"]["description"].Value = "desc3";
			tmp["_data"]["item4"]["description"].Value = "desc4";
			tmp["_buffer"].Value = null;
			tmp["set"].Value = "[_buffer][copy]";
			tmp["set"]["value"].Value = "[_data]";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that using [set] to copy 
one node-list, and put it into another node, behaves correctly, and retain
the original correctly, and the new list is an exact replica";
				e.Params.AddRange(tmp);
				return;
			}

			Node original = tmp["_data"].Clone();

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (!tmp["_buffer"]["_data"].HasNodes(original))
			{
				throw new ApplicationException("The '_buffer' Node didn't equal the original Node-list in the 'Data' Node as were expected");
			}

			if (!tmp["_data"].HasNodes(original))
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

			tmp["_data"]["items"]["item1"]["description"].Value = "desc1";
			tmp["_data"]["items"]["item2"]["description"].Value = "desc2";
			tmp["_data"]["items"]["item3"]["description"].Value = "desc3";
			tmp["_data"]["items"]["item4"]["description"].Value = "desc4";
			tmp["set"].Value = "[_data][items]";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that setting a node to null, 
behaves correctly, and deletes the entire node, with its children";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_data"].Contains("items"))
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

			tmp["_data"]["item1"]["description"].Value = "desc1";
			tmp["_buffer"].Value = null;
			tmp["set"].Value = "[_buffer].Value";
			tmp["set"]["value"].Value = "[_data][item1][description].Value";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that setting a node's value
from another node's value, behaves correctly";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "desc1")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"desc1",
						tmp["_buffer"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with null Values
		 */
		[ActiveEvent(Name = "magix.test.execute.set-value-null")]
		public void magix_test_execute_set_value_null(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description"].Value = "desc1";
			tmp["set"].Value = "[_data][item1][description].Value";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that setting a node's value
to null, behaves correctly";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"]["item1"]["description"].Value != null)
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"desc1",
						tmp["_buffer"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with null Values
		 */
		[ActiveEvent(Name = "magix.test.execute.set-name-null")]
		public void magix_test_execute_set_name_null(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description"].Value = "desc1";
			tmp["try"].Value = null;
			tmp["try"]["code"]["set"].Value = "[/][_data][item1][description].Name";
			tmp["try"]["catch"]["set"].Value = "[/][_data].Value";
			tmp["try"]["catch"]["set"]["value"].Value = "success";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that attempting to set a node's name
to a null value, throws an exception, as it should";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_data"].Get<string>() != "success")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"desc1",
						tmp["_data"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with Name
		 */
		[ActiveEvent(Name = "magix.test.execute.get-name")]
		public void magix_test_execute_get_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = null;
			tmp["_buffer"].Value = null;
			tmp["set"].Value = "[_buffer].Value";
			tmp["set"]["value"].Value = "[_data][item1][0].Name";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that name is extracted 
correctly by setting another node's value to 
the name of a node";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "description123")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"description123",
						tmp["_buffer"].Get<string>()));
			}
		}

		/**
		 * Tests to see if "set", works with Name
		 */
		[ActiveEvent(Name = "magix.test.execute.set-name")]
		public void magix_test_execute_set_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = null;
			tmp["_buffer"]["tmp"].Value = null;
			tmp["set"].Value = "[_buffer][tmp].Name";
			tmp["set"]["value"].Value = "[_data][item1][0].Name";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that setting a node's name 
behaves correctly, when set to another node's name";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"][0].Name != "description123" || 
			    tmp["_buffer"].Count != 1)
			{
				throw new ApplicationException(
					string.Format(
						"Expected {0}, got {1}",
						"description123",
						tmp["_buffer"][0].Name));
			}
		}

		/**
		 * Tests to see if ""set"" ing a Value to a Node-list throws an exception
		 */
		[ActiveEvent(Name = "magix.test.execute.value-2-list")]
		public void magix_test_execute_value_2_list(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description"].Value = "desc1";
			tmp["_data"]["item2"]["description"].Value = "desc2";
			tmp["_data"]["item3"]["description"].Value = "desc3";
			tmp["_data"]["item4"]["description"].Value = "desc4";
			tmp["_buffer"].Value = null;
			tmp["set"].Value = "[_buffer].Value";
			tmp["set"]["value"].Value = "[_data]";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that setting a value to a node-list
behaves correctly";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<Node>().Name != "_data" || 
			    tmp["_buffer"].Get<Node>() == null || 
			    tmp["_buffer"].Get<Node>()["item4"]["description"].Get<string>() != "desc4" ||
			    tmp["_buffer"].Get<Node>().Count != 4)
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

			tmp["_data"]["item1"]["description"].Value = "desc1";
			tmp["_buffer"]["item"].Value = null;
			tmp["set"].Value = "[_buffer][item].Name";
			tmp["set"]["value"].Value = "[_data][item1]";

			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that attempting to set a node's 
name to a node-list throws an exception";
				e.Params.AddRange(tmp);
				return;
			}

			bool hasException = false;
			try
			{
				RaiseActiveEvent(
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

