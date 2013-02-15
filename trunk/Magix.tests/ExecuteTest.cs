/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.tests
{
	/**
	 * Contains Unit Tests for "magix.execute" active event
	 */
	public class ExecuteTest : ActiveController
	{
		/**
		 * Tests to see if "set", works with Node-Lists
		 */
		[ActiveEvent(Name = "magix.test.set-node-list")]
		public void magix_test_set_node_list (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer][Copy]";
			tmp["set"]["value"].Value = "[Data]";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if copying of 
lists of Nodes is functioning as it should, 
by copying the contents from ""Data"" to
""Buffer"". Then checks to see if the ""Buffer""
Node contains a copy of the original Node-set.";
				e.Params.AddRange (tmp);
				return;
			}

			Node original = tmp["Data"].Clone ();

			RaiseEvent (
				"magix.execute",
				tmp);

			if (!tmp["Buffer"]["Copy"].HasNodes (original))
			{
				throw new ApplicationException("The 'Buffer' Node didn't equal the original Node-list in the 'Data' Node as were expected");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.set-node-list executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "set", works with Values
		 */
		[ActiveEvent(Name = "magix.test.set-value")]
		public void magix_test_set_value (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data][Item1][Description].Value";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if copying of 
Node values is functioning as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
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

			Node xM = new Node();
			xM["message"].Value = "magix.test.set-value executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "set", works with Name
		 */
		[ActiveEvent(Name = "magix.test.get-name")]
		public void magix_test_get_name (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = null;
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data][Item1][0].Name";

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

			if (tmp["Buffer"].Get<string>() != "Description123")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"Description123",
						tmp["Buffer"].Get<string>()));
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.set-name executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "set", works with Name
		 */
		[ActiveEvent(Name = "magix.test.set-name")]
		public void magix_test_set_name (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = null;
			tmp["Buffer"]["DataTmp"].Value = null;
			tmp["set"].Value = "[Buffer][DataTmp].Name";
			tmp["set"]["value"].Value = "[Data][Item1][0].Name";

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

			if (tmp["Buffer"][0].Name != "Description123")
			{
				throw new ApplicationException(
					string.Format (
						"Expected {0}, got {1}",
						"Description123",
						tmp["Buffer"][0].Name));
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.set-name executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "if", "set" and "magix.execute" works
		 */
		[ActiveEvent(Name = "magix.test.execute-1")]
		public void magix_test_execute_1 (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node("execute");
			tmp["Data"].Value = "not-set";
			tmp["if"].Value = "[Data].Value==not-set";
			tmp["if"]["set"].Value = "[Data].Value";
			tmp["if"]["set"]["value"].Value = "new-value";

			if (e.Params.Contains ("inspect"))
			{
				e.Params["inspect"].Value = @"Tests to see if basic magix.execute
functionality works, specifically ""set"" on a 
Data node in the Node tree. Throws an exception 
unless the ""set"" operation executed successfully.";
				e.Params.Add (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			// Asserting new value is set ...
			if (tmp["Data"].Get<string>() != "new-value")
				throw new ApplicationException(
					string.Format(
						"Set didn't update as supposed to, expected {0}, got {1}",
						"new-value",
					tmp["Data"].Get<string>()));

			Node xM = new Node();
			xM["message"].Value = "magix.test.execute-1 executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if ""set"" ing a Value to a Node-list throws an exception
		 */
		[ActiveEvent(Name = "magix.test.assure-set-lists-dont-become-values")]
		public void magix_test_assure_set_lists_dont_become_values (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"].Value = null;
			tmp["set"].Value = "[Buffer].Value";
			tmp["set"]["value"].Value = "[Data]";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Value
functions as it should, by demanding an exception to
declare as success.";
				e.Params.AddRange (tmp);
				return;
			}

			bool hasException = false;
			try
			{
				RaiseEvent (
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
					"Expected exception due to assigning a Node-list to a Value, but didn't get one ...?");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.assure-set-lists-dont-become-values successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if ""set"" ing a Name to a Node-list throws an exception
		 */
		[ActiveEvent(Name = "magix.test.assure-set-lists-dont-become-names")]
		public void magix_test_assure_set_lists_dont_become_names (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description"].Value = "desc1";
			tmp["Data"]["Item2"]["Description"].Value = "desc2";
			tmp["Data"]["Item3"]["Description"].Value = "desc3";
			tmp["Data"]["Item4"]["Description"].Value = "desc4";
			tmp["Buffer"]["Item"].Value = null;
			tmp["set"].Value = "[Buffer][Item].Name";
			tmp["set"]["value"].Value = "[Data][Item1]";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if setting Node Name
functions as it should, by demanding an exception to
declare as success.";
				e.Params.AddRange (tmp);
				return;
			}

			bool hasException = false;
			try
			{
				RaiseEvent (
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

			Node xM = new Node();
			xM["message"].Value = "magix.test.assure-set-lists-dont-become-names successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

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

			Node xM = new Node();
			xM["message"].Value = "magix.test.if executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "else-if", works
		 */
		[ActiveEvent(Name = "magix.test.else-if")]
		public void magix_test_else_if (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"]["Description123"].Value = "thomas";
			tmp["Buffer"].Value = null;
			tmp["if"].Value = "[Data][Item1][Description123].Value==thomas1";
			tmp["if"]["set"].Value = "[Buffer].Value";
			tmp["if"]["set"]["value"].Value = "failure";
			tmp["else-if"].Value = "[Data][Item1][Description123].Value==thomas";
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

			Node xM = new Node();
			xM["message"].Value = "magix.test.else-if executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
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

			Node xM = new Node();
			xM["message"].Value = "magix.test.else executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "for-each", works
		 */
		[ActiveEvent(Name = "magix.test.for-each")]
		public void magix_test_for_each (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Data"]["Item1"].Value = "thomas1";
			tmp["Data"]["Item2"].Value = "thomas2";
			tmp["Data"]["Item3"].Value = "thomas3";
			tmp["Buffer"].Value = null;
			tmp["for-each"].Value = "[Data]";
			tmp["for-each"]["set"].Value = "[/][Buffer][[.].Name].Value";
			tmp["for-each"]["set"]["value"].Value = "[.].Value";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if for-each
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"]["Item1"].Get<string>() != "thomas1" || 
			    tmp["Buffer"]["Item2"].Get<string>() != "thomas2" ||
			    tmp["Buffer"]["Item3"].Get<string>() != "thomas3")
			{
				throw new ApplicationException(
					"Failure of executing for-each statement");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.for-each executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "throw", works
		 */
		[ActiveEvent(Name = "magix.test.throw")]
		public void magix_test_for_throw (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["throw"].Value = "This is our Message!";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if throw
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			try
			{
				RaiseEvent (
					"magix.execute",
					tmp);
				throw new ApplicationException("Exception didn't occur!");
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;
				if (!err.Message.Contains ("This is our Message!"))
					throw new ApplicationException("Wrong message in Exception");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.throw executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "remove", works
		 */
		[ActiveEvent(Name = "magix.test.remove")]
		public void magix_test_remove (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["Buffer"]["Data"]["Item1"].Value = "This is our Value1!";
			tmp["Buffer"]["Data"]["Item2"].Value = "This is our Value2!";
			tmp["Buffer"]["Data"]["Item3"].Value = "This is our Value3!";
			tmp["Buffer"]["Data"]["Item4"].Value = "This is our Value4!";
			tmp["Buffer"]["Data"]["Item5"].Value = "This is our Value5!";
			tmp["remove"].Value = "[Buffer][Data]";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if throw
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["Buffer"].Count != 0)
			{
				throw new ApplicationException(
					"Failure of executing remove statement");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.remove executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if create "function", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.function-invoke")]
		public void magix_test_function_invoke (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["function"]["event"].Value = "foo.bar";
			tmp["function"]["code"]["Data"].Value = "howdy";
			tmp["function"]["code"]["set"].Value = "[.ip][..][Data].Value";
			tmp["foo.bar"].Value = null;

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if function
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["foo.bar"].Value != null)
			{
				throw new ApplicationException(
					"Failure of executing remove statement");
			}

			bool found = false;
			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (idx == "foo.bar")
					found = true;
			}
			if (!found)
				throw new ApplicationException("Couldn't find foo.bar after creating it ...");

			Node xM = new Node();
			xM["message"].Value = "magix.test.function-invoke executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "remove-function", works
		 */
		[ActiveEvent(Name = "magix.test.remove-function")]
		public void magix_test_remove_function (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["function"]["event"].Value = "foo.bar";
			tmp["function"]["code"]["Data"].Value = "howdy";
			tmp["function"]["code"]["set"].Value = "[.ip][..][Data].Value";
			tmp["remove-function"].Value = "foo.bar";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if remove-function
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (idx == "foo.bar")
				{
					throw new ApplicationException(
						"Failure of executing remove-function statement");
				}
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.remove-function executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if create "function", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.remote-function-invoke")]
		public void magix_test_remote_function_invoke (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["function"]["event"].Value = "foo.bar";
			tmp["function"]["remotable"].Value = true;
			tmp["function"]["code"]["Data"].Value = "howdy";
			tmp["remote"]["URL"].Value = "http://127.0.0.1:8080";
			tmp["remote"]["event"].Value = "foo.bar";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if function
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["remote"]["params"]["Data"].Get<string>() != "howdy")
			{
				throw new ApplicationException(
					"Failure of executing remote statement");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.remote-function-invoke executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "fork", works
		 */
		[ActiveEvent(Name = "magix.test.fork")]
		public void magix_test_fork (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"]["key"].Value = "fork-test-buffer-object";
			tmp["fork"]["Buffer"].Value = null;
			tmp["fork"]["magix.data.save"]["key"].Value = "fork-test-buffer-object";
			tmp["fork"]["magix.data.save"]["object"]["Value"].Value = "thomas";
			tmp["sleep"].Value = 500;
			tmp["magix.data.load"]["key"].Value = "fork-test-buffer-object";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if creating a new thread
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["magix.data.load"]["object"]["Value"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing fork statement");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.fork executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}

		/**
		 * Tests to see if "magix.data.save/load", works
		 */
		[ActiveEvent(Name = "magix.test.data-save-load-by-key")]
		public void magix_test_data_save_load_by_key (object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["magix.data.remove"]["key"].Value = "data-save-test";
			tmp["execute"]["magix.data.load"]["key"].Value = "data-save-test";
			tmp["execute"]["if"].Value = "[execute][magix.data.load][object]";
			tmp["execute"]["if"]["throw"].Value = "Object didn't delete";
			tmp["Buffer"].Value = null;
			tmp["magix.data.save"]["key"].Value = "data-save-test";
			tmp["magix.data.save"]["object"]["Value"].Value = "thomas";
			tmp["magix.data.load"]["key"].Value = "data-save-test";

			if (e.Params.Contains ("inspect"))
			{
				e.Params.Clear ();
				e.Params["inspect"].Value = @"Checks to see if creating a new thread
functions as it should.";
				e.Params.AddRange (tmp);
				return;
			}

			RaiseEvent (
				"magix.execute",
				tmp);

			if (tmp["magix.data.load"]["object"]["Value"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing fork statement");
			}

			Node xM = new Node();
			xM["message"].Value = "magix.test.fork executed successfully";
			RaiseEvent("magix.viewport.show-message", xM);
		}
	}
}

