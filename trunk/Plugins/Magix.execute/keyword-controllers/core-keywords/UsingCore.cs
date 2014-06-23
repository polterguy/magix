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
	 * using implementation
	 */
	public class UsingCore : ActiveController
	{
        /*
         * using keyword implementation
         */
        [ActiveEvent(Name = "magix.execute.using")]
        public static void magix_execute_using(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.using-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.using-sample]");
                return;
            }

            try
            {
                e.Params["_namespaces"].Add(new Node("item", ip.Get<string>()));
                RaiseActiveEvent(
                    "magix.execute",
                    e.Params);
            }
            finally
            {
                e.Params["_namespaces"][e.Params["_namespaces"].Count - 1].UnTie();
                if (e.Params["_namespaces"].Count == 0)
                    e.Params["_namespaces"].UnTie();
            }
        }
	}
}

