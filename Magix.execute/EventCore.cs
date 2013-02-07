/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using Magix.UX.Builder;
using Db4objects.Db4o;
using Db4objects.Db4o.Config;

namespace Magix.execute
{
	/**
	 */
	[ActiveController]
	public class EventCore : ActiveController
	{
		private static string _dbFile = "store.db4o";

		public class Event
		{
			private string _key;
			private Node _node;

			public Event(Node node, string key)
			{
				_node = node;
				_key = key;
			}

			public string Key
			{
				get { return _key; }
				set { _key = value; }
			}

			public Node Node
			{
				get { return _node; }
				set { _node = value; }
			}
		}

		/**
		 */
		[ActiveEvent(Name = "magix.core.application-startup")]
		public static void magix_core_application_startup (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["initial-startup-of-process"].Value = null;
				e.Params["inspect"].Value = @"Called during startup
of application to make sure our Active Events, 
which are dynamically tied towards serialized 
magix.execute blocks of code are being correctly 
re-mapped. ""initial-startup-of-process"" must exists to run event.";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);

				foreach (Event idx in db.QueryByExample (new Event(null, null)))
				{
					ActiveEvents.Instance.CreateEventMapping (idx.Key, "magix.execute._active-event-2-code-callback");
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.function")]
		public void magix_execute_function (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"].Value = "thomas";
				e.Params["Backup"].Value = "thomas";
				e.Params["OR"]["Path2Code"]["if"].Value = "[Data].Value==[Backup].Value";
				e.Params["OR"]["Path2Code"]["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["OR"]["Path2Code"]["if"]["raise"]["params"]["message"].Value = "Howdy lady!!!";
				e.Params["event"].Value = "foo-bar";
				e.Params["code"]["if"].Value = "[Data].Value==[Backup].Value";
				e.Params["code"]["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["code"]["if"]["raise"]["params"]["message"].Value = "Howdy boy!!";
				e.Params["context"].Value = "[OR][Path2Code]";
				e.Params["inspect"].Value = @"Overrides the active event in ""event""
with either the code in ""code"" or the code pointed
to from the ""context"" Value's expression.  Functions 
as a ""magix.execute"" keyword.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string key = ip["event"].Get<string>();

			// If function contains a child node with "code" name it
			// will use the ip pointer as the place to extract the code.
			// Otherwise it will use the dp pointer, Data Pointer, e.g.
			// the [.] expression from a for-each, etc as the place
			// it will expect find 
			// the serialized code ...
			// UNLESS it finds a "Context" node, which case that will become the 
			// place which it will use 
			if (ip.Contains ("code"))
			{
				dp = ip["code"];
			}
			else if (ip.Contains ("context"))
			{
				Node tmp = Expressions.GetExpressionValue (ip["context"].Get<string>(), dp, ip) as Node;
				dp = tmp;
			}
			dp = dp.Clone ();

			Node parent = dp.Parent;
			dp.Parent = null;
			new DeterministicExecutor(
			delegate
				{
					using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
					{
						db.Ext ().Configure ().UpdateDepth (1000);
						db.Ext ().Configure ().ActivationDepth (1000);

						bool found = false;
						foreach (Event idx in db.QueryByExample (new Event(null, key)))
						{
							idx.Node = dp;
							db.Store (idx);
							found = true;
							break;
						}
						if (!found)
						{
							db.Store (new Event(dp, key));
						}
						db.Commit ();
						ActiveEvents.Instance.CreateEventMapping (key, "magix.execute._active-event-2-code-callback");
					}
				},
				delegate
				{
					dp.Parent = parent;
				});

			Node node = new Node();
			node["ActiveEvent"].Value = key;
			RaiseEvent ("magix.execute._event-overridden", node);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute.remove-function")]
		public void magix_execute_remove_function (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event"].Value = "foo-bar";
				e.Params["inspect"].Value = @"Removes and deletes the active event
found in the ""event"" child node. Functions as a ""magix.execute"" keyword.";
				return;
			}
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Params["event"].Get<string>();

				foreach (Event idx in db.QueryByExample (new Event(null, key)))
				{
					db.Delete (idx);
					break;
				}
				db.Commit ();
				ActiveEvents.Instance.RemoveMapping (key);
			}

			Node node = new Node();
			node["ActiveEvent"].Value = e.Params["event"].Get<string>();
			RaiseEvent ("magix.execute._event-override-removed", node);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.execute._active-event-2-code-callback")]
		public void magix_data__active_event_2_code_callback (object sender, ActiveEventArgs e)
		{
			Node caller = null;
			using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
			{
				db.Ext ().Configure ().UpdateDepth (1000);
				db.Ext ().Configure ().ActivationDepth (1000);
				string key = e.Name;

				foreach (Event idx in db.QueryByExample (new Event(null, key)))
				{
					idx.Node.Name = null;
					if (e.Params.Contains ("inspect"))
					{
						e.Params["event"].Value = e.Name;
						e.Params["code"].Clear ();
						e.Params["code"].AddRange (idx.Node);
						e.Params.Value = idx.Node.Value;
						e.Params["inspect"].Value = @"This is a dynamically created
active event, containing ""magix.executor"" code, meaning keywords from the executor,
such that this serialized code will be called upon the raising of this event.";
					}
					else
					{
						caller = idx.Node;
					}
					break;
				}
			}
			if (caller != null)
				RaiseEvent ("magix.execute", caller);
		}
	}
}

