/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using System.Globalization;

namespace Magix.date
{
	/*
	 * contains the magix.date main active events
	 */
	public class DateCore : ActiveController
	{
        /*
         * returns the "now" date
         */
        [ActiveEvent(Name = "magix.date.now")]
        public static void magix_date_now(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.add-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.add-sample]");
                return;
            }

            ip["value"].Value = DateTime.Now;
        }
    }
}

