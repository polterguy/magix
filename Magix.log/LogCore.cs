/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.log
{
	/**
	 * contains the logging functionality of magix
	 */
	public class LogCore : ActiveController
	{
		/**
		 * will log the given header and body
		 */
		[ActiveEvent(Name = "magix.log.append")]
		public static void magix_log_append(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.log.append"].Value = null;
				e.Params["inspect"].Value = @"creates a new item in your log.&nbsp;&nbsp;
you need to submit both a [header] and a [body].&nbsp;&nbsp;
parameters added beneath [body] and/or [header] will be formatted into string.&nbsp;&nbsp;thread safe";
				e.Params["header"].Value = "descriptive header of log item";
				e.Params["body"].Value = "detailed description of log item";
				return;
			}

			if (!e.Params.Contains("header") || e.Params["header"].Get<string>("") == "")
				throw new ArgumentException("no [header] given to log.append");

			if (!e.Params.Contains("body") || e.Params["body"].Get<string>("") == "")
				throw new ArgumentException("no [body] given to log.append");

			string header = e.Params["header"].Get<string>();
			string body   = e.Params["body"].Get<string>();
			DateTime date = DateTime.Now;

			if (e.Params["body"].Count > 0)
			{
				// contains parameters
				object[] arrs = new object[e.Params["body"].Count];

				int idxNo = 0;
				foreach (Node idx in e.Params["body"])
				{
					arrs[idxNo++] = idx.Value;
				}
				body = string.Format(body, arrs);
			}

			if (e.Params["header"].Count > 0)
			{
				// contains parameters
				object[] arrs = new object[e.Params["header"].Count];

				int idxNo = 0;
				foreach (Node idx in e.Params["header"])
				{
					arrs[idxNo++] = idx.Value;
				}
				header = string.Format(header, arrs);
			}

			Node node = new Node();

			node["object"]["type"].Value = "magix.log.item";
			node["object"]["header"].Value = header;
			node["object"]["body"].Value   = body;
			node["object"]["date"].Value   = date;

			RaiseActiveEvent(
				"magix.data.save",
				node);
		}
	}
}

