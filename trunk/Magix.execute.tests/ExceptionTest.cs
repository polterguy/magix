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
		[ActiveEvent(Name = "magix.test.throw")]
		public void magix_test_for_throw(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["throw"].Value = "This is our Message!";

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if throw
functions as it should.";
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

				if (!err.Message.Contains("This is our Message!"))
					throw new ApplicationException("Wrong message in Exception");
			}
		}

		/**
		 * Tests to see if "remove", works
		 */
		[ActiveEvent(Name = "magix.test.throw-1")]
		public void magix_test_throw_1(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["try"].Value = null;
			tmp["try"]["code"]["throw"].Value = "Exception Thrown by Test";
			tmp["try"]["catch"].Value = null;

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if try, throw and catch
functions as it should.";
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
				if (!tmp["try"]["catch"]["exception"].Get<string>().Contains("Exception Thrown by Test"))
					throw new ApplicationException("Exception Message didn't show when exception was thrown");

				return;
			}
		}
	}
}

