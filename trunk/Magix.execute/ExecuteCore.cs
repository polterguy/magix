/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Level3: Controller that encapsulates execution engine
	 */
	[ActiveController]
	public class ExecuteCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "Magix.execute")]
		public void Magix_execute (object sender, ActiveEventArgs e)
		{
			if (e.Params.Count == 0)
			{
				e.Params["Data"].Value = "thomas";
				e.Params["if"].Value = "[../][Data].Value == \"thomas\"";
				e.Params["if"]["call"].Value = "Magix.Core.ShowMessage";
				e.Params["if"]["call"]["params"]["Message"].Value = "Hi Thomas!";
				e.Params["else"].Value = null;
				e.Params["else"]["call"].Value = "Magix.Core.ShowMessage";
				e.Params["else"]["call"]["params"]["Message"].Value = "Hi Stranger!";
			}
			else
			{
				e.Params["_state"].Value = null;
				foreach (Node idx in e.Params)
				{
					string nodeName = idx.Name;

					// Checking to see if it starts with a small letter
					if ("abcdefghijklmnopqrstuvwxyz".IndexOf (nodeName[0]) != -1)
					{
						// This is a keyword
						string eventName = "Magix.execute." + nodeName;
						RaiseEvent (eventName, idx);
					}
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.if")]
		public void Magix_execute_if (object sender, ActiveEventArgs e)
		{
			if (e.Params.Name != "if")
			{
				e.Params.Name = "if";
				e.Params.Value = "[call].Value == \"Magix.Core.ShowMessage\"";
				e.Params["call"].Value = "Magix.Core.ShowMessage";
				e.Params["call"]["params"]["Message"].Value = "Parameters to event";
			}
			else
			{
				if (e.Params.Parent != null)
					e.Params.Parent[e.Params.Parent.Count - 1].Value = null;

				string expr = e.Params.Value as string;
				if (string.IsNullOrEmpty (expr))
					throw new ArgumentException("You cannot have an empty if statement");
				string[] sections = expr.Split (' ');
				string left = sections[0];
				string comparison = sections[1];
				string right = expr.Substring (left.Length + 2 + comparison.Length);
				if (right.IndexOf ("\"") == 0)
				{
					right = right.Substring (1);
					right = right.Substring (0, right.Length - 1);
				}
				if (right.IndexOf("[") == 0)
				{
					right = Expressions.GetExpressionValue (right, e.Params).ToString ();
				}
				bool isTrue = false;
				string valueOfExpr = Expressions.GetExpressionValue (left, e.Params).ToString ();
				switch (comparison)
				{
				case "==":
					isTrue = valueOfExpr == right;
					break;
				case "!=":
					isTrue = valueOfExpr != right;
					break;
				case ">":
					isTrue = valueOfExpr.CompareTo (right) == -1;
					break;
				case "<":
					isTrue = valueOfExpr.CompareTo (right) == 1;
					break;
				case ">=":
					isTrue = valueOfExpr.CompareTo (right) < 1;
					break;
				case "<=":
					isTrue = valueOfExpr.CompareTo (right) > -1;
					break;
				}
				if (e.Params.Parent != null)
					e.Params.Parent[e.Params.Parent.Count - 1].Value = isTrue;
				if (isTrue)
				{
					RaiseEvent ("Magix.execute", e.Params);
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.else-if")]
		public void Magix_execute_else_if (object sender, ActiveEventArgs e)
		{
			if (e.Params.Name != "else-if")
			{
				e.Params.Name = "else-if";
				e.Params.Value = "[call].Value == \"Magix.Core.ShowMessage\"";
				e.Params["call"].Value = "Magix.Core.ShowMessage";
				e.Params["call"]["params"]["Message"].Value = "Parameters to event";
			}
			else
			{
				if (e.Params.Parent != null &&
				    e.Params.Parent[e.Params.Parent.Count - 1] != null &&
				    e.Params.Parent[e.Params.Parent.Count - 1].Get<bool>())
					return;
				string expr = e.Params.Value as string;
				if (string.IsNullOrEmpty (expr))
					throw new ArgumentException("You cannot have an empty else-if statement");
				string[] sections = expr.Split (' ');
				string left = sections[0];
				string comparison = sections[1];
				string right = expr.Substring (left.Length + 2 + comparison.Length);
				if (right.IndexOf ("\"") == 0)
				{
					right = right.Substring (1);
					right = right.Substring (0, right.Length - 1);
				}
				if (right.IndexOf("[") == 0)
				{
					right = Expressions.GetExpressionValue (right, e.Params).ToString ();
				}
				bool isTrue = false;
				string valueOfExpr = left;
				if (valueOfExpr.IndexOf("[") == 0)
				{
					valueOfExpr = Expressions.GetExpressionValue (valueOfExpr, e.Params).ToString ();
				}
				switch (comparison)
				{
				case "==":
					isTrue = valueOfExpr == right;
					break;
				case "!=":
					isTrue = valueOfExpr != right;
					break;
				case ">":
					isTrue = valueOfExpr.CompareTo (right) == -1;
					break;
				case "<":
					isTrue = valueOfExpr.CompareTo (right) == 1;
					break;
				case ">=":
					isTrue = valueOfExpr.CompareTo (right) < 1;
					break;
				case "<=":
					isTrue = valueOfExpr.CompareTo (right) > -1;
					break;
				}
				if (e.Params.Parent != null)
					e.Params.Parent[e.Params.Parent.Count - 1].Value = isTrue;
				if (isTrue)
				{
					RaiseEvent ("Magix.execute", e.Params);
				}
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.else")]
		public void Magix_execute_else (object sender, ActiveEventArgs e)
		{
			if (e.Params.Name != "else")
			{
				e.Params.Name = "else";
				e.Params["call"].Value = "Magix.Core.ShowMessage";
				e.Params["call"]["params"]["Message"].Value = "Parameters to event";
			}
			else
			{
				if (e.Params.Parent != null &&
				    e.Params.Parent[e.Params.Parent.Count - 1] != null &&
				    e.Params.Parent[e.Params.Parent.Count - 1].Get<bool>())
					return;
				if (e.Params.Parent != null)
					e.Params.Parent[e.Params.Parent.Count - 1].Value = null;
				RaiseEvent ("Magix.execute", e.Params);
			}
		}

		/**
		 */
		[ActiveEvent(Name = "Magix.execute.call")]
		public void Magix_execute_call (object sender, ActiveEventArgs e)
		{
			if (e.Params.Name != "call")
			{
				e.Params.Name = "call";
				e.Params.Value = "Magix.Core.ShowMessage";
				e.Params["params"]["Message"].Value = "Parameters to event";
			}
			else
			{
				RaiseEvent (e.Params.Get<string>(), e.Params["params"]);
			}
		}
	}
}

