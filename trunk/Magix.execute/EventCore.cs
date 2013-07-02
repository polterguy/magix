/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * hyper lisp event support
	 */
	public class EventCore : ActiveController
	{
		private static Node _events = new Node();

		/*
		 * creates the associations between existing events in the database, and active event references
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

					_events[idx["event"].Get<string>()].ReplaceChildren(idx["code"].Clone());
				}
			}
		}

		/*
		 * event hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.event")]
		public static void magix_execute_event(object sender, ActiveEventArgs e)
		{
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

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.add] directly, except for inspect purposes");

			Node ip = e.Params ["_ip"].Value as Node;

			string activeEvent = ip.Get<string>();

			if (string.IsNullOrEmpty(activeEvent))
				throw new ArgumentException("you cannot create an event without a name in the value of the [event] keyword");

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
				else
					ActiveEvents.Instance.RemoveRemotable(activeEvent);

				_events[activeEvent].Clear();
				_events[activeEvent].AddRange(ip["code"].Clone());
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

				_events[activeEvent].UnTie();
			}
		}

		private static Node GetEventCode(string name)
		{
			if (_events.Contains(name) && _events[name].Count > 0)
				return _events[name].Clone();
			else if (_events.Contains(name))
				return null;

			lock (typeof(EventCore))
			{
				if (_events.Contains(name) && _events[name].Count > 0)
					return _events[name].Clone();
				else if (_events.Contains(name))
					return null;
				else
				{
					Node n = new Node();

					n["prototype"]["type"].Value = "magix.execute.event";
					n["prototype"]["event"].Value = name;

					RaiseActiveEvent(
						"magix.data.load",
						n);

					if (n.Contains("objects"))
					{
						Node caller = n["objects"][0]["code"];
						_events[name].AddRange(caller);
						return _events[name].Clone();
					}
					_events[name].Value = null;
					return null;
				}
			}
		}

		/**
		 * entry point for hyper lisp created active event overrides
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

			Node code = GetEventCode(e.Name);

			if (code != null)
			{
				code["$"].AddRange(e.Params);

				RaiseActiveEvent(
					"magix.execute", 
					code);

				e.Params.ReplaceChildren(code["$"]);
			}
		}
	}
}

