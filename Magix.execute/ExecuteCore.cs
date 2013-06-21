/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
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
		private class SecurityHyperLispException : Exception
		{
			public SecurityHyperLispException(string err)
				: base(err)
			{ }
		}

		private class HyperLispExecutionEngineException : Exception
		{
			public HyperLispExecutionEngineException(string err)
				: base(err)
			{ }
		}

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
		public static void magix_execute(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"executes the child nodes as if they
were a block of execute statements, or code, within 
the magix.execute namespace.&nbsp;&nbsp;all magix active 
events starting with magix.execute 
namespace, can be embedded inside a magix.execute
scope as keywords, the [if] keyword for instance, will
map towards the magix.execute.if active event.&nbsp;&nbsp;
this means that you can use all magix.execute
active events as code instructions, which you can
execute by raising this active event, by using its
short version.&nbsp;&nbsp;
if you supply a value for your magix.execute node, the
execution pointer becomes that of the expression of the
value, effectively 'goto'.&nbsp;&nbsp;all nodes in child
collection of magix.execute will be treated as keywords, 
if they only contain these characters; 
abcdefghijklmnopqrstuvwxyz-.@£#$¤%&!?+:*.&nbsp;&nbsp;therefor, 
a useful convention is to, either prefix your data parts 
with an underscore, '_', or a Capital letter, or add numbers 
to the node's name.&nbsp;&nbsp;
if you add a [whitelist] child node, together with a context value, 
the node names underneath the 
whitelist node, will become a list of exclusively allowed keywords and
active events to run, and any other keywords or events, will be perceived
as a security breach, and logged.&nbsp;&nbsp;thread safe";
				e.Params["_data"]["value"].Value = "thomas";
				e.Params["if"].Value = "[_data][value].Value==thomas";
				e.Params["if"]["magix.viewport.show-message"].Value = null;
				e.Params["if"]["magix.viewport.show-message"]["message"].Value = "hi thomas";
				e.Params["else"]["magix.viewport.show-message"].Value = null;
				e.Params["else"]["magix.viewport.show-message"]["message"].Value = "hi stranger";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			Node dp = e.Params;
			if (e.Params.Contains("_dp"))
				dp = e.Params["_dp"].Value as Node;

			Node whiteList = null;
			if (e.Params.Contains("_whitelist"))
				whiteList = e.Params["_whitelist"].Value as Node;

			// Checking to see if we've got a "context"
			if ((ip.Name == "execute" || ip.Name == "magix.execute") && 
				!string.IsNullOrEmpty(ip.Get<string>()))
			{
				if (ip.Contains("whitelist"))
				{
					if (whiteList != null)
					{
						// logging security breach
						Node log = new Node();

						log["header"].Value = "security breach in [magix.execute]";
						log["body"].Value = "attempted to load another whitelist into the system, even though an existing exists.  security breach!!";
						log["error"].Value = true;

						ip["_state"].UnTie();

						while (ip.Parent != null)
						{
							ip.Parent["_state"].UnTie();
							ip = ip.Parent;
						}

						ip["_state"].UnTie();
						while (ip.Parent != null)
						{
							ip.Parent["_state"].UnTie();
							ip = ip.Parent;
						}

						ip.Name += " ( ** execution engine error ** )";

						log["code"].ReplaceChildren(ip.RootNode().Clone());

						RaiseActiveEvent(
							"magix.log.append", 
							log);

						throw new ExecuteCore.SecurityHyperLispException("attempted to inject another whitelist of hyper lisp keywords unto an existing whitelist.&nbsp;&nbsp;security breach!!");
					}

					whiteList = ip["whitelist"].Clone();
				}

				ip = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as Node;
				if (ip == null)
					throw new ArgumentException(
						"you can only supply a context pointing to an existing node hierarchy");
			}

			// Temporary variable used internally by some functions, such as "if" and "else-if"
			ip["_state"].Value = null;

			for (int idxNo = 0; idxNo < ip.Count; idxNo++)
			{
				Node idx = ip[idxNo];
				string nodeName = idx.Name;

				// Checking to see if it starts with a small letter
				if ("abcdefghijklmnopqrstuvwxyz".IndexOf(nodeName[0]) != -1)
				{
					// Checking to see if it's a reference to an active event
					if (nodeName.Contains("."))
					{
						Node tmp = new Node();

						// "Instruction" pointer, code Node
						tmp["_ip"].Value = idx;

						// Data Pointer, pointer to data
						tmp["_dp"].Value = dp;

						if (whiteList != null)
						{
							if (!whiteList.Contains(nodeName))
							{
								// logging security breach
								Node log = new Node();

								log["header"].Value = "security breach in [magix.execute]";
								log["body"].Value = "attempted to raise an active event not in the whitelist of the execution context, name of active event was was [" + nodeName + "].  security breach!!";
								log["error"].Value = true;

								ip["_state"].UnTie();

								while (ip.Parent != null)
								{
									ip.Parent["_state"].UnTie();
									ip = ip.Parent;
								}

								ip["_state"].UnTie();
								while (ip.Parent != null)
								{
									ip.Parent["_state"].UnTie();
									ip = ip.Parent;
								}

								idx.Name += " ( ** execution engine error ** )";

								log["code"].ReplaceChildren(ip.RootNode().Clone());

								RaiseActiveEvent(
									"magix.log.append", 
									log);

								throw new ExecuteCore.SecurityHyperLispException("attempted to raise an active event not in the whitelist of the execution context, name of active event was was [" + nodeName + "].  security breach!!");
							}
							tmp["_whitelist"].Value = whiteList;
						}

						// to support stop keywords, and similar constructs
						try
						{
							RaiseActiveEvent(
								"magix.execute.raise", 
								tmp);
						}
						catch(Exception err)
						{
							ip["_state"].UnTie();

							while (err.InnerException != null)
								err = err.InnerException;

							if (err is StopCore.HyperLispStopException)
							{
								if (e.Params.Contains("_ip"))
								{
									throw; // keep on rethrowing till we meet somewhere magix.execute was explicitly called ...
								}
								// outer execution, returning as if nothing happened ...
								return;
							}
							else if (err is ExecuteCore.HyperLispExecutionEngineException)
							{
								throw;
							}
							else if (err is ExecuteCore.SecurityHyperLispException)
							{
								throw;
							}
							else
							{
								idx.Name += " ( ** execution engine error ** )";

								// logging security breach
								Node log = new Node();

								log["header"].Value = "execution error [magix.execute]";
								log["body"].Value = "execution engine broke down at [" + nodeName + "], inner exception was; '" + err.Message + "'";
								log["error"].Value = true;

								ip["_state"].UnTie();

								while (ip.Parent != null)
								{
									ip.Parent["_state"].UnTie();
									ip = ip.Parent;
								}

								log["code"].ReplaceChildren(ip.RootNode().Clone());

								RaiseActiveEvent(
									"magix.log.append", 
									log);

								throw new ExecuteCore.HyperLispExecutionEngineException("execution engine exception, error was; '" + err.Message + "'");
							}
						}
					}
					else
					{
						bool isKeyword = true;
						foreach (char idxC in nodeName)
						{
							if ("abcdefghijklmnopqrstuvwxyz-@£#$¤%&!?+*:.".IndexOf(idxC) == -1)
							{
								isKeyword = false;
								break;
							}
						}

						if (!isKeyword)
							continue;

						// This is a keyword
						string eventName = "magix.execute." + nodeName;

						if (nodeName == "execute")
							eventName = "magix.execute";

						Node tmp = new Node();

						// "Instruction" pointer, code Node
						tmp["_ip"].Value = idx;

						// Data Pointer, pointer to data
						tmp["_dp"].Value = dp;

						if (whiteList != null)
						{
							if (!whiteList.Contains(nodeName))
							{
								// logging security breach
								Node log = new Node();

								log["header"].Value = "security breach in [magix.execute]";
								log["body"].Value = "attempted to use a keyword not in the whitelist of the execution context, keyword was [" + nodeName + "].  security breach!!";
								log["error"].Value = true;

								ip["_state"].UnTie();

								while (ip.Parent != null)
								{
									ip.Parent["_state"].UnTie();
									ip = ip.Parent;
								}

								idx.Name += " ( ** execution engine error ** )";

								log["code"].ReplaceChildren(ip.RootNode().Clone());

								RaiseActiveEvent(
									"magix.log.append", 
									log);

								throw new ExecuteCore.SecurityHyperLispException("attempted to use a keyword not in the whitelist of the execution context, keyword was [" + nodeName + "].  security breach!!");
							}
							tmp["_whitelist"].Value = whiteList;
						}


						// to support stop keywords, and similar constructs
						try
						{
							RaiseActiveEvent(
								eventName, 
								tmp);
						}
						catch(Exception err)
						{
							ip["_state"].UnTie();

							while (err.InnerException != null)
								err = err.InnerException;

							if (err is StopCore.HyperLispStopException)
							{
								if (e.Params.Contains("_ip"))
								{
									throw; // keep on rethrowing till we meet somewhere magix.execute was explicitly called ...
								}
								// outer execution, returning as if nothing happened ...
								return;
							}
							else if (err is ExecuteCore.HyperLispExecutionEngineException)
							{
								throw;
							}
							else if (err is ExecuteCore.SecurityHyperLispException)
							{
								throw;
							}
							else
							{
								idx.Name += " ( ** execution engine error ** )";

								// logging security breach
								Node log = new Node();

								log["header"].Value = "execution error [magix.execute]";
								log["body"].Value = "execution engine broke down at [" + nodeName + "], inner exception was; '" + err.Message + "'";
								log["error"].Value = true;

								ip["_state"].UnTie();

								while (ip.Parent != null)
								{
									ip.Parent["_state"].UnTie();
									ip = ip.Parent;
								}

								log["code"].ReplaceChildren(ip.RootNode().Clone());

								RaiseActiveEvent(
									"magix.log.append", 
									log);

								throw new ExecuteCore.HyperLispExecutionEngineException("execution engine exception, error was; '" + err.Message + "'");
							}
						}
					}
				}
			}
			ip["_state"].UnTie();
		}
	}
}

