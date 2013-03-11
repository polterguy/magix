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
	 * Contains active events for handling "stop" keyword
	 */
	public class StopCore : ActiveController
	{
		/**
		 * stops the execution of the current block of code
		 */
		[ActiveEvent(Name = "magix.execute.stop")]
		public static void magix_execute_stop(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"stops the execution of the current level
of code nodes in the tree";
				e.Params["_data"].Value = 1;
				e.Params["while"].Value = "[_data]==1";
				e.Params["while"]["stop"].Value = null;
				return;
			}

			e.Params["_state"].Value = "stop";
		}
	}
}
