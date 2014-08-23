/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
    /*
     * exception hyperlisp logic
     */
    internal sealed class ExceptionCore : ActiveController
    {
        /*
         * creates a try block
         */
        [ActiveEvent(Name = "magix.execute.try")]
        public static void magix_execute_try(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.try-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.try-sample]");
                return;
            }

            if (!ip.Contains("code"))
                throw new HyperlispSyntaxErrorException("you need a [code] block inside your [try] keyword, which is supposed to contain the tried code");
            if (!ip.Contains("catch") && !ip.Contains("finally"))
                throw new HyperlispSyntaxErrorException("you need at least one of [catch] or [finally] within your [try] keyword");
            try
            {
                ExecuteTry(e.Params, ip);
            }
            catch (Exception err)
            {
                if (!ExecuteCatch(e.Params, ip, err))
                    throw;
            }
            finally
            {
                ExecuteFinally(e.Params, ip);
            }
        }

        /*
         * throw support
         */
        [ActiveEvent(Name = "magix.execute.throw")]
        public static void magix_execute_throw(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.throw-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.throw-sample]");
                return;
            }

            if (string.IsNullOrEmpty(ip.Get<string>()))
            {
                // this is a rethrow, checking stack to see if we're inside a catch statement
                Node stackTraceNode = ip;
                while (stackTraceNode != null)
                {
                    if (stackTraceNode.Name == "catch")
                        break;
                    stackTraceNode = stackTraceNode.Parent;
                }
                if (stackTraceNode != null)
                    e.Params["_rethrow"].Value = true;
                else
                    throw new HyperlispSyntaxErrorException("cannot rethrow an exception, unless you're inside of a [catch] block");
            }
            else
                throw new HyperlispException(ip.Get<string>(), ip.GetValue<string>("type", null));
        }

        /*
         * executes try code block
         */
        private static void ExecuteTry(Node pars, Node ip)
        {
            pars["_ip"].Value = ip["code"];
            RaiseActiveEvent(
                "magix.execute",
                pars);
        }

        /*
         * executes try catch block, returns true if exception was caught
         */
        private static bool ExecuteCatch(Node pars, Node ip, Exception err)
        {
            bool catched = false;
            if (ip.Contains("catch"))
            {
                while (err.InnerException != null)
                    err = err.InnerException;
                HyperlispException hlEx = err as HyperlispException;

                foreach (Node idxNode in ip)
                {
                    if (idxNode.Name == "catch")
                    {
                        if (!string.IsNullOrEmpty(idxNode.Get<string>()))
                        {
                            // checking to see if type information on catch is correct according to type of exception
                            if (hlEx != null)
                                catched = hlEx.TypeInfo == idxNode.Get<string>();
                            else
                                catched = err.GetType().FullName == idxNode.Get<string>();
                        }
                        else
                            catched = true; // catch all

                        if (catched)
                        {
                            // if ExecuteCatchBlock returns false, we're supposed to rethrow exception
                            catched = ExecuteCatchBlock(pars, idxNode, err);
                            break;
                        }
                    }
                }
            }
            return catched;
        }

        /*
         * executes specific catch block, returns true if exception is handled and should not be rethrown
         */
        private static bool ExecuteCatchBlock(Node pars, Node catchBlock, Exception err)
        {
            // executing [catch] block
            catchBlock["exception"].Value = err.Message;
            pars["_ip"].Value = catchBlock;

            RaiseActiveEvent(
                "magix.execute",
                pars);

            if (pars.GetValue("_rethrow", false))
            {
                // checking to see if we should rethrow exception
                pars["_rethrow"].UnTie();
                return false;
            }
            return true;
        }

        /*
         * executes try finally block
         */
        private static void ExecuteFinally(Node pars, Node ip)
        {
            if (ip.Contains("finally"))
            {
                pars["_ip"].Value = ip["finally"];

                RaiseActiveEvent(
                    "magix.execute",
                    pars);
            }
        }
    }
}

