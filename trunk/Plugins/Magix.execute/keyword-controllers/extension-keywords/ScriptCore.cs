/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * script hyperlisp logic
	 */
	public class ScriptCore : ActiveController
	{
		/*
		 * executes hyperlisp script
		 */
		[ActiveEvent(Name = "magix.execute.execute-script")]
		public static void magix_execute_execute_script(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.execute-script-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.execute-script-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.Contains("file") && !ip.Contains("script"))
                throw new ArgumentException("[execute-script] needs either a [file] or a [script] parameter");

            if (ip.Contains("file") && ip.Contains("script"))
                throw new ArgumentException("you cannot supply both [file] and [script] to [execute-script]");

            string script = null;
            if (ip.Contains("script"))
                script = Expressions.GetExpressionValue<string>(ip["script"].Get<string>(), dp, ip, false);
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

            conversionNode["node"]["inspect"].UnTie();

            Node exe = conversionNode["node"].Clone();
            exe["inspect"].UnTie();
			ExecuteScript(exe, ip);
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
                ip["params"].Clear();
                ip["params"].AddRange(exe["$"]);
            }
		}
	}
}
