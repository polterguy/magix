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
				e.Params["inspect"].Value = @"<p>runs the hyper lisp file given in value of [execute-file], 
putting all other child nodes into the [$] collection, accessible from inside the file, which again is able 
to return nodes through the [$] node, which will become children of the [execute-file] node after execution
</p><p>thread safe</p>";
                e.Params["execute-file"].Value = "core-scripts/doesnt-exist.hl";
                e.Params["execute-file"]["parameter"].Value = @"some parameter";
                return;
			}

            Node ip = Ip(e.Params);
			string file = ip.Get<string>();
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("[execute-file] needs a file to execute as the value of the [execute-file] node");

			Node loadFileNode = new Node();
			loadFileNode.Value = file;
			RaiseActiveEvent(
				"magix.file.load",
				loadFileNode);

			string txt = loadFileNode["value"].Get<string>();

			Node executeFileNode = new Node();
			executeFileNode["code"].Value = txt;
			RaiseActiveEvent(
				"magix.code.code-2-node",
				executeFileNode);
			ExecuteScript(executeFileNode["json"].Get<Node>(), ip);
		}
		
		/**
		 * executes script
		 */
		[ActiveEvent(Name = "magix.execute.execute-script")]
		public static void magix_execute_execute_script(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>runs the hyper lisp script given in value of 
[execute-script], putting all other child nodes into the [$] collection, accessible from inside 
the script, which again is able to return nodes through the [$] node, which will become children 
of the [execute-script] node after execution</p><p>thread safe</p>";
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

            Node ip = Ip(e.Params);
			string script = ip.Get<string>();

			Node conversionNode = new Node();
			conversionNode["code"].Value = script;
			RaiseActiveEvent(
				"magix.code.code-2-node",
				conversionNode);
			ExecuteScript(conversionNode["json"].Get<Node>(), ip);
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
			ip.ReplaceChildren(exe["$"]);
		}
	}
}
