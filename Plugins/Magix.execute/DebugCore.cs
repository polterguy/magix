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
	/**
	 * debug logic
	 */
	public class DebugCore : ActiveController
	{
		/**
		 * debug hyper lisp keyword
		 */
		[ActiveEvent(Name = "magix.execute.debug")]
		public void magix_execute_debug(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
				ip["inspect"].Value = @"<p>shows the entire stack of hyper lisp code 
in a modal message box.&nbsp;&nbsp;alternatively, you can submit an expression, pointing 
to a node list, to show only a subsection of the tree</p><p>not thread safe</p>";
				ip["debug"].Value = null;
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.add] directly, except for inspect purposes");

			Node dp = e.Params ["_dp"].Value as Node;


            Node stack = null;

            if (!string.IsNullOrEmpty(ip.Get<string>()))
            {
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

