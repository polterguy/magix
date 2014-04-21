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
		 * executes the given hyper lisp file
		 */
		[ActiveEvent(Name = "magix.execute.execute-file")]
		public static void magix_execute_execute_file(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"runs the hyper lisp file given in value of [execute-file], 
putting all other child nodes into the [$] collection, accessible from inside the file, 
which again is able to return nodes through the [$] node, which will become children of the 
[execute-file] node after execution.&nbsp;&nbsp;
thread safe";
				e.Params["execute-file"].Value = "core-scripts/some-script.hl";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip") && e.Params["_ip"].Value is Node)
				ip = e.Params["_ip"].Value as Node;

			if (ip.Value == null)
				throw new ArgumentException("execute-file needs value object pointing to existing hyper lisp file");

			string file = ip.Get<string>();

			Node fn = new Node();

			fn.Value = file;

			RaiseActiveEvent(
				"magix.file.load",
				fn);

			string txt = fn["value"].Get<string>();

			Node tmp = new Node();
			tmp["code"].Value = txt;

			RaiseActiveEvent(
				"magix.code.code-2-node",
				tmp);
			
			ExecuteScript(tmp["json"].Get<Node>(), ip);
		}
		
		/**
		 * executes script
		 */
		[ActiveEvent(Name = "magix.execute.execute-script")]
		public static void magix_execute_execute_script(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"runs the hyper lisp script given in value of [execute-script], 
putting all other child nodes into the [$] collection, accessible from inside the script, 
which again is able to return nodes through the [$] node, which will become children of the 
[execute-script] node after execution.&nbsp;&nbsp;
thread safe";
				e.Params["execute-script"].Value = @"
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
                e.Params["execute-script"]["input"].Value = "hello world";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip") && e.Params["_ip"].Value is Node)
				ip = e.Params["_ip"].Value as Node;

			if (ip.Value == null)
				throw new ArgumentException("need script value for [execute-script]");

			string txt = ip.Get<string>();

			Node tmp = new Node();
			tmp["code"].Value = txt;

			RaiseActiveEvent(
				"magix.code.code-2-node",
				tmp);

			ExecuteScript(tmp["json"].Get<Node>(), ip);
		}

		/*
		 * helper for above
		 */
		private static void ExecuteScript(Node exe, Node ip)
		{
			foreach (Node idx in ip)
			{
				exe["$"].Add(idx.Clone());
			}

			RaiseActiveEvent(
				"magix.execute", 
				exe);

			if (exe.Contains("$"))
			{
				ip.ReplaceChildren(exe["$"]);
			}
		}
	}
}
