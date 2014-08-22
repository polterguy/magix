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
         * exception type thrown when [stop] keyword is invoked
         */
        public class ManagedHyperLispException : ApplicationException
        {
            public string TypeInfo { get; set; }

            public ManagedHyperLispException(string msg, string typeInfo)
                : base(msg)
            {
                TypeInfo = typeInfo;
            }
        }

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
                throw new ApplicationException("you need a [code] block inside your [try] keyword, which is supposed to contain the tried code");
            if (!ip.Contains("catch") && !ip.Contains("finally"))
                throw new ApplicationException("you need at least one of [catch] or [finally] within your [try] keyword");
            try
            {
                ExecuteTry(e.Params, ip);
            }
            catch (Exception err)
            {
                if (ExecuteCatch(e.Params, ip, err))
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

            throw new ExceptionCore.ManagedHyperLispException(ip.Get<string>(), ip.GetValue<string>("type", null));
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
         * executes try catch block
         */
        private static bool ExecuteCatch(Node pars, Node ip, Exception err)
        {
            if (ip.Contains("catch"))
            {
                while (err.InnerException != null)
                    err = err.InnerException;
                if (!string.IsNullOrEmpty(ip["catch"].Get<string>()))
                {
                    if (!(err is ManagedHyperLispException))
                        return false; // not to be handled by this exception handler
                    string type = ip["catch"].Get<string>();
                    if ((err as ManagedHyperLispException).TypeInfo != type)
                        return true; // type mismatch, rethrow
                }

                ip["catch"]["exception"].Value = err.Message;
                pars["_ip"].Value = ip["catch"];

                RaiseActiveEvent(
                    "magix.execute",
                    pars);
                return false; // don't rethrow, exception was handled
            }
            return true; // no catch, return rethrow
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

