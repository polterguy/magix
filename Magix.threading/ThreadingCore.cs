/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
				e.Params["inspect"].Value = @"Spawns a new thread which the given
code block will be executed within. Useful for long operations, 
where you'd like to return to caller before the operation is
finished.";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			Node pars = new Node();
			pars["_ip"].Value = ip.Clone ();
			pars["_dp"].Value = dp.Clone ();

			Thread thread = new Thread(ExecuteThread);
			thread.Start (pars);
		}

		private static void ExecuteThread (object pars)
		{
			Node par = pars as Node;
			RaiseEvent (
				"magix.execute",
				par);
		}

		/**
		 * Sleeps the current thread for "time" milliseconds
		 */
		[ActiveEvent(Name = "magix.execute.sleep")]
		public static void magix_execute_sleep (object sender, ActiveEventArgs e)
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
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Thread.Sleep (ip.Get<int>());
		}
	}
}

