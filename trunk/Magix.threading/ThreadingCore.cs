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

namespace Magix.threading
{
	/**
	 * Contains logic for creating and manipulating new system threads, aka; Forks
	 */
	public class ExecuteCore : ActiveController
	{
		private Stack<object> stack = new Stack<object>();

		/**
		 * Spawns a new thread which the given
		 * code block will be executed within. Useful for long operations, 
		 * where you'd like to return to caller before the operation is
		 * finished
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		public void magix_execute_fork(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
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
thread will not change any data on the original node-set";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			Node node = new Node();

			// Need to CLONE parameters here, to not get race-conditions in changing them ...
			node["_ip"].Value = ip.Clone();
			node["_dp"].Value = dp.Clone();

			Thread thread = new Thread(ExecuteThread);
			thread.Start(node);
		}

		private void ExecuteThread(object input)
		{
			lock (stack)
				stack.Push(typeof(Thread));

			try
			{
				Node node = input as Node;

				RaiseActiveEvent(
					"magix.execute",
					node);
			}
			finally
			{
				lock (stack)
				{
					object tmp = stack.Pop();

					if (stack.Count > 0 && stack.Peek() is ManualResetEvent)
					{
						// signaling to release wait object
						ManualResetEvent evt = stack.Pop() as ManualResetEvent;
						evt.Set();
					}
				}
			}
		}

		/**
		 * Sleeps the current thread for "time" milliseconds
		 */
		[ActiveEvent(Name = "magix.execute.wait")]
		public void magix_execute_wait(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["wait"].Value = null;
				e.Params["inspect"].Value = @"will wait for multiple treads to finish.&nbsp;&nbsp;
all [fork] blocks created underneath a [wait], will have to be finished, before the 
execution will leave the [wait] block";
				return;
			}

			ManualResetEvent evt = new ManualResetEvent(false);

			stack.Push(evt);

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params ["_dp"].Value as Node;

			Node node = new Node();

			node["_ip"].Value = ip.Clone();
			node["_dp"].Value = dp.Clone();

			RaiseActiveEvent(
				"magix.execute",
				node);

			evt.WaitOne();
		}
	}
}

