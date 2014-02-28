/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"show the entire stack of hyper lisp tree 
in a modal message box.&nbsp;&nbsp;not thread safe";
				e.Params["debug"].Value = null;
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.add] directly, except for inspect purposes");

			Node ip = e.Params ["_ip"].Value as Node;

			Node tmp = new Node();

			tmp["code"].AddRange(ip.RootNode().Clone());
			tmp["code"]["_state"].UnTie();
			tmp["message"].Value = "stackdump of tree from debug instruction";
			tmp["closable-only"].Value = true;

			RaiseActiveEvent(
				"magix.viewport.confirm",
				tmp);
		}
	}
}

