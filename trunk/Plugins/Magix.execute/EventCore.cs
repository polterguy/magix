/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * hyper lisp event support
	 */
	public class EventCore : ActiveController
	{
        private static Node _events = new Node();
        private static Node _inspect = new Node();

        private Dictionary<string, Node> SessionEvents
        {
            get
            {
                if (Page.Session["magix.execute.EventCore.SessionEvents"] == null)
                    Page.Session["magix.execute.EventCore.SessionEvents"] = new Dictionary<string, Node>();
                return Page.Session["magix.execute.EventCore.SessionEvents"] as Dictionary<string, Node>;
            }
        }

        /**
         * creates the associations between existing events in the database, and active event references
         */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>called during startup of application 
to make sure our active events, which are dynamically tied towards serialized hyper 
lisp blocks of code, are being correctly re-mapped</p>";
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

					_events[idx["event"].Get<string>()].ReplaceChildren(idx["code"]);
                    _inspect[idx["event"].Get<string>()].ReplaceChildren(idx["inspect"]);
				}
			}
		}

        /**
         * event hyper lisp keyword
         */
        [ActiveEvent(Name = "magix.execute.event")]
        public static void magix_execute_event(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>overrides the active event in [event]
with the hyper lisp code in the [code] expression</p><p>these types of active events 
can take and return parameters.&nbsp;&nbsp;if you wish to pass in or retrieve parameters, 
then as you invoke the function, just append your parameters underneath the function 
invocation, and they will be passed into the function, where they will be accessible 
underneath the [$] node, appended as the last parts of your code block, into your function 
invocation.&nbsp;&nbsp;from outside of the function/event itself, you can access these 
parameters directly underneath the active event itself</p><p>any existing event with 
the name from the [event] node's value will be deleted, if you pass in no [code] block</p>
<p>if you set the [remotable] node to true, then the active event will be possible to 
invoke by remote servers, and marked as open.&nbsp;&nbsp;if you set [persist] to false, 
then the active event will not be serialized into the data storage, meaning it will only 
last as long as the application is not restarted.&nbsp;&nbsp;this is useful for active 
events whom are created for instance during the startup of your application, since it 
will save time, since they will anyway be overwritten the next time your application 
restarts</p><p>thread safe</p>";
				e.Params["event"].Value = "foo.bar";
                e.Params["event"]["remotable"].Value = false;
                e.Params["event"]["persist"].Value = false;
                e.Params["event"]["code"]["_data"].Value = "thomas";
				e.Params["event"]["code"]["_backup"].Value = "thomas";
				e.Params["event"]["code"]["if"].Value = "equals";
                e.Params["event"]["code"]["if"]["lhs"].Value = "[_data].Value";
                e.Params["event"]["code"]["if"]["rhs"].Value = "[_backup].Value";
				e.Params["event"]["code"]["if"]["code"]["set"].Value = "[$][output].Value";
                e.Params["event"]["code"]["if"]["code"]["set"]["value"].Value = "return-value";
                e.Params["event"]["code"]["if"]["code"].Add(new Node("set", "[/][magix.viewport.show-message][message].Value"));
                e.Params["event"]["code"]["if"]["code"][e.Params["event"]["code"]["if"]["code"].Count - 1]["value"].Value = "[$][input].Value";
				e.Params["event"]["code"]["magix.viewport.show-message"].Value = null;
				e.Params["foo.bar"].Value = null;
				e.Params["foo.bar"]["input"].Value = "hello world 2.0";
				e.Params.Add (new Node("event", "foo.bar"));
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [event] directly, except for inspect purposes");

			Node ip = e.Params ["_ip"].Value as Node;

			string activeEvent = ip.Get<string>();

			if (string.IsNullOrEmpty(activeEvent))
				throw new ArgumentException("you cannot create an event without a name in the value of the [event] keyword");

			bool remotable = ip.Contains("remotable") && ip["remotable"].Get<bool>();

			if (ip.Contains("code"))
			{
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    // removing any previous similar events
                    Node removeNode = new Node();

                    removeNode["prototype"]["event"].Value = activeEvent;
                    removeNode["prototype"]["type"].Value = "magix.execute.event";

                    RaiseActiveEvent(
                        "magix.data.remove",
                        removeNode);

                    Node saveNode = new Node();

                    saveNode["id"].Value = Guid.NewGuid().ToString();
                    saveNode["value"]["event"].Value = activeEvent;
                    saveNode["value"]["type"].Value = "magix.execute.event";
                    saveNode["value"]["remotable"].Value = remotable;
                    saveNode["value"]["code"].ReplaceChildren(ip["code"].Clone());

                    if (ip.Contains("inspect"))
                        saveNode["value"]["inspect"].Value = ip["inspect"].Value;

                    RaiseActiveEvent(
                        "magix.data.save",
                        saveNode);
                }

				ActiveEvents.Instance.CreateEventMapping(
					activeEvent, 
					"magix.execute._active-event-2-code-callback");

				if (remotable)
					ActiveEvents.Instance.MakeRemotable(activeEvent);
				else
					ActiveEvents.Instance.RemoveRemotable(activeEvent);

				_events[activeEvent].Clear();
                _events[activeEvent].AddRange(ip["code"].Clone());

                _inspect[activeEvent].Clear();
                _inspect[activeEvent].Value = ip["inspect"].Value;
			}
			else
			{
                if (!ip.Contains("persist") || ip["persist"].Get<bool>())
                {
                    Node removeNode = new Node();

                    removeNode["prototype"]["event"].Value = activeEvent;
                    removeNode["prototype"]["type"].Value = "magix.execute.event";

                    RaiseActiveEvent(
                        "magix.data.remove",
                        removeNode);
                }

				ActiveEvents.Instance.RemoveMapping(activeEvent);
				ActiveEvents.Instance.RemoveRemotable(activeEvent);

                _events[activeEvent].UnTie();
                _inspect[activeEvent].UnTie();
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
                e.Params["inspect"].Value = @"<p>dynamically created active event, 
created with the [event] keyword.&nbsp;&nbsp;thread safety is dependent upon the 
events raised internally within event</p>";

                if (_events.Contains(e.Name))
                {
                    if (_inspect.Contains(e.Name))
                        e.Params["inspect"].Value = _inspect[e.Name].Value;
                    foreach (Node idx in _events[e.Name])
                    {
                        e.Params.Add(idx.Clone().UnTie());
                    }
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

        /**
         * session-event hyper lisp keyword
         */
        [ActiveEvent(Name = "magix.execute.session-event")]
        public void magix_execute_session_event(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>overrides the active event in [session-event]
with the hyper lisp in the [code] expression for the current session</p><p>these types of active 
events can take and return parameters.&nbsp;&nbsp;if you wish to pass in or retrieve parameters, 
then as you invoke the function, just append your args underneath the function invocation, and 
they will be passed into the function, where they will be accessible underneath a [$] node, 
appended as the last parts of your code block, into your function invocation.&nbsp;&nbsp;from
outside of the function/event itself, you can access these parameters directly underneath the 
active event itself</p><p>event will be deleted, if you pass in no [code] block</p><p>not thread 
safe</p>";
                e.Params["session-event"].Value = "foo.bar";
                e.Params["session-event"]["remotable"].Value = false;
                e.Params["session-event"]["code"]["_data"].Value = "thomas";
                e.Params["session-event"]["code"]["_backup"].Value = "thomas";
                e.Params["session-event"]["code"]["if"].Value = "equals";
                e.Params["session-event"]["code"]["if"]["lhs"].Value = "[_data].Value";
                e.Params["session-event"]["code"]["if"]["rhs"].Value = "[_backup].Value";
                e.Params["session-event"]["code"]["if"]["code"]["set"].Value = "[$][output].Value";
                e.Params["session-event"]["code"]["if"]["code"]["set"]["value"].Value = "return-value";
                e.Params["session-event"]["code"]["if"]["code"].Add(new Node("set", "[/][magix.viewport.show-message][message].Value"));
                e.Params["session-event"]["code"]["if"]["code"][e.Params["session-event"]["code"]["if"]["code"].Count - 1]["value"].Value = "[$][input].Value";
                e.Params["session-event"]["code"]["magix.viewport.show-message"].Value = null;
                e.Params["foo.bar"].Value = null;
                e.Params["foo.bar"]["input"].Value = "hello world 2.0";
                e.Params.Add(new Node("session-event", "foo.bar"));
                return;
            }

            if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
                throw new ArgumentException("you cannot raise [magix.execute.session-event] directly, except for inspect purposes");

            Node ip = e.Params["_ip"].Value as Node;

            string activeEvent = ip.Get<string>();

            if (string.IsNullOrEmpty(activeEvent))
                throw new ArgumentException("you cannot create an event without a name in the value of the [session-event] keyword");

            if (ip.Contains("code"))
            {
                Node n = new Node();
                n.ReplaceChildren(ip["code"].Clone());
                SessionEvents[activeEvent] = n;
            }
            else
            {
                SessionEvents.Remove(activeEvent);
            }
        }

        /**
         * entry point for hyper lisp created active session-event overrides
         */
        [ActiveEvent(Name = "")]
        public void magix_data__active_event_2_code_callback_session_events(object sender, ActiveEventArgs e)
        {
            if (SessionEvents.ContainsKey(e.Name))
            {
                Node code = SessionEvents[e.Name];
                code["$"].AddRange(e.Params);

                RaiseActiveEvent(
                    "magix.execute",
                    code);

                e.Params.ReplaceChildren(code["$"]);
            }
        }

        /*
         * get active events active event
         */
        [ActiveEvent(Name = "magix.execute.list-events")]
        public static void magix_execute_list_events(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns all active events 
within the system.&nbsp;&nbsp;add [all], [open], [remoted], [overridden] or 
[begins-with] to filter the events returned</p>active events are returned 
in [events].&nbsp;&nbsp;will not return unit tests and active events starting 
with _ if [all] is false.&nbsp;&nbsp;if [all], [open], [remoted] or [overridden] 
is defined, it will return all events fullfilling criteria, regardless of 
whether or not they are private events, tests or don't match the [begins-with] 
parameter</p><p>thread safe</p>";
                e.Params["list-events"]["all"].Value = true;
                e.Params["list-events"]["open"].Value = false;
                e.Params["list-events"]["remoted"].Value = false;
                e.Params["list-events"]["overridden"].Value = false;
                e.Params["list-events"]["begins-with"].Value = "magix.execute.";
                return;
            }

            Node ip = Ip(e.Params);

            bool open = false;
            if (ip.Contains("open"))
                open = ip["open"].Get<bool>();

            bool all = true;
            if (ip.Contains("all"))
                all = ip["all"].Get<bool>();

            bool remoted = false;
            if (ip.Contains("remoted"))
                remoted = ip["remoted"].Get<bool>();

            bool overridden = false;
            if (ip.Contains("overridden"))
                overridden = ip["overridden"].Get<bool>();

            string beginsWith = null;
            if (ip.Contains("begins-with"))
                beginsWith = ip["begins-with"].Get<string>();

            foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
            {
                if (open && !ActiveEvents.Instance.IsAllowedRemotely(idx))
                    continue;
                if (remoted && string.IsNullOrEmpty(ActiveEvents.Instance.RemotelyOverriddenURL(idx)))
                    continue;
                if (overridden && !ActiveEvents.Instance.IsOverride(idx))
                    continue;

                if (!all && !string.IsNullOrEmpty(beginsWith) && idx.Replace(beginsWith, "").Contains("_"))
                    continue;

                if (!all && idx.StartsWith("magix.test."))
                    continue;

                if (!string.IsNullOrEmpty(beginsWith) && !idx.StartsWith(beginsWith))
                    continue;

                ip["events"][idx].Value = null;
            }

            ip["events"].Sort(
                delegate(Node left, Node right)
                {
                    return left.Name.CompareTo(right.Name);
                });
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
					Node evtNode = new Node();

					evtNode["prototype"]["type"].Value = "magix.execute.event";
					evtNode["prototype"]["event"].Value = name;

					RaiseActiveEvent(
						"magix.data.load",
						evtNode);

					if (evtNode.Contains("objects"))
					{
                        _events[name].AddRange(evtNode["objects"][0]["code"].UnTie());
                        _inspect[name].AddRange(evtNode["objects"][0]["inspect"].UnTie());
                        return _events[name].Clone();
					}
					_events[name].Value = null;
					return null;
				}
			}
		}
	}
}

