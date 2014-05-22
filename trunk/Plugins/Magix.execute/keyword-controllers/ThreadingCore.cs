/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
	 * multi threading
	 */
	public class ThreadingCore : ActiveController
	{
		private Stack<object> stack = new Stack<object>();

		/**
		 * spawns new thread
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		public void magix_execute_fork(object sender, ActiveEventArgs e)
		{
			// TODO: Make thread safe, somehow ...
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.fork-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.fork-sample]");
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [fork] directly, except for inspect purposes");

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
		 * sleeps the current thread
		 */
		[ActiveEvent(Name = "magix.execute.wait")]
		public void magix_execute_wait(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.wait-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.wait-sample]");
                return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [wait] directly, except for inspect purposes");

			int milliseconds = ip.Get<int>(-1);
			ManualResetEvent evt = new ManualResetEvent(false);

            lock (stack)
            {
                stack.Push(evt);
            }
			try
			{
				RaiseActiveEvent(
					"magix.execute",
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

