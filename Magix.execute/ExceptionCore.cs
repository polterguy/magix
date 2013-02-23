/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Threading;
using Magix.Core;

namespace Magix.execute
{
	/**
	 */
	public class ExceptionCore : ActiveController
	{
		/**
		 * Creates a try block, with an associated code block, and a catch block,
		 * which will be invoked if an exception is thrown. The catch statement
		 * will only be invoked if an exception is thrown
		 */
		[ActiveEvent(Name = "magix.execute.try")]
		public static void magix_execute_try (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Use the ""try"" keyword to create
a block of code, which will execute your ""catch"" 
execution block of code, if an exception is thrown
inside your ""code"" block. This exception handler
will be invoked, even if an exception occurs any place
underneath your try code block, deep within your logic. 
Meaning, you can handle errors being raised in 
sub-functions, or invoked active events this way.";
				e.Params["try"].Value = null;
				e.Params["try"]["code"]["throw"].Value = "To Throw or Not to Throw!!";
				e.Params["try"]["code"]["magix.viewport.show-message"]["message"].Value = "NOT supposed to show!!";
				e.Params["try"]["catch"]["set"].Value = "[magix.viewport.show-message][message].Value";
				e.Params["try"]["catch"]["set"]["value"].Value = "[exception].Value";
				e.Params["try"]["catch"]["magix.viewport.show-message"].Value = null;
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			if (!ip.Contains ("code"))
				throw new ApplicationException("No code block inside of try statement");

			if (!ip.Contains ("catch"))
				throw new ApplicationException("No catch block inside of try statement");

			try
			{
				RaiseEvent (
					"magix.execute",
					ip["code"]);
			}
			catch (Exception err)
			{
				while (err.InnerException != null)
					err = err.InnerException;

				if (ip["code"].Contains ("_state"))
					ip["code"]["_state"].UnTie ();

				ip["catch"]["exception"].Value = err.Message;
				RaiseEvent (
					"magix.execute",
					ip["catch"]);
			}
		}

		/**
		 * Throws an exception with the Value descriptive message
		 */
		[ActiveEvent(Name = "magix.execute.throw")]
		public static void magix_execute_throw (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Throws an exception, which
will stop the entire current execution, and halt 
back to the previous catch in the stack of 
active events. Message thrown becomes the 
Value of the throw Node. Use together with
""try"" to handle errors.";
				e.Params["try"]["code"]["throw"].Value = "Some Exception Error Message";
				e.Params["try"]["catch"]["set"].Value = "[magix.viewport.show-message][message].Value";
				e.Params["try"]["catch"]["set"]["value"].Value = "[exception].Value";
				e.Params["try"]["catch"]["magix.viewport.show-message"].Value = null;
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			throw new ApplicationException(ip.Get<string>());
		}
	}
}

