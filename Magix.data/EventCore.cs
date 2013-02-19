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
		 * Creates a new magix.execute Activ Event, which should contain magix.execute keywords,
		 * which will be raised when your "event" active event is raised. Submit the code
		 * in the "code" node
		 */
		[ActiveEvent(Name = "magix.execute.event")]
		public static void magix_execute_event (object sender, ActiveEventArgs e)
		{
			_hasNull = null;
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Overrides the active event in ""event""
with the code in the ""code"" expression. Functions 
as a ""magix.execute"" keyword.";
				e.Params["event"].Value = "foo.bar";
				e.Params["event"]["remotable"].Value = true;
				e.Params["event"]["code"]["Data"].Value = "thomas";
				e.Params["event"]["code"]["Backup"].Value = "thomas";
				e.Params["event"]["code"]["if"].Value = "[Data].Value==[Backup].Value";
				e.Params["event"]["code"]["if"]["magix.viewport.show-message"].Value = null;
				e.Params["event"]["code"]["if"]["magix.viewport.show-message"]["message"].Value = "Howdy World!!";
				e.Params["foo.bar"].Value = null;
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			string key = ip.Get<string>("");

			// If it finds "code", event will be created, otherwise deleted ...
			if (ip.Contains ("code"))
			{
				dp = ip["code"].Clone ();
			}
			else
			{
				dp = null;
			}

			bool remotable = false;
			if (ip.Contains ("remotable"))
				remotable = ip["remotable"].Get<bool>();

			Node parent = dp == null ? null : dp.Parent;
			if (dp != null)
				dp.SetParent(null);
			new DeterministicExecutor(
			delegate
				{
					lock (typeof(Node))
					{
						if (dp != null)
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
								db.Commit ();
								ActiveEvents.Instance.CreateEventMapping (key, "magix.execute._active-event-2-code-callback");
								if (remotable)
									ActiveEvents.Instance.MakeRemotable (key);
							}
							Node node = new Node();
							node["ActiveEvent"].Value = key;
							RaiseEvent ("magix.execute._event-overridden", node);
						}
						else
						{
							// Removing existing event
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
					}
				},
				delegate
				{
					dp.SetParent(parent);
				});
		}

		/**
		 * Null event handler for handling null active event overrides for the event keyword
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
		 * overridden with the event keyword
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

