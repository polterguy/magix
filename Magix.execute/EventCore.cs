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
	 * Controller logic for handling magix.execute overrides, where you've
	 * overridden an Active Event with magix.execute code
	 */
	public class EventCore : ActiveController
	{
		private static string _dbFile = "store.db4o";
		private static bool? _hasNull;

		public class Event
		{
			private string _key;
			private Node _node;
			private bool _remotable;

			public Event(Node node, string key, bool remotable)
			{
				_node = node;
				_key = key;
				_remotable = remotable;
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

			public bool Remotable
			{
				get { return _remotable; }
				set { _remotable = value; }
			}
		}

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
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);

					foreach (Event idx in db.QueryByExample (new Event(null, null, false)))
					{
						ActiveEvents.Instance.CreateEventMapping (idx.Key, "magix.execute._active-event-2-code-callback");
						if (idx.Remotable)
							ActiveEvents.Instance.MakeRemotable (idx.Key);
					}
				}
			}
		}

		/**
		 * Creates a new magix.execute function, which should contain magix.execute keywords,
		 * which will be raised when your "event" active event is raised. Submit the code
		 * in the "code" node
		 */
		[ActiveEvent(Name = "magix.execute.function")]
		public static void magix_execute_function (object sender, ActiveEventArgs e)
		{
			_hasNull = null;
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"].Value = "thomas";
				e.Params["Backup"].Value = "thomas";
				e.Params["event"].Value = "foo-bar";
				e.Params["remotable"].Value = true;
				e.Params["code"]["if"].Value = "[Data].Value==[Backup].Value";
				e.Params["code"]["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["code"]["if"]["raise"]["message"].Value = "Howdy boy!!";
				e.Params["inspect"].Value = @"Overrides the active event in ""event""
with the code in the ""code"" expression. Functions 
as a ""magix.execute"" keyword.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string key = ip["event"].Get<string>("");

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
			else
			{
				throw new ArgumentException("function needs code to execute");
			}
			dp = dp.Clone ();

			bool remotable = false;
			if (ip.Contains ("remotable"))
				remotable = ip["remotable"].Get<bool>();

			Node parent = dp.Parent;
			dp.SetParent(null);
			new DeterministicExecutor(
			delegate
				{
					lock (typeof(Node))
					{
						using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
						{
							db.Ext ().Configure ().UpdateDepth (1000);
							db.Ext ().Configure ().ActivationDepth (1000);

							bool found = false;

							foreach (Event idx in db.QueryByExample (new Event(null, key, false)))
							{
								idx.Node = dp;
								idx.Remotable = remotable;
								db.Store (idx);
								found = true;
								break;
							}
							if (!found)
							{
								db.Store (new Event(dp, key, remotable));
								found = true;
							}
							if (!found)
							{
								foreach (Event idx in db.QueryByExample (new Event(null, key, true)))
								{
									idx.Node = dp;
									idx.Remotable = remotable;
									db.Store (idx);
									found = true;
									break;
								}
								if (!found)
								{
									db.Store (new Event(dp, key, remotable));
								}
							}
							db.Commit ();
							ActiveEvents.Instance.CreateEventMapping (key, "magix.execute._active-event-2-code-callback");
							if (remotable)
								ActiveEvents.Instance.MakeRemotable (key);
						}
					}
				},
				delegate
				{
					dp.SetParent(parent);
				});

			Node node = new Node();
			node["ActiveEvent"].Value = key;
			RaiseEvent ("magix.execute._event-overridden", node);
		}

		/**
		 * Remove the given "event" active event event override
		 */
		[ActiveEvent(Name = "magix.execute.remove-function")]
		public static void magix_execute_remove_function (object sender, ActiveEventArgs e)
		{
			_hasNull = null;
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event"].Value = "foo-bar";
				e.Params["inspect"].Value = @"Removes and deletes the active event
found in the ""event"" child node. Functions as a ""magix.execute"" keyword.";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string key = ip.Get<string>();
			if (key == null)
				throw new ArgumentException("Cannot remove null function");
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);

					foreach (Event idx in db.QueryByExample (new Event(null, key, false)))
					{
						db.Delete (idx);
						if (idx.Remotable)
							ActiveEvents.Instance.RemoveRemotable (idx.Key);
						break;
					}
					db.Commit ();
					ActiveEvents.Instance.RemoveMapping (key);
				}
			}

			Node node = new Node();
			node["ActiveEvent"].Value = e.Params["event"].Get<string>();
			RaiseEvent ("magix.execute._event-override-removed", node);
		}

		/**
		 * Null event handler for handling null active event overrides for the function keyword
		 * in magix.execute
		 */
		[ActiveEvent(Name = "")]
		public static void magix_data__active_event_2_code_callback_null_helper (object sender, ActiveEventArgs e)
		{
			// Small optimization, to not traverse Data storage file for EVERY SINGLE ACTIVE EVENT ...!
			if (_hasNull.HasValue && !_hasNull.Value)
				return;

			Node caller = null;
			_hasNull = false;
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);

					foreach (Event idx in db.QueryByExample (new Event(null, null, false)))
					{
						if (idx.Key == "")
						{
							idx.Node.Name = null;
							caller = idx.Node;
							_hasNull = true;
							break;
						}
					}
				}
			}
			if (caller != null)
			{
				Node tmp = new Node();
				tmp.AddRange (caller.UnTie ());
				tmp["_method"].Value = e.Name;
				tmp["_method"].AddRange (e.Params.Clone ());
				RaiseEvent ("magix.execute", tmp, true);
			}
		}

		/**
		 * Handled to make sure we map our serialized active event overrides, the ones
		 * overridden with the function keyword
		 */
		[ActiveEvent(Name = "magix.execute._active-event-2-code-callback")]
		public static void magix_data__active_event_2_code_callback (object sender, ActiveEventArgs e)
		{
			bool remote = false;
			if (e.Params.Contains ("remote"))
				remote = e.Params["remote"].Get<bool>();
			Node caller = null;
			lock (typeof(Node))
			{
				using (IObjectContainer db = Db4oFactory.OpenFile(_dbFile))
				{
					db.Ext ().Configure ().UpdateDepth (1000);
					db.Ext ().Configure ().ActivationDepth (1000);
					string key = e.Name;

					foreach (Event idx in db.QueryByExample (new Event(null, key, remote)))
					{
						idx.Node.Name = null;
						if (e.Params.Contains ("inspect"))
						{
							e.Params["event"].Value = e.Name;
							e.Params["code"].Clear ();
							e.Params["code"].AddRange (idx.Node);
							e.Params["remotable"].Value = idx.Remotable;
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
					if (caller == null && remote == false)
					{
						foreach (Event idx in db.QueryByExample (new Event(null, key, true)))
						{
							idx.Node.Name = null;
							if (e.Params.Contains ("inspect"))
							{
								e.Params["event"].Value = e.Name;
								e.Params["code"].Clear ();
								e.Params["code"].AddRange (idx.Node);
								e.Params["remotable"].Value = idx.Remotable;
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
				}
			}
			if (caller != null)
			{
				RaiseEvent ("magix.execute", caller);
				e.Params.ReplaceChildren (caller);
			}
		}
	}
}

