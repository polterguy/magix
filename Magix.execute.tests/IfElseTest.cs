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
		[ActiveEvent(Name = "magix.test.if")]
		public void magix_test_if (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Description123].Value==thomas";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-name")]
		public void magix_test_if_name (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][0].Name==Description123";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-not-equals")]
		public void magix_test_if_not_equals (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1].Value!=thomas";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-single-value")]
		public void magix_test_if_single_value (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1].Value";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-single-node")]
		public void magix_test_if_single_node (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1]";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-single-node-no-exists")]
		public void magix_test_if_single_node_not_exists (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Item2]";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-single-value-null")]
		public void magix_test_if_single_value_null (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1].Value";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-single-value-not-exists")]
		public void magix_test_if_single_value_not_exists (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Item2].Value";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-nested-if")]
		public void magix_test_if_nested_if (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = "thomas";
			tmp["Buffer"].Value = "failure";
			tmp["if"].Value = "[Data]";
			tmp["if"]["if"].Value = "[Data][Item1]";
			tmp["if"]["if"]["if"].Value = "![Data].Value";
			tmp["if"]["if"]["if"]["if"].Value = "[Data].Name==Data";
			tmp["if"]["if"]["if"]["if"]["if"].Value = "[Data][Item1].Value==thomas";
			tmp["if"]["if"]["if"]["if"]["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["if"]["if"]["if"]["if"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-single-value-not-null")]
		public void magix_test_if_single_value_not_null (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "![Data][Item1].Value";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-not-generating-path")]
		public void magix_test_if_not_generating_path (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"].Value = null;
			tmp["if"].Value = "[Data][Item].Name";
			tmp["if"]["set"].Value = "[Data].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Data].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Data"].Count > 0 || 
			    tmp["Data"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "if", works
		 */
		[ActiveEvent(Name = "magix.test.if-not-generating-path-2")]
		public void magix_test_if_not_generating_path_2 (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"].Value = null;
			tmp["if"].Value = "[Data][0].Name";
			tmp["if"]["set"].Value = "[Data].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Data].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Data"].Count > 0 || 
			    tmp["Data"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing if statement");
			}
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.else-if-single-statement")]
		public void magix_test_else_if_single_statement (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Description123].Value==thomas1";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[Data][Item1][Description123].Value";
			tmp["else-if"]["set"].Value = "[Buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else-if statement");
			}
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.else-if-node-exists")]
		public void magix_test_else_if_node_exists (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Description123].Value==thomas1";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[Data][Item1][Description123]";
			tmp["else-if"]["set"].Value = "[Buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else-if statement");
			}
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.else-if-sub-expression")]
		public void magix_test_else_if_sub_expression (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Description123].Value==thomas1";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[Data][Item1][[0][0][0].Name].Value==thomas";
			tmp["else-if"]["set"].Value = "[Buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "success";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "failure";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else-if statement");
			}
		}

		/**
		 * Tests to see if "else", works
		 */
		[ActiveEvent(Name = "magix.test.else")]
		public void magix_test_else (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Description123].Value==thomas1";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[Data][Item1][Description123].Value==thomas2";
			tmp["else-if"]["set"].Value = "[Buffer].Value";
			tmp["else-if"]["set"]["value"].Value = "failure";
			tmp["else"]["set"].Value = "[Buffer].Value";
			tmp["else"]["set"]["value"].Value = "success";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Get<string>() != "success")
			{
				throw new ApplicationException(
					"Failure of executing else statement");
			}
		}
	}
}

