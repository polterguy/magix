/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * debug logic
	 */
	public class DebugCore : ActiveController
	{
		/*
		 * debug hyperlisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.debug")]
		public void magix_execute_debug(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.debug-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.debug-sample]");
                return;
			}

            Node stack = null;
            if (!string.IsNullOrEmpty(ip.Get<string>()))
            {
                Node dp = Dp(e.Params);
                stack = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
                if (stack == null)
                    throw new ArgumentException("tried to debug a non-existing node tree in [debug]");
            }
            else
                stack = ip.RootNode();

			Node tmp = new Node();
			tmp["code"].AddRange(stack.Clone());
			tmp["code"]["_state"].UnTie();
			tmp["message"].Value = "stackdump of tree from debug instruction";
			tmp["closable-only"].Value = true;

			RaiseActiveEvent(
				"magix.viewport.confirm",
				tmp);
		}
	}
}

