/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Contains logic for creating and manipulating new system threads, aka; Forks
	 */
	public class ThreadingCore : ActiveController
	{
		private Stack<object> stack = new Stack<object>();

		// TODO: Make thread safe, somehow ...
		/**
		 * Spawns a new thread which the given
		 * code block will be executed within. Useful for long operations, 
		 * where you'd like to return to caller before the operation is
		 * finished
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		public void magix_execute_fork(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["fork"]["_data"]["value"].Value = "thomas";
				e.Params["fork"]["if"].Value = "[_data][value].Value==thomas";
				e.Params["fork"]["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["fork"]["if"]["raise"]["message"].Value = "hi thomas";
				e.Params["inspect"].Value = @"spawns a new thread, which the given
code block will be executed within.&nbsp;&nbsp;useful for long operations, 
where you'd like to return to caller, before the operation is
finished.&nbsp;&nbsp;the entire node-list underneath the [fork]
keyword, will be cloned, and passed into the magix.execute.fork active event,
for execution on a different thread.&nbsp;&nbsp;the forked
thread will not change any data on the original node set.&nbsp;&nbsp;
if the value of [fork] is true, the new thread will be executed 
as a fire-and-forget thread, bypassing any [wait] statements you 
might have.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.set] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node node = ip.Clone();

			if (node.Get<bool>(false))
			{
				Thread thread = new Thread(ExecuteThreadForget);
				thread.Start(node);
			}
			else
			{
				Thread thread = new Thread(ExecuteThread);
				thread.Start(node);
			}
		}

		private void ExecuteThread(object input)
		{
			Node node = input as Node;

			lock (stack)
				stack.Push(typeof(Thread));
			try
			{
				RaiseActiveEvent(
					"magix.execute",
					node);
			}
			finally
			{
				lock (stack)
				{
					stack.Pop();

					if (stack.Count > 0 && stack.Peek() is ManualResetEvent)
					{
						// signaling to release wait object
						ManualResetEvent evt = stack.Peek() as ManualResetEvent;
						evt.Set();
					}
				}
			}
		}

		private void ExecuteThreadForget(object input)
		{
			Node node = input as Node;

			RaiseActiveEvent(
				"magix.execute",
				node);
		}

		/**
		 * Sleeps the current thread for "time" milliseconds
		 */
		[ActiveEvent(Name = "magix.execute.wait")]
		public void magix_execute_wait(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["wait"].Value = null;
				e.Params["wait"]["fork"].Value = null;
				e.Params["inspect"].Value = @"will wait for multiple treads to finish.&nbsp;&nbsp;
all [fork] blocks created underneath [wait], will have to be finished, before the 
execution will leave the [wait] block.&nbsp;&nbsp;you can optionally set a maximum number 
of milliseconds, before the wait is dismissed as an integer value of [set].&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.set] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			int milliseconds = ip.Get<int>(-1);

			ManualResetEvent evt = new ManualResetEvent(false);

			lock (stack)
				stack.Push(evt);
			try
			{
				RaiseActiveEvent(
					"magix._execute",
					e.Params);

				if (milliseconds != -1)
					evt.WaitOne(milliseconds);
				else
					evt.WaitOne();
			}
			finally
			{
				lock (stack)
					stack.Pop();
			}
		}
	}
}

