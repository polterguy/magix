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
	public class EventsTest : ActiveController
	{
		/**
		 * Tests to see if create "event", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.event-invoke")]
		public void magix_test_event_invoke(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["code"]["Data"].Value = "howdy";
			tmp["event"]["code"]["set"].Value = "[/][P][Data].Value";
			tmp["event"]["code"]["set"]["value"].Value = "thomas";
			tmp["foo.bar"].Value = null;
			tmp.Add (new Node("event", "foo.bar"));

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if ""event""
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			if (!tmp["foo.bar"].Contains("Data") || 
			    tmp["foo.bar"]["Data"].Get<string>() != "thomas")
			{
				throw new ApplicationException(
					"Failure of executing event invoke statement");
			}
		}

		/**
		 * Tests to see if "remove-event", works
		 */
		[ActiveEvent(Name = "magix.test.remove-event")]
		public void magix_test_remove_event(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["code"]["Data"].Value = "howdy";
			tmp["event"]["code"]["set"].Value = "[Data].Value";
			tmp.Add (new Node("event", "foo.bar"));

			if (e.Params.Contains("inspect"))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"Checks to see if remove-event
functions as it should.";
				e.Params.AddRange(tmp);
				return;
			}

			RaiseEvent(
				"magix.execute",
				tmp);

			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (idx == "foo.bar")
				{
					throw new ApplicationException(
						"Failure of executing remove-event statement");
				}
			}
		}

		/**
		 * Tests to see if create "event", and invoking it later, works
		 */
		[ActiveEvent(Name = "magix.test.remote-event-invoke")]
		public void magix_test_remote_event_invoke(object sender, ActiveEventArgs e)
		{
			Node tmp = new Node();

			tmp["event"].Value = "foo.bar";
			tmp["event"]["remotable"].Value = true;
			tmp["event"]["code"]["set"].Value = "[/][P][Data].Value";
			tmp["event"]["code"]["set"]["value"].Value = "thomas";
			tmp["remote"]["URL"].Value = "http://127.0.0.1:8080";
			tmp["remote"].Value = "foo.bar";
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
	}
}

