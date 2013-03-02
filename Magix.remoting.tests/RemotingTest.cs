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
	 * Contains Unit Tests for "magix.execute" active event
	 */
	public class RemotingTest : ActiveController
	{
		/**
		 * Tests to see if create "event", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.remote-event-invoke-passing-data")]
		public void magix_test_remote_event_invoke_passing_data(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["remotable"].Value = true;
			tmp["event"]["code"]["set"].Value = "[/][P][Data].Value";
			tmp["event"]["code"]["set"]["value"].Value = "[/][P][Name].Value";
			tmp["remote"]["URL"].Value = "http://127.0.0.1:8080";
			tmp["remote"].Value = "foo.bar";
			tmp["remote"]["Name"].Value = "thomas";
			tmp.Add (new Node("event", "foo.bar"));

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if event
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (tmp["remote"]["Data"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing remote statement");
			}
		}

		/**
		 * Tests to see if create default "event", and invoking it later remotely, throws
		 */
		[ActiveEvent(Name = "magix.test.remoting.default-event-remoted-throws")]
		public void magix_test_default_event_remoted_throws(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["code"]["Data"].Value = "howdy";
			tmp["remote"]["URL"].Value = "http://127.0.0.1:8080";
			tmp["remote"]["event"].Value = "foo.bar";
			tmp.Add (new Node("event", "foo.bar"));

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"verifies that an event
created as a default event, throws when attempted to be invoked remotely";
				e.Params.AddRange(tmp);
				return;
			}

			try
			{
				RaiseEvent(
					"magix.execute",
					tmp);
				throw new ApplicationException("default active event invoked remotely didn't throw an exception ...?");
			}
			catch
			{
				Node tmp2 = new Node();
				tmp2["event"].Value = "foo.bar";
				RaiseEvent(
					"magix.execute",
					tmp2);
			}
		}
	}
}

