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
	 * Contains Unit Tests for "magix.execute.throw", et. al. active events
	 */
	public class ExceptionTest : ActiveController
	{
		/**
		 * Tests to see if "throw", works
		 */
		[ActiveEvent(Name = "magix.test.execute.throw")]
		public void magix_test_execute_throw(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["throw"].Value = "this is our message!";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [throw] behaves correctly, 
and throws";
				e.Params.AddRange(tmp);
				return;
			}

			try
			{
				RaiseEvent(
					"magix.execute",
					tmp);

				throw new ApplicationException("Exception didn't occur!");
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				if (err.Message != "this is our message!")
					throw new ApplicationException("Wrong message in Exception");
			}
		}

		/**
		 * Tests to see if "remove", works
		 */
		[ActiveEvent(Name = "magix.test.execute.catch")]
		public void magix_test_execute_catch(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["try"].Value = null;
			tmp["try"]["code"]["throw"].Value = "exception thrown by test";
			tmp["try"]["catch"].Value = null;

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"verifies that [try], [throw] and [catch]
behave as they should, by checking for the existence of an exception
after throwing and catching";
				e.Params.AddRange(tmp);
				return;
			}

			try
			{
				RaiseEvent(
					"magix.execute",
					tmp);

				throw new ApplicationException(
					"throw didn't throw exception ...?");
			}
			catch
			{
				if (tmp["try"]["catch"]["exception"].Get<string>() != "exception thrown by test")
					throw new ApplicationException("Exception Message didn't show when exception was thrown");

				return;
			}
		}
	}
}

