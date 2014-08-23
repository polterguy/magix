/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
	internal sealed class DebugCore : ActiveController
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
                    "[magix.execute.debug-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.debug-sample]");
                return;
			}

            Node stack = ip.RootNode();
            if (!string.IsNullOrEmpty(ip.Get<string>()))
            {
                stack = Expressions.GetExpressionValue<Node>(ip.Get<string>(), Dp(e.Params), ip, false);
                if (stack == null)
                    throw new HyperlispExecutionErrorException("tried to debug a non-existing node tree in [debug], expression was; '" + ip.Get<string>() +"'");
            }

			Node confirmNode = new Node();
			confirmNode["code"].AddRange(stack.Clone());
			confirmNode["message"].Value = "stackdump of tree from debug instruction";
			confirmNode["closable-only"].Value = true;
			RaiseActiveEvent(
				"magix.viewport.confirm",
				confirmNode);
		}
	}
}

