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
		public void magix_log_append(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["header"].Value = "descriptive header of log item";
				e.Params["description"].Value = "detailed description of log item";
				return;
			}

			if (!e.Params.Contains("header") || e.Params["header"].Get<string>("") == "")
				throw new ArgumentException("no [header] given to log.append");

			if (!e.Params.Contains("body") || e.Params["body"].Get<string>("") == "")
				throw new ArgumentException("no [body] given to log.append");

			string header = e.Params["header"].Get<string>();
			string body   = e.Params["body"].Get<string>();
			DateTime date = DateTime.Now;

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

