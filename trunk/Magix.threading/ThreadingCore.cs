/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Threading;
using Magix.Core;

namespace Magix.threading
{
	/**
	 * Contains logic for creating and manipulating new system threads, aka; Forks
	 */
	public class ExecuteCore : ActiveController
	{
		/**
		 * Spawns a new thread which the given
		 * code block will be executed within. Useful for long operations, 
		 * where you'd like to return to caller before the operation is
		 * finished
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		public static void magix_execute_fork (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["fork"]["Data"]["Value"].Value = "thomas";
				e.Params["fork"]["if"].Value = "[Data][Value].Value==thomas";
				e.Params["fork"]["if"]["raise"].Value = "magix.viewport.show-message";
				e.Params["fork"]["if"]["raise"]["message"].Value = "Hi Thomas!";
				e.Params["inspect"].Value = @"Spawns a new thread, which the given
code block will be executed within. Useful for long operations, 
where you'd like to return to caller, before the operation is
finished. Notice the entire node-list underneath the ""fork""
keyword, will be cloned, and passed into the ""fork"" active event,
for execution on a different thread. This means the forked
thread will not change any data on the original node-set.";
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

		private static void ExecuteThrea(object input)
		{
			Node node = input as Node;

			RaiseEvent(
				"magix.execute",
				node);
		}

		// TODO: Do we really NEED this one, or would "wait" keyword be enough ...?
		/**
		 * Sleeps the current thread for "time" milliseconds
		 */
		[ActiveEvent(Name = "magix.execute.sleep")]
		public static void magix_execute_sleep(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = 500;
				e.Params["sleep"].Value = 500;
				e.Params["inspect"].Value = @"Sleeps the current threat
for Value number of milliseconds.";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			Thread.Sleep(ip.Get<int>());
		}
	}
}

