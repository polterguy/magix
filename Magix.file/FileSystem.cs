/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Threading;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Contains logic for loading, saving and re-organizing files
	 */
	public class FileSystem : ActiveController
	{
		/**
		 * Loads a file from disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.load")]
		public static void magix_file_load(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.load"].Value = "ExecuteScripts/TODO.txt";
				e.Params["inspect"].Value = @"loads a file into the 
[file] node as text.&nbsp;&nbsp;the file to load is 
given as value of the file node";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			string file = ip.Get<string>();
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException("You need to define which file to load, as Value of the load Node");

			using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(file)))
			{
				ip["value"].Value = reader.ReadToEnd();
			}
		}

		/**
		 * Saves a file to disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.save")]
		public static void magix_file_save_file(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.save"].Value = "ExecuteScripts/TODO.txt";
				e.Params["magix.file.save"]["file"].Value = @"contents that will replace the contents
in the existing file, alternatively become the 
contents of a new file";
				e.Params["inspect"].Value = @"saves a file from the 
[file] node.&nbsp;&nbsp;the file to save is 
given as value of the save-file node.&nbsp;&nbsp;
will overwrite an existing file, if any,
otherwise it'll create a new file.&nbsp;&nbsp;if you 
pass in null as [file] Node, or no [file] node
at all, any existing file will be deleted, and no 
new created";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			string file = ip.Get<string>();
			if (string.IsNullOrEmpty(file))
				throw new ArgumentException("You need to define which file to save, as Value of the save Node");

			if (!ip.Contains("file") || ip["file"].Get<string>("") == "")
			{
				// Deletes an existing file
				File.Delete(HttpContext.Current.Server.MapPath(file));
			}
			else
			{
				string fileContent = ip["file"].Get<string>();

				using (TextWriter writer = 
				       new StreamWriter(File.OpenWrite(HttpContext.Current.Server.MapPath(file))))
				{
					writer.Write(fileContent);
				}
			}
		}
	}
}

