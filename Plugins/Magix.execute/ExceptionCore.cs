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
	 * exception hyper lisp logic
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
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>use the [try] keyword to create
a block of [code], which will execute your [catch] execution block of code, if an 
exception is thrown, inside your [code] execution block</p><p>this exception handler
will be invoked, even if an exception occurs any place underneath your try code 
block, deep within your logic.&nbsp;&nbsp;you can handle exceptions being raised 
in sub-functions, or even recursively invoked active events, or natively thrown
exceptions this way</p><p>if an exception is thrown, you can access the description 
of the exception in the [exception] node underneath your catch statement.&nbsp;&nbsp;
you can also add a [finally] piece of code block underneath the [try], which will 
always be executed, regardless of whether or not an exception was thrown</p><p>
thread safe</p>";
				e.Params["try"].Value = null;
				e.Params["try"]["code"]["throw"].Value = "to try or not to try";
				e.Params["try"]["code"]["magix.viewport.show-message"]["message"].Value = "crap, didn't work";
				e.Params["try"]["catch"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
				e.Params["try"]["catch"]["set"]["value"].Value = "[@][exception].Value";
				e.Params["try"]["catch"]["magix.viewport.show-message"].Value = null;
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.try] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

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

		/**
		 * throw support
		 */
		[ActiveEvent(Name = "magix.execute.throw")]
		public static void magix_execute_throw(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>throws an exception, which will stop 
the entire current execution, and halt back to the previous catch, in the stack of 
active events.&nbsp;&nbsp;[exception] in [catch] becomes the value of the [throw] 
node</p><p>use together with [try] to handle errors</p><p>thread safe</p>";
				e.Params["try"]["code"]["throw"].Value = "some exception error message";
				e.Params["try"]["catch"]["set"].Value = "[@][magix.viewport.show-message][message].Value";
				e.Params["try"]["catch"]["set"]["value"].Value = "[@][exception].Value";
				e.Params["try"]["catch"]["magix.viewport.show-message"].Value = null;
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.throw] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			throw new ExceptionCore.ManagedHyperLispException(ip.Get<string>());
		}
	}
}

