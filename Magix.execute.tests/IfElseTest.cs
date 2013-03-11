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
	 * Contains Unit Tests for "magix.execute.if", "else-if" and "else" active events
	 */
	public class IfElseTest : ActiveController
	{
		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if")]
		public static void magix_test_execute_if(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][description123].Value==thomas";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [if] correctly behaves by 
comparing a node expression's value against a static value, 
and from within the [if], change a value to success";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-name")]
		public static void magix_test_execute_if_name(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][0].Name==description123";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that an [if] comparison which 
compares the name of a node against a static value behaves correctly";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-not-equals")]
		public static void magix_test_execute_if_not_equals(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1].Value!=thomas";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that a not-equals comparison
on [if] behaves correctly, by returning false when it is supposed to";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-single-value")]
		public static void magix_test_execute_if_single_value(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1].Value";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies checking for the existence of
a node's value returns true, when value is not null";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-single-node")]
		public static void magix_test_execute_if_single_node(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = null;
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1]";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies checking for the existence 
of a node within [if] returns true, when node exist";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-no-node")]
		public static void magix_test_execute_if_no_node(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = null;
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][item2]";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that a single-component [if]
statement behaves correctly when the node it looks for
does not exist.&nbsp;&nbsp;also verifies that the [if] 
does not generate a path, as it traverses the nodes";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}

			if (tmp["Data"]["Item1"].Contains("Item2"))
			{
				throw new ApplicationException(
					"Failure of executing if statement, if generated path as it was traversing!");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-value-null")]
		public static void magix_test_execute_if_value_null(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = null;
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1].Value";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that checking for the 
existence of a value, when it does not exist, behaves correctly
and return false when supposed to";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.nested-if")]
		public static void magix_test_execute_nested_if(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = "thomas";
			tmp["_buffer"].Value = "failure";
			tmp["if"].Value = "[_data]";
			tmp["if"]["if"].Value = "[_data][item1]";
			tmp["if"]["if"]["if"].Value = "![_data].Value";
			tmp["if"]["if"]["if"]["if"].Value = "[_data].Name==_data";
			tmp["if"]["if"]["if"]["if"]["if"].Value = "[_data][item1].Value==thomas";
			tmp["if"]["if"]["if"]["if"]["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["if"]["if"]["if"]["if"]["set"]["value"].Value = "success";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that nested [if] statements 
behaves as they should, by setting a node's value to success";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.if-not-value")]
		public static void magix_test_execute_if_not_value(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"].Value = null;
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "![_data][item1].Value";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that not'ing the returned 
value of an [if] that checks for the existence of a value behaves correctly,
by returning true when supposed to";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.elseif-single-param")]
		public static void magix_test_execute_elseif_single_param(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][description123].Value==thomas1";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[_data][item1][description123].Value";
			tmp["else-if"]["set"].Value = "[_buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [else-if] can positively 
return true for the existence of a value in a single 
component else-if expression";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else-if statement");
			}
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.elseif-node-exists")]
		public static void magix_test_execute_elseif_node_exists(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][description123].Value==thomas1";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[_data][item1][description123]";
			tmp["else-if"]["set"].Value = "[_buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [else-if] can positively
check if a node exist, as a single statement else-if expression";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else-if statement");
			}
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.execute.elseif-sub-expression")]
		public static void magix_test_execute_elseif_sub_expression(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["description123"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][description123].Value==thomas1";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[_data][item1][[0][0][0].Name].Value==thomas";
			tmp["else-if"]["set"].Value = "[_buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that sub-expression 
within [else-if] statements correctly behaves in a 
comparison expression between a node-set with a 
sub-expression, and its value, and a static value";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else-if statement");
			}
		}

		/**
		 * Tests to see if "else", works
		 */
		[ActiveEvent(Name = "magix.test.execute.else")]
		public static void magix_test_execute_else(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["_data"]["item1"]["Description123"].Value = "thomas";
			tmp["_buffer"].Value = null;
			tmp["if"].Value = "[_data][item1][Description123].Value==thomas1";
			tmp["if"]["set"].Value = "[_buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[_data][item1][Description123].Value==thomas2";
			tmp["else-if"]["set"].Value = "[_buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[_buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [else] behaves
as it should and kicks in if neither [if] nor [else-if] kicks in";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseActiveEvent(
				"magix.execute",
				tmp);

			if (tmp["_buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else statement");
			}
		}
	}
}

