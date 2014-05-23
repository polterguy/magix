/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Threading;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * multi threading
	 */
	public class ThreadingCore : ActiveController
	{
		private Stack<object> _stack = new Stack<object>();

		/*
		 * spawns new thread
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		public void magix_execute_fork(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
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

			Node node = ip.Clone();
			if (node.Get<bool>(false))
				new Thread(ExecuteThreadForget).Start(node);
			else
				new Thread(ExecuteThread).Start(node);
		}

		private void ExecuteThread(object input)
		{
			Node node = input as Node;

			lock (_stack)
				_stack.Push(typeof(Thread));
			try
			{
				RaiseActiveEvent(
					"magix.execute",
					node);
			}
			finally
			{
				lock (_stack)
				{
					_stack.Pop();
					if (_stack.Count > 0 && _stack.Peek() is ManualResetEvent)
					{
						// signaling to release wait object
						ManualResetEvent evt = _stack.Peek() as ManualResetEvent;
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

		/*
		 * sleeps the current thread
		 */
		[ActiveEvent(Name = "magix.execute.wait")]
		public void magix_execute_wait(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
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

			int milliseconds = ip.Get<int>(-1);
			ManualResetEvent evt = new ManualResetEvent(false);

            lock (_stack)
                _stack.Push(evt);
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
				lock (_stack)
					_stack.Pop();
			}
		}
	}
}

