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
	internal sealed class ThreadingCore : ActiveController
	{
		/*
		 * spawns new thread
		 */
		[ActiveEvent(Name = "magix.execute.fork")]
		private static void magix_execute_fork(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.fork-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.fork-sample]");
                return;
			}

            Node nPars = e.Params.Clone();
            nPars["_ip"].Value = e.Params["_ip"].Get<Node>().Clone();
            nPars["_dp"].Value = nPars["_ip"].Value;
            nPars["_orig-pars"].Value = e.Params;
            nPars["_root-only-execution"].UnTie();
            if (ip.Get<bool>(false) || !e.Params.Contains("_threads"))
                new Thread(ExecuteThreadForget).Start(nPars);
            else
            {
                nPars["_orig-ip"].Value = e.Params["_ip"].Get<Node>();
                new Thread(ExecuteThread).Start(nPars);
            }
		}

		private static void ExecuteThread(object input)
		{
            Node node = input as Node;
            Node origPars = node["_orig-pars"].Get<Node>();

            int beforeCount;
            lock (origPars)
            {
                beforeCount = origPars["_current-executed-iterations"].Get<int>();
            }

            try
            {
                RaiseActiveEvent(
                    "magix.execute",
                    node);
            }
            catch (Exception err)
            {
                while (err.InnerException != null)
                    err = err.InnerException;
                lock (origPars)
                {
                    origPars["_threads"]["exceptions"].Add(new Node("error", err.Message.Trim()));
                }
            }

            int afterCount = node["_current-executed-iterations"].Get<int>();
            int delta = afterCount - beforeCount;

            Node origIp = node["_orig-ip"].Get<Node>();
            origIp.Clear();
            origIp.AddRange(node["_ip"].Get<Node>());

            lock (origPars)
            {
                ManualResetEvent reset = origPars["_threads"]["reset"].Get<ManualResetEvent>();
                int threadCount = origPars["_threads"]["threadcount"].Get<int>();
                threadCount -= 1;
                origPars["_threads"]["threadcount"].Value = threadCount;
                if (threadCount == 0)
                    reset.Set();

                // increasing execution count
                beforeCount = origPars["_current-executed-iterations"].Get<int>();
                origPars["_current-executed-iterations"].Value = beforeCount + delta;
            }
		}

		private static void ExecuteThreadForget(object input)
		{
            Node node = input as Node;
            Node origPars = node["_orig-pars"].Get<Node>();

            int beforeCount;
            lock (origPars)
            {
                beforeCount = origPars["_current-executed-iterations"].Get<int>();
            }

            try
            {
                RaiseActiveEvent(
                    "magix.execute",
                    node);
            }
            catch (Exception err)
            {
                while (err.InnerException != null)
                    err = err.InnerException;

                Node logNode = new Node();
                logNode["header"].Value = "thread choked";
                logNode["body"].Value = "[fork] threw an unhandled exception, message was; '" + err.Message + "'";
                logNode["code"].AddRange(node["_ip"].Get<Node>());
                logNode["error"].Value = true;
                RaiseActiveEvent(
                    "magix.log.append",
                    logNode);
            }

            int afterCount = node["_current-executed-iterations"].Get<int>();
            int delta = afterCount - beforeCount;

            lock (origPars)
            {
                // increasing execution count
                beforeCount = origPars["_current-executed-iterations"].Get<int>();
                origPars["_current-executed-iterations"].Value = beforeCount + delta;
            }
        }

		/*
		 * sleeps the current thread till all children threads are finished
		 */
		[ActiveEvent(Name = "magix.execute.wait")]
		private static void magix_execute_wait(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.wait-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.wait-sample]");
                return;
			}

            int noChildren = 0;
            foreach (Node idxIp in ip)
            {
                if (idxIp.Name != "fork")
                    throw new ArgumentException("[wait] can only have [fork] as children");
                noChildren += 1;
            }
            if (noChildren == 0)
                return;
            
            int milliseconds = ip.Get<int>(-1);
            using (ManualResetEvent reset = new ManualResetEvent(false))
            {
                try
                {
                    e.Params["_threads"]["reset"].Value = reset;
                    e.Params["_threads"]["threadcount"].Value = noChildren;
                    foreach (Node idxIp in ip)
                    {
                        e.Params["_ip"].Value = idxIp;
                        e.Params["_root-only-execution"].Value = true;
                        RaiseActiveEvent(
                            "magix.execute",
                            e.Params);
                    }
                    if (milliseconds != -1)
                        reset.WaitOne(milliseconds);
                    else
                        reset.WaitOne();

                    if (e.Params["_threads"].Contains("exceptions"))
                    {
                        string exceptions = "";
                        foreach (Node idxErr in e.Params["_threads"]["exceptions"])
                        {
                            exceptions += idxErr.Get<string>() + "\r\n";
                        }
                        exceptions = exceptions.TrimEnd();
                        throw new ApplicationException("one or more threads beneath [wait] choked, message from thread was; '" + exceptions + "'");
                    }
                }
                finally
                {
                    e.Params["_root-only-execution"].UnTie();
                    e.Params["_threads"].UnTie();
                }
            }
		}
	}
}

