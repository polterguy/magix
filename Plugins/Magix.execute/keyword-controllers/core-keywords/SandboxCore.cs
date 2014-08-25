/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Configuration;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * sandbox keyword
	 */
	public class SandboxCore : ActiveController
	{
         /*
          * sandbox keyword implementation
          */
        [ActiveEvent(Name = "magix.execute.sandbox")]
        public static void magix_execute_sandbox(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sandbox-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sandbox-sample]");
                return;
            }

            // verifying sandbox has a [code] block
            if (!ip.Contains("code"))
                throw new ArgumentException("you need to supply a [code] block to [sandbox] active event");

            // verifying sandbox has a [whitelist]
            if (!ip.Contains("whitelist"))
                throw new ArgumentException("you need to supply a [whitelist] block to [sandbox] active event");

            // executing sandboxed code
            SandboxCode(e.Params, ip);
        }

        /*
         * actual implementation of sandbox
         */
        private static void SandboxCode(Node pars, Node ip)
        {
            // storing and clearing old whitelist
            // and checking to see that no keyword not previously whitelisted has been whitelisted
            Node oldWhitelist = null;
            if (pars.Contains("_whitelist"))
            {
                foreach (Node idx in ip["whitelist"])
                {
                    if (!pars["_whitelist"].Contains(idx.Name))
                        throw new HyperlispExecutionErrorException("cannot [whitelist] an active event that was blacklisted in a previous [sandbox]");
                }
                oldWhitelist = pars["_whitelist"].Clone();
                pars["_whitelist"].Clear();
            }

            // setting new whitelist
            pars["_whitelist"].AddRange(ip["whitelist"].Clone());
            try
            {
                pars["_ip"].Value = ip["code"];
                RaiseActiveEvent(
                    "magix.execute",
                    pars);
            }
            finally
            {
                pars["_whitelist"].UnTie();
                if (oldWhitelist != null)
                    pars["_whitelist"].AddRange(oldWhitelist);
            }
        }
	}
}

