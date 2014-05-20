/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
				e.Params["code"].Value = "hyperlisp code";
				e.Params["error"].Value = false;
				return;
			}

            if (!Ip(e.Params).Contains("header") || Ip(e.Params)["header"].Get<string>("") == "")
				throw new ArgumentException("no [header] given to log.append");

            if (!Ip(e.Params).Contains("body") || Ip(e.Params)["body"].Get<string>("") == "")
				throw new ArgumentException("no [body] given to log.append");

            string header = Ip(e.Params)["header"].Get<string>();
            string body = Ip(e.Params)["body"].Get<string>();

			DateTime date = DateTime.Now;

            if (Ip(e.Params)["body"].Count > 0)
			{
				// contains parameters
                object[] arrs = new object[Ip(e.Params)["body"].Count];

				int idxNo = 0;
                foreach (Node idx in Ip(e.Params)["body"])
				{
					arrs[idxNo++] = idx.Value;
				}
				body = string.Format(body, arrs);
			}

            if (Ip(e.Params)["header"].Count > 0)
			{
				// contains parameters
                object[] arrs = new object[Ip(e.Params)["header"].Count];

				int idxNo = 0;
                foreach (Node idx in Ip(e.Params)["header"])
				{
					arrs[idxNo++] = idx.Value;
				}
				header = string.Format(header, arrs);
			}

			Node node = new Node();

			node["value"]["type"].Value = "magix.log.item";
			node["value"]["header"].Value = header;
			node["value"]["body"].Value   = body;
			node["value"]["date"].Value   = date;

            if (Ip(e.Params).Contains("error"))
                node["value"]["error"].Value = Ip(e.Params)["error"].Value;

            if (Ip(e.Params).Contains("code"))
			{
                node["value"]["code"].Clear();
                node["value"]["code"].AddRange(Ip(e.Params)["code"].Clone());
			}

			RaiseActiveEvent(
				"magix.data.save",
				node);
		}
	}
}

