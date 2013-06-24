/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Controller logic for handling magix.execute overrides, where you've
	 * overridden an Active Event with magix.execute code
	 */
	public class EventCore : ActiveController
	{
		private static bool? _hasNull;

		public EventCore()
		{
			lock (typeof(EventCore))
			{
				_hasNull = new bool?();
			}
		}

		/**
		 * Handled to make sure we map our overridden magix.execute events during
		 * app startup
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.core.application-startup"].Value = null;
				e.Params["inspect"].Value = @"called during startup
of application to make sure our active events, which are dynamically tied towards serialized 
magix.execute blocks of code, are being correctly re-mapped";
				return;
			}

			Node tmp = new Node();
			tmp["prototype"]["type"].Value = "magix.execute.event";

			RaiseActiveEvent(
				"magix.data.load",
				tmp);

			if (tmp.Contains("objects"))
			{
				foreach (Node idx in tmp["objects"])
				{
					ActiveEvents.Instance.CreateEventMapping(
						idx["event"].Get<string>(), 
						"magix.execute._active-event-2-code-callback");

					if (idx.Contains("remotable") && idx["remotable"].Get<bool>())
						ActiveEvents.Instance.MakeRemotable(idx["event"].Get<string>());
				}
			}
		}

		/**
		 * Creates a new magix.execute Activ Event, which should contain magix.execute keywords,
		 * which will be raised when your "event" active event is raised. Submit the code
		 * in the "code" node
		 */
		[ActiveEvent(Name = "magix.execute.event")]
		public static void magix_execute_event(object sender, ActiveEventArgs e)
		{
			_hasNull = null;
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"overrides the active event in [event]
with the hyper lisp in the [code] expression.&nbsp;&nbsp;these types
of functions can take and return parameters, if you wish
to pass in or retrieve parameters, then as you invoke the 
function, just append your args underneath the function invocation,
and they will be passed into the function, where they will
be accessible underneath a [$] node, appended as the last
parts of your code block, into your function invocation.&nbsp;&nbsp;from
outside of the function/event itself, you can access these 
parameters directly underneath the active event itself.&nbsp;&nbsp;
event will be deleted, if you pass in no [code] block.&nbsp;&nbsp;thread safe";
				e.Params["event"].Value = "foo.bar";
				e.Params["inspect"].Value = "description of your event";
				e.Params["event"]["remotable"].Value = false;
				e.Params["event"]["code"]["_data"].Value = "thomas";
				e.Params["event"]["code"]["_backup"].Value = "thomas";
				e.Params["event"]["code"]["if"].Value = "[_data].Value==[_backup].Value";
				e.Params["event"]["code"]["if"]["set"].Value = "[/][P][output].Value";
				e.Params["event"]["code"]["if"]["set"]["value"].Value = "return-value";
				e.Params["event"]["code"]["if"].Add (new Node("set", "[.ip][/][magix.viewport.show-message][message].Value"));
				e.Params["event"]["code"]["if"][e.Params["event"]["code"]["if"].Count - 1]["value"].Value = "[/][$][input].Value";
				e.Params["event"]["code"]["magix.viewport.show-message"].Value = null;
				e.Params["foo.bar"].Value = null;
				e.Params["foo.bar"]["input"].Value = "hello world 2.0";
				e.Params.Add (new Node("event", "foo.bar"));
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			string activeEvent = ip.Get<string>("");
			bool remotable = ip.Contains("remotable") && ip["remotable"].Get<bool>();

			if (ip.Contains("code"))
			{
				// removing any previous siimilar events
				Node n = new Node();

				n["prototype"]["event"].Value = activeEvent;
				n["prototype"]["type"].Value = "magix.execute.event";

				RaiseActiveEvent(
					"magix.data.remove",
					n);

				n = new Node();

				n["id"].Value = Guid.NewGuid().ToString();
				n["object"]["event"].Value = activeEvent;
				n["object"]["type"].Value = "magix.execute.event";
				n["object"]["remotable"].Value = remotable;
				n["object"]["code"].ReplaceChildren(ip["code"].Clone());

				if (ip.Contains("inspect"))
					n["object"]["inspect"].Value = ip["inspect"].Value;

				RaiseActiveEvent(
					"magix.data.save",
					n);

				ActiveEvents.Instance.CreateEventMapping(
					activeEvent, 
					"magix.execute._active-event-2-code-callback");

				if (remotable)
					ActiveEvents.Instance.MakeRemotable(activeEvent);
			}
			else
			{
				Node n = new Node();

				n["prototype"]["event"].Value = activeEvent;
				n["prototype"]["type"].Value = "magix.execute.event";

				RaiseActiveEvent(
					"magix.data.remove",
					n);

				ActiveEvents.Instance.RemoveMapping(activeEvent);
				ActiveEvents.Instance.RemoveRemotable(activeEvent);
			}
		}

		/**
		 * Null event handler for handling null active event overrides for the event keyword
		 * in magix.execute
		 */
		[ActiveEvent(Name = "")]
		public static void magix_execute_event_null_helper(object sender, ActiveEventArgs e)
		{
			if (string.IsNullOrEmpty(e.Name) && ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["inspect"].Value = @"null event handler for raising
null active event handlers created with magix.execute.event";
				return;
			}

			// Small optimization, to not traverse Data storage file for EVERY SINGLE ACTIVE EVENT ...!
			if (_hasNull.HasValue && !_hasNull.Value)
				return;

			Node caller = null;
			_hasNull = false;

			Node n = new Node();

			n["prototype"]["type"].Value = "magix.execute.event";
			n["prototype"]["event"].Value = "";

			RaiseActiveEvent(
				"magix.data.load",
				n);

			if (n.Contains("objects"))
			{
				caller = n["objects"][0];
			}

			if (caller != null)
			{
				Node tmp = new Node(e.Name);
				tmp.AddRange(caller.UnTie());
				tmp["_method"].Value = e.Name;
				tmp["_method"].AddRange(e.Params.Clone());

				RaiseActiveEvent(
					"magix.execute", 
					tmp, 
					true);
			}
		}

		/**
		 * Handled to make sure we map our serialized active event overrides, the ones
		 * overridden with the event keyword
		 */
		[ActiveEvent(Name = "magix.execute._active-event-2-code-callback")]
		public static void magix_data__active_event_2_code_callback(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"dynamically created active event, created with the [magix.execute.event] keyword.&nbsp;&nbsp;
thread safety is dependent upon the events raised internally within event";

				Node le = new Node();

				le["prototype"]["type"].Value = "magix.execute.event";
				le["prototype"]["event"].Value = e.Name;
				
				RaiseActiveEvent(
					"magix.data.load",
					le);

				if (le.Contains("objects"))
				{
					if (le["objects"][0].Contains("code"))
						e.Params.AddRange(le["objects"][0]["code"].Clone());
					if (le["objects"][0].Contains("inspect"))
						e.Params["inspect"].Value = le["objects"][0]["inspect"].Value;
				}

				return;
			}

			Node n = new Node();

			n["prototype"]["type"].Value = "magix.execute.event";
			n["prototype"]["event"].Value = e.Name;

			RaiseActiveEvent(
				"magix.data.load",
				n);

			if (n.Contains("objects"))
			{
				Node caller = n["objects"][0]["code"].Clone();
				caller["$"].AddRange(e.Params);

				RaiseActiveEvent(
					"magix.execute", 
					caller);

				e.Params.ReplaceChildren(caller["$"]);
			}
		}
	}
}

