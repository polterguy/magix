/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using Magix.Core;

namespace Magix.admin
{
	/**
	 * Controller logic for the Active Event Executor. Helps load the executor, and 
	 * other support functions
	 */
	public class EventExecutor : ActiveController
	{
		/**
		 * Returns the Active Events registered in the system. This includes
		 * also the Active events which are temporarily loaded, due to Active
		 * Modules and similar which are registering events. Returns
		 * all Active Events as a list of Node in the "ActiveEvent" child node
		 */
		[ActiveEvent(Name = "magix.admin.get-active-events")]
		public static void magix_admin__get_active_events(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.get-active-events"].Value = null;
				e.Params["all"].Value = false;
				e.Params["open"].Value = false;
				e.Params["remoted"].Value = false;
				e.Params["overridden"].Value = false;
				e.Params["begins-with"].Value = "magix.execute.";
				e.Params["inspect"].Value = @"returns all active events 
within the system.&nbsp;&nbsp;add [all], [open], [remoted], [overridden] 
or [begins-with] to filter the events returned.&nbsp;&nbsp;active events 
are returned in [events].&nbsp;&nbsp;by default, unit tests and active events starting with _
as their name, will not be returned, unless [all] is true.&nbsp;&nbsp;
if [all], [open], [remoted] or [overridden] is defined, it will return all events 
fullfilling criteria, regardless of whether or not they are private events, tests or
don't match the [begins-with] parameter.&nbsp;&nbsp;thread safe";
				return;
			}

			bool takeAll = false;
			if (e.Params.Contains("all"))
				takeAll = e.Params["all"].Get<bool>();

			bool open = false;
			if (e.Params.Contains("open"))
				open = e.Params["open"].Get<bool>();

			bool remoted = false;
			if (e.Params.Contains("remoted"))
				remoted = e.Params["remoted"].Get<bool>();

			bool overridden = false;
			if (e.Params.Contains("overridden"))
				overridden = e.Params["overridden"].Get<bool>();

			string beginsWith = null;
			if (e.Params.Contains("begins-with"))
				beginsWith = e.Params["begins-with"].Get<string>();

			Node node = e.Params;
			int idxNo = 0;
			foreach (string idx in ActiveEvents.Instance.ActiveEventHandlers)
			{
				if (open)
				{
					if (ActiveEvents.Instance.IsAllowedRemotely(idx))
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					continue;
				}
				if (remoted)
				{
					if (ActiveEvents.Instance.RemotelyOverriddenURL(idx) != null)
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					continue;
				}
				if (overridden)
				{
					if (ActiveEvents.Instance.IsOverride(idx))
						node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
					continue;
				}
				if (!takeAll && string.IsNullOrEmpty(beginsWith) && idx.StartsWith("magix.test."))
					continue;

				if (idx.Contains("."))
				{
					string[] splits = idx.Split ('.');
					if (!takeAll && splits[splits.Length - 1].StartsWith("_"))
						continue; // "Hidden" event ...
				}

				if (!string.IsNullOrEmpty(beginsWith) && !idx.StartsWith(beginsWith))
					continue;

				node["events"]["no_" + idxNo.ToString()].Value = string.IsNullOrEmpty (idx) ? "" : idx;
				idxNo += 1;
			}

			node["events"].Sort (
				delegate(Node left, Node right)
				{
					return ((string)left.Value).CompareTo(right.Value as String);
				});
		}

		/**
		 * Loads the Active Event viewer in the given "container" Viewport Container.
		 * The Active Event viewer allows you to see all events in the system, and also
		 * execute arbitrary events with nodes as arguments
		 */
		[ActiveEvent(Name = "magix.admin.open-event-executor")]
		public void magix_admin_open_event_executor(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.open-even-executor"].Value = null;
				e.Params["container"].Value = "content1";
				e.Params["inspect"].Value = @"opens the active event executor module with [css], 
defaulting to span-24, in [container] viewport container, 
defaulting to content1.&nbsp;&nbsp;not thread safe";
				return;
			}

			Node tmp = new Node();

			tmp["css"].Value = e.Params["css"].Get<string>("span-24");

			LoadModule(
				"Magix.admin.ExecutorForm", 
				e.Params["container"].Get<string>("content1"),
				tmp);

			RaiseActiveEvent(
				"magix.execute._event-overridden");
		}

		/**
		 * Opens up the Event Sniffer, which allows you to spy
		 * on all events internally raised within the system
		 */
		[ActiveEvent(Name = "magix.admin.open-event-sniffer")]
		public void magix_admin_open_event_sniffer(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.open-event-sniffer"].Value = null;
				e.Params["inspect"].Value = @"will open active event sniffer, 
allowing you to spy on all active events being raised in your system.&nbsp;&nbsp;
[container] instructs magix which viewport container to load the module in.&nbsp;&nbsp;
default container is trace.&nbsp;&nbsp;
[css] instructs magix about which css classes to load the module with, 
default is span-24.&nbsp;&nbsp;not thread safe";
				e.Params["container"].Value = "trace";
				e.Params["css"].Value = "span-24";
				return;
			}

			Node tmp = new Node();

			tmp["css"].Value = e.Params.Contains("css") ? e.Params["css"].Get<string>() : "span-24";

			LoadModule(
				"Magix.admin.EventSniffer", 
				e.Params["container"].Get<string>("trace"),
				tmp);
		}

		/**
		 * Loads code into active event executor, and loads executor
		 */
		[ActiveEvent(Name = "magix.admin.load-executor-code")]
		public void magix_admin_load_executor_code(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.load-executor-code"].Value = null;
				e.Params["inspect"].Value = @"loads active event executor module
in [container] viewpoert, defaulting to content2, with given [code] Value.&nbsp;&nbsp;[code] is expected to be 
textually based hyper lisp node syntax.&nbsp;&nbsp;not thread safe";
				e.Params["code"].Value = @"
event:magix.execute
_data=>thomas
if=>[_data].Value==thomas
  magix.viewport.show-message
    message=>howdy world";
				return;
			}

			if (!e.Params.Contains("code"))
				throw new ArgumentException("cannot raise load-executor-code without [code] being hyper lisp");

			Node node = new Node();

			node["container"].Value = e.Params["container"].Get<string>("content2");

			RaiseActiveEvent(
				"magix.admin.open-event-executor",
				node);

			node = new Node();
			node["code"].Value = e.Params["code"].Get<string>();

			RaiseActiveEvent(
				"magix.admin.set-code",
				node);
		}

		/**
		 * executes the given hyper lisp file
		 */
		[ActiveEvent(Name = "magix.admin.run-file")]
		public static void magix_admin_run_file(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.run-file"].Value = null;
				e.Params["inspect"].Value = @"runs the hyper lisp [file] given.&nbsp;&nbsp;
thread safe";
				e.Params["file"].Value = "core-scripts/some-script.hl";
				return;
			}

			if (!e.Params.Contains ("file") || e.Params["file"].Get<string>("") == "")
				throw new ArgumentException("Need file object");

			string file = e.Params["file"].Get<string>();

			string txt = "";
			using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(file)))
			{
				txt = reader.ReadToEnd();
			}

			string wholeTxt = txt.TrimStart();
			string method = "magix.execute";
			if (wholeTxt.StartsWith("event:"))
			{
				method = wholeTxt.Split (':')[1];
				method = method.Substring(0, method.Contains("\n") ? method.IndexOf("\n") : method.Length);
				wholeTxt = wholeTxt.Contains("\n") ? 
					wholeTxt.Substring(wholeTxt.IndexOf("\n")).TrimStart() : 
						"";
			}

			Node tmp = new Node();
			tmp["code"].Value = wholeTxt;

			RaiseActiveEvent(
				"magix.code.code-2-node",
				tmp);

			tmp = tmp["json"].Get<Node> ();

			foreach (Node idx in e.Params)
			{
				if (idx.Name == "file")
					continue;
				tmp["$"].Add(idx.Clone());
			}

			RaiseActiveEvent(
				method, 
				tmp);
		}

		/**
		 * executes script
		 */
		[ActiveEvent(Name = "magix.admin.run-script")]
		public static void magix_admin_run_script(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.run-script"].Value = null;
				e.Params["inspect"].Value = @"runs the [script] given, and tries to set the 
active event executor to the hyper lisp code given in [script].&nbsp;&nbsp;thread safe";
				e.Params["script"].Value =  @"
event:magix.execute
_data=>thomas
if=>[_data].Value==thomas
  magix.viewport.show-message
    message=>hello world";
				return;
			}

			if (!e.Params.Contains ("script") || e.Params["script"].Get<string>("") == "")
				throw new ArgumentException("need [script] node for magix.admin.run-script");

			string txt = e.Params["script"].Get<string>();

			string wholeTxt = txt.TrimStart();
			string method = "";
			if (wholeTxt.StartsWith("event:"))
			{
				method = wholeTxt.Split (':')[1];
				method = method.Substring(0, method.Contains("\n") ? method.IndexOf("\n") : method.Length);
				wholeTxt = wholeTxt.Contains("\n") ? 
					wholeTxt.Substring(wholeTxt.IndexOf("\n")).TrimStart() : 
						"";
			}

			// try to push code into active event executor, if it exists in page
			Node tmp = new Node();
			tmp["code"].Value = txt;

			RaiseActiveEvent(
				"magix.admin.set-code",
				tmp);

			tmp = new Node();
			tmp["code"].Value = wholeTxt;

			RaiseActiveEvent(
				"magix.code.code-2-node",
				tmp);

			RaiseActiveEvent(
				method, 
				tmp["json"].Get<Node>());
		}
	}
}
