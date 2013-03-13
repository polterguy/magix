/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Net;
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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.load"]["path"].Value = "ExecuteScripts/todo.txt";
				e.Params["inspect"].Value = @"loads the [path] file into the 
[file] node as text.&nbsp;&nbsp;[path] can be a relative path, to 
fetch a document beneath your web application directory structure, or 
an http or ftp path, to a document.&nbsp;&nbsp;thread safe";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params ["_ip"].Value as Node;

			if (!ip.Contains("path") || ip["path"].Get<string>("") == "")
				throw new ArgumentException("You need to define which file to load, as [path] node");

			string file = ip["path"].Get<string>();

			if (file.StartsWith("http") || file.StartsWith("ftp"))
			{
				WebRequest request = WebRequest.Create(file) as HttpWebRequest;
				using (WebResponse response = request.GetResponse())
				{
					using (TextReader reader = new StreamReader(response.GetResponseStream()))
					{
						ip["file"].Value = reader.ReadToEnd();
					}
				}
			}
			else
			{
				using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(file)))
				{
					ip["file"].Value = reader.ReadToEnd();
				}
			}
		}

		/**
		 * Saves a file to disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.save")]
		public static void magix_file_save_file(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.save"]["path"].Value = "ExecuteScripts/sample.txt";
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
new created.&nbsp;&nbsp;thread safe";
				return;
			}

			Node ip = e.Params;
			if (e.Params.Contains("_ip"))
				ip = e.Params["_ip"].Value as Node;

			if (!ip.Contains("path") || ip["path"].Get<string>("") == "")
				throw new ArgumentException("You need to define which file to save, as [path] node");

			string file = ip["path"].Get<string>();

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

