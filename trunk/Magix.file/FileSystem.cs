/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
				e.Params["magix.file.load"]["file"].Value = "core-scripts/some-files.txt";
				e.Params["inspect"].Value = @"loads the [file] node into the 
[value] node as text.&nbsp;&nbsp;[file] can be a relative path, to 
fetch a document beneath your web application directory structure, or 
an http or ftp path to a document.&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("file") || e.Params["file"].Get<string>("") == "")
				throw new ArgumentException("You need to define which file to load, as [file] node");

			string file = e.Params["file"].Get<string>();

			if (file.StartsWith("http") || file.StartsWith("ftp"))
			{
				WebRequest request = WebRequest.Create(file) as HttpWebRequest;
				using (WebResponse response = request.GetResponse())
				{
					using (TextReader reader = new StreamReader(response.GetResponseStream()))
					{
						e.Params["value"].Value = reader.ReadToEnd();
					}
				}
			}
			else
			{
				using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(file)))
				{
					e.Params["value"].Value = reader.ReadToEnd();
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
				e.Params["magix.file.save"]["file"].Value = "core-scripts/sample.txt";
				e.Params["magix.file.save"]["value"].Value = @"contents that will replace the contents
in the existing file, alternatively become the 
contents of a new file";
				e.Params["inspect"].Value = @"saves a file from the 
[file] node.&nbsp;&nbsp;the file to save is 
given as the [value] node.&nbsp;&nbsp;
will overwrite an existing file, if any,
otherwise it'll create a new file.&nbsp;&nbsp;if you 
pass in null as [file] Node, or no [file] node
at all, any existing file will be deleted, and no 
new created.&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("file") || e.Params["file"].Get<string>("") == "")
				throw new ArgumentException("You need to define which file to save, as [file] node");

			string file = e.Params["file"].Get<string>();

			if (!e.Params.Contains("value") || e.Params["value"].Get<string>() == null)
			{
				// Deletes an existing file
				File.Delete(HttpContext.Current.Server.MapPath(file));
			}
			else
			{
				string fileContent = e.Params["value"].Get<string>();

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
				throw new ArgumentException("you need to define which directory to delete, as [path] node");

			string dir = e.Params["path"].Get<string>();

			Directory.Delete(HttpContext.Current.Server.MapPath(dir));
		}

		/**
		 * Loads a file from disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.move-directory")]
		public static void magix_file_move_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.move-directory"]["from"].Value = "media/tmp";
				e.Params["magix.file.move-directory"]["to"].Value = "media/some-new-directory";
				e.Params["inspect"].Value = @"moves the given [from] directory, and puts all files into [to].
&nbsp;&nbsp;thread safe";
				return;
			}

			if (!e.Params.Contains("from") || e.Params["from"].Get<string>("") == "")
				throw new ArgumentException("you need to define which directory to copy, as [from] node");

			if (!e.Params.Contains("to") || e.Params["to"].Get<string>("") == "")
				throw new ArgumentException("you need to define which directory to copy to, as [to] node");

			string from = e.Params["from"].Get<string>();
			string to = e.Params["to"].Get<string>();

			Directory.Move(from, to);
		}

		/**
		 */
		[ActiveEvent(Name = "magix.file.list-files")]
		public static void magix_file_list_files(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.list-files"]["directory"].Value = "system42";
				e.Params["magix.file.list-files"]["filter"].Value = "*.hl";
				e.Params["inspect"].Value = @"lists the files in [files] in the given
[path], which must be relative to the app's main path.&nbsp;&nbsp;
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

			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
				e.Params["files"][idxFile.Substring(rootDir.Length)].Value = null;
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
				e.Params["magix.file.list-directories"]["directory"].Value = "system42";
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

			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
				e.Params["directories"][idxFile.Substring(rootDir.Length)].Value = null;
			}
		}
	}
}

