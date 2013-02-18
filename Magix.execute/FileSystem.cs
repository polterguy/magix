/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
	 */
	public class FileSystem : ActiveController
	{
		/**
		 * Loads a file from disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.execute.load-file")]
		public static void magix_execute_load_file (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["load-file"].Value = "ExecuteScripts/TODO.txt";
				e.Params["inspect"].Value = @"Loads a file into the 
""file"" node. The file to load is 
given as Value of the load-file Node.";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			string file = ip.Get<string>();
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException("You need to define which file to load, as Value of the load-file Node");

			using (TextReader reader = File.OpenText (HttpContext.Current.Server.MapPath (file)))
			{
				ip["value"].Value = reader.ReadToEnd ();
			}
		}

		/**
		 * Saves a file to disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.execute.save-file")]
		public static void magix_execute_save_file (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["load-file"].Value = "ExecuteScripts/TODO.txt";
				e.Params["load-file"]["file"].Value = @"Contens that will replace the contents
in the existing file, if any exist.";
				e.Params["inspect"].Value = @"Saves a file from the 
""file"" node. The file to save is 
given as Value of the save-file Node.
Will overwrite an existing file, if any,
otherwise it'll create a new file. If you 
pass in null as ""file"" Node, or no file Node
at all, any existing file will simply be deleted.";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains ("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			string file = ip.Get<string>();
			if (string.IsNullOrEmpty (file))
				throw new ArgumentException("You need to define which file to load, as Value of the load-file Node");

			if (!ip.Contains ("file") || ip["file"].Get<string>("") == "")
			{
				// Deletes an existing file
				File.Delete (HttpContext.Current.Server.MapPath (file));
			}
			else
			{
				string fileContent = ip["file"].Get<string>();

				using (TextWriter writer = new StreamWriter(File.OpenWrite (HttpContext.Current.Server.MapPath (file))))
				{
					writer.Write(fileContent);
				}
			}
		}
	}
}

