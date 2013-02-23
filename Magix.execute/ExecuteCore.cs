/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Controller logic for the magix.execute programming language in magix, which
	 * facilitates for creating logic in nodes, such that you can execute logic
	 * based upon your nodes dynamically
	 */
	public class ExecuteCore : ActiveController
	{
		/**
		 * Executes all the children nodes starting with
		 * a lower case alpha character,
		 * expecting them to be callable keywords, directly embedded into
		 * the "magix.execute" namespace, such that they become extensions
		 * of the execution engine itself. Will internally keep a list
		 * pointer to where the code/instruction-pointer is, and where the
		 * data-pointer is, which is relevant for most other execution statements.
		 * Will ignore all node with Name not starting with a lower case alpha character.
		 * All child nodes with a name starting with a lower case letter,
		 * will be raise as "magix.execute" keywords, meaning e.g. "if"
		 * will become the active event "magix.executor.if", meaning you
		 * can extend the programming language in Magix itself by adding up 
		 * new keywords. All active events in the "magix.execute"
		 * namespace, can be called directly as keywords in magix.execute.
		 * And in fact, most active events in the magix.execute namespace, 
		 * such as "if", cannot be called directly, but must be called
		 * from within a "magix.execute" event code block
		 */
		[ActiveEvent(Name = "magix.execute")]
		public static void magix_execute (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect") &&
			    e.Params["inspect"].Get<string>("") == "")
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"Executes the children nodes as if they
were a block of execute statements, or code, within 
the ""magix.execute"" namespace. All magix active 
events which starts with the ""magix.execute."" 
namespace, can be embedded inside a ""magix.execute""
scope, as keywords, e.g. the ""if"" keyword, will
map towards the ""magix.execute.if"" active event.
This means that you can use all ""magix.execute""
active events as code instructions, which you can
execute by raising this active event, by using the
short version ""if"" instead of ""magix.execute.if"".
If you supply a Value for your ""execute"" node, the
event will be executed, with the code returned from
the Node-List expression from your Value, meaning
the Value effectively becomes a 'goto keyword'.";
				e.Params["Data"]["Value"].Value = "thomas";
				e.Params["if"].Value = "[Data][Value].Value==thomas";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "Hi Thomas!";
				e.Params["else"]["magix.viewport.show-message"].Value = null;
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "Hi Stranger!";
				return;
			}
			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains ("_dp"))
				dp = e.Params["_dp"].Value as Node;

			// Checking to see if we've got a "context"
			if ((ip.Name == "execute" || ip.Name == "magix.execute") && 
				!string.IsNullOrEmpty(ip.Get<string>()))
			{
				ip = Expressions.GetExpressionValue (ip.Get<string>(), dp, ip) as Node;
				if (ip == null)
					throw new ArgumentException(
						"You can only supply a context pointing to an existing Node hierarchy");
			}

			ip["_state"].Value = null;

			foreach (Node idx in ip)
			{
				string nodeName = idx.Name;

				// Checking to see if it starts with a small letter
				if ("abcdefghijklmnopqrstuvwxyz".IndexOf (nodeName[0]) != -1)
				{
					// Checking to see if it's a reference to an active event
					if (nodeName.Contains ("."))
					{
						Node tmp = new Node();

						// "Instruction" pointer, code Node
						tmp["_ip"].Value = idx;

						// Data Pointer, pointer to data
						tmp["_dp"].Value = dp;

						RaiseEvent ("magix.execute.raise", tmp);
					}
					else
					{
						// This is a keyword
						string eventName = "magix.execute." + nodeName;

						if (nodeName == "execute")
							eventName = "magix.execute";

						Node tmp = new Node();

						// "Instruction" pointer, code Node
						tmp["_ip"].Value = idx;

						// Data Pointer, pointer to data
						tmp["_dp"].Value = dp;

						RaiseEvent (eventName, tmp);
					}
				}
			}
			ip.Remove (ip["_state"]);
		}
	}
}

