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
				e.Params["magix.file.load"]["path"].Value = "core-scripts/some-files.txt";
				e.Params["inspect"].Value = @"loads the [path] file into the 
[file] node as text.&nbsp;&nbsp;[path] can be a relative path, to 
fetch a document beneath your web application directory structure, or 
an http or ftp path, to a document.&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("path") || e.Params["path"].Get<string>("") == "")
				throw new ArgumentException("You need to define which file to load, as [path] node");

			string file = e.Params["path"].Get<string>();

			if (file.StartsWith("http") || file.StartsWith("ftp"))
			{
				WebRequest request = WebRequest.Create(file) as HttpWebRequest;
				using (WebResponse response = request.GetResponse())
				{
					using (TextReader reader = new StreamReader(response.GetResponseStream()))
					{
						e.Params["file"].Value = reader.ReadToEnd();
					}
				}
			}
			else
			{
				using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(file)))
				{
					e.Params["file"].Value = reader.ReadToEnd();
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
				e.Params["magix.file.save"]["path"].Value = "core-scripts/sample.txt";
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

			if (!e.Params.Contains("path") || e.Params["path"].Get<string>("") == "")
				throw new ArgumentException("You need to define which file to save, as [path] node");

			string file = e.Params["path"].Get<string>();

			if (!e.Params.Contains("file") || e.Params["file"].Get<string>() == null)
			{
				// Deletes an existing file
				File.Delete(HttpContext.Current.Server.MapPath(file));
			}
			else
			{
				string fileContent = e.Params["file"].Get<string>();

				using (TextWriter writer = 
				       new StreamWriter(File.OpenWrite(HttpContext.Current.Server.MapPath(file))))
				{
					writer.Write(fileContent);
				}
			}
		}

		/**
		 * Loads a file from disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.create-directory")]
		public static void magix_file_create_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.create-directory"]["path"].Value = "media/tmp";
				e.Params["inspect"].Value = @"creates the [path] directory.
&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("path") || e.Params["path"].Get<string>("") == "")
				throw new ArgumentException("You need to define which directory to create, as [path] node");

			string dir = e.Params["path"].Get<string>();

			Directory.CreateDirectory(HttpContext.Current.Server.MapPath(dir));
		}

		/**
		 * Loads a file from disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.delete-directory")]
		public static void magix_file_delete_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.delete-directory"]["path"].Value = "media/tmp";
				e.Params["inspect"].Value = @"deletes the [path] directory from disc.
&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("path") || e.Params["path"].Get<string>("") == "")
				throw new ArgumentException("You need to define which directory to delete, as [path] node");

			string dir = e.Params["path"].Get<string>();

			Directory.Delete(HttpContext.Current.Server.MapPath(dir));
		}

		/**
		 */
		[ActiveEvent(Name = "magix.file.list-files")]
		public static void magix_file_list_files(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.list-files"]["directory"].Value = "zigano";
				e.Params["magix.file.list-files"]["filter"].Value = "*.hl";
				e.Params["inspect"].Value = @"lists the files in [files] in the given
[directory], which must be relative to the app's main path.&nbsp;&nbsp;
use [filter] as a search pattern.&nbsp;&nbsp;thread safe";
				return;
			}

			string dir = e.Params.Contains("directory") ? e.Params["directory"].Get<string>("").TrimStart('/') : "~/";
			string filter = e.Params.Contains("filter") ? e.Params["filter"].Get<string>() : null;

			string[] files = null;

			if (string.IsNullOrEmpty(filter))
				files = Directory.GetFiles(HttpContext.Current.Server.MapPath(dir));
			else
				files = Directory.GetFiles(HttpContext.Current.Server.MapPath(dir), filter);

			int idxNo = 0;
			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
				e.Params["files"]["f_" + idxNo++].Value = idxFile.Substring(rootDir.Length);
			}
		}

		/**
		 */
		[ActiveEvent(Name = "magix.file.list-directories")]
		public static void magix_file_list_directories(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.list-directories"]["directory"].Value = "zigano";
				e.Params["magix.file.list-directories"]["filter"].Value = "*s*";
				e.Params["inspect"].Value = @"lists the directories in [directories] in the given
[directory], which must be relative to the app's main path.&nbsp;&nbsp;
use [filter] as a search pattern.&nbsp;&nbsp;thread safe";
				return;
			}

			string dir = e.Params.Contains("directory") ? e.Params["directory"].Get<string>("").TrimStart('/') : "~/";
			string filter = e.Params.Contains("filter") ? e.Params["filter"].Get<string>() : null;

			string[] files = null;

			if (string.IsNullOrEmpty(filter))
				files = Directory.GetDirectories(HttpContext.Current.Server.MapPath(dir));
			else
				files = Directory.GetDirectories(HttpContext.Current.Server.MapPath(dir), filter);

			int idxNo = 0;
			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
				e.Params["directories"]["f_" + idxNo++].Value = idxFile.Substring(rootDir.Length);
			}
		}
	}
}

