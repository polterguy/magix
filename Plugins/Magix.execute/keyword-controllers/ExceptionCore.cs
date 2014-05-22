/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * exception hyperlisp logic
	 */
	public class ExceptionCore : ActiveController
	{
		public class ManagedHyperLispException : Exception
		{
			public ManagedHyperLispException(string msg)
				: base(msg)
			{ }
		}

		/*
		 * creates a try block
		 */
		[ActiveEvent(Name = "magix.execute.try")]
		public static void magix_execute_try(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.try-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.try-sample]");
                return;
			}

			if (!ip.Contains("code"))
				throw new ApplicationException("you need a [code] block inside your [try] statement, which is supposed to contain the tried code");

			if (!ip.Contains("catch"))
				throw new ApplicationException("you need a [catch] block inside your [try] statement");

			e.Params["_ip"].Value = ip["code"];
			try
			{
				RaiseActiveEvent(
					"magix.execute",
					e.Params);
			}
			catch (Exception err)
			{
                if (ip.Contains("catch"))
                {
                    while (err.InnerException != null)
                        err = err.InnerException;

                    ip["catch"]["exception"].Value = err.Message;
                    e.Params["_ip"].Value = ip["catch"];

                    RaiseActiveEvent(
                        "magix.execute",
                        e.Params);
                }
			}
			finally
			{
                if (ip.Contains("finally"))
                {
                    e.Params["_ip"].Value = ip["finally"];

                    RaiseActiveEvent(
                        "magix.execute",
                        e.Params);
                }
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
                    "[magix.execute.throw-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.throw-sample]");
                return;
			}

			throw new ExceptionCore.ManagedHyperLispException(ip.Get<string>());
		}
	}
}

