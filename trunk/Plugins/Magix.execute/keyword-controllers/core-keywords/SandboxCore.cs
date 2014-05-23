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
                    "[magix.execute.sandbox-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.sandbox-sample]");
                return;
            }

            if (!ip.Contains("code"))
                throw new ArgumentException("you need to supply a [code] block to [sandbox] active event");
            if (!ip.Contains("whitelist"))
                throw new ArgumentException("you need to supply a [whitelist] block to [sandbox] active event");

            Node oldWhitelist = null;
            if (e.Params.Contains("_whitelist"))
            {
                foreach (Node idx in ip["whitelist"])
                {
                    if (!e.Params["_whitelist"].Contains(idx.Name))
                        throw new ArgumentException("cannot whitelist an active event that was blacklisted previously");
                }
                oldWhitelist = Node.Create("_whitelist", null, e.Params["_whitelist"].Clone());
            }

            e.Params["_whitelist"].Clear();
            e.Params["_whitelist"].AddRange(ip["whitelist"].Clone());
            try
            {
                e.Params["_ip"].Value = ip["code"];
                RaiseActiveEvent(
                    "magix.execute",
                    e.Params);
            }
            finally
            {
                e.Params["_whitelist"].UnTie();
                if (oldWhitelist != null)
                    e.Params["_whitelist"].AddRange(oldWhitelist);
            }
        }
	}
}

