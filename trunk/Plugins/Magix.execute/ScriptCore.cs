/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using Magix.Core;

namespace Magix.admin
{
	/**
	 * script hyper lisp logic
	 */
	public class ScriptCore : ActiveController
	{
		/**
		 * executes hyper lisp script
		 */
		[ActiveEvent(Name = "magix.execute.execute-script")]
		public static void magix_execute_execute_script(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>runs the hyper lisp script given in value of 
[script], putting all child nodes from underneath the [params] node into the [$] collection, 
accessible from inside the script, which again is able to return nodes through the [$] node, 
which will become children of the [params] node after execution</p><p>you can optionally 
supply a [file] parameter, which will be loaded through [magix.file.load], and executed.&nbsp;
&nbsp;if you supply a [file] parameter, you cannot supply a [script] parameter</p><p>both [file] 
and [script], can either be expressions, or constants</p><p>thread safe</p>";
				ip["execute-script"]["script"].Value = @"
_data=>thomas
if=>equals
  lhs=>[_data].Value
  rhs=>thomas
  code
    set=>[@][magix.viewport.show-message][message].Value
      value=>[$][input].Value
    magix.viewport.show-message
    set=>[$][output].Value
      value=>dude's still thomas";
                ip["execute-script"]["input"].Value = "hello world";
				return;
			}

            Node dp = Dp(e.Params);

            if (!ip.Contains("file") && !ip.Contains("script"))
                throw new ArgumentException("[execute-script] needs either a [file] or a [script] parameter");

            if (ip.Contains("file") && ip.Contains("script"))
                throw new ArgumentException("you cannot supply both [file] and [script] to [execute-script]");

            string script = null;
            if (ip.Contains("script"))
                script = Expressions.GetExpressionValue(ip["script"].Get<string>(), dp, ip, false) as string;
            else
            {
                try
                {
                    RaiseActiveEvent(
                        "magix.file.load",
                        e.Params);

                    script = ip["value"].Get<string>();
                }
                finally
                {
                    ip["value"].UnTie();
                }
            }

			Node conversionNode = new Node();
			conversionNode["code"].Value = script;

            RaiseActiveEvent(
				"magix.execute.code-2-node",
				conversionNode);

			ExecuteScript(conversionNode["node"].Clone(), ip);
		}

		/*
		 * helper for above
		 */
		private static void ExecuteScript(Node exe, Node ip)
		{
            if (ip.Contains("params"))
            {
                foreach (Node idx in ip["params"])
                {
                    exe["$"].Add(idx.Clone());
                }
            }

			RaiseActiveEvent(
				"magix.execute", 
				exe);

            if (exe.Contains("$"))
            {
                ip["params"].ReplaceChildren(exe["$"]);
            }
		}
	}
}
