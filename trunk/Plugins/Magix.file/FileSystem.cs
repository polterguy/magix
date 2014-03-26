/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
	 * contains logic for loading, saving and re-organizing files
	 */
	public class FileSystem : ActiveController
	{
		/**
		 * loads a file from disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.load")]
		public static void magix_file_load(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.load"].Value = "core-scripts/some-files.txt";
				e.Params["inspect"].Value = @"loads the file from value of [magix.file.load] into the 
[value] node as text.&nbsp;&nbsp;file can be a relative path, to 
fetch a document beneath your web application directory structure, or 
an http or ftp path to a document.&nbsp;&nbsp;thread safe";
				return;
			}

            string file = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(file))
				throw new ArgumentException("You need to define which file to load, as value of [magix.file.load]");

			if (file.StartsWith("http") || file.StartsWith("ftp"))
			{
				WebRequest request = WebRequest.Create(file) as HttpWebRequest;
				using (WebResponse response = request.GetResponse())
				{
					using (TextReader reader = new StreamReader(response.GetResponseStream()))
					{
                        Ip(e.Params)["value"].Value = reader.ReadToEnd();
					}
				}
			}
			else
			{
				using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(file)))
				{
                    Ip(e.Params)["value"].Value = reader.ReadToEnd();
				}
			}
		}

		/**
		 * saves a file to disc, relatively from the root of the web application
		 */
		[ActiveEvent(Name = "magix.file.save")]
		[ActiveEvent(Name = "magix.file.delete")]
		public static void magix_file_save(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.save"].Value = "core-scripts/sample.txt";
				e.Params["magix.file.save"]["value"].Value = @"contents that will replace the contents
in the existing file, alternatively become the 
contents of a new file";
				e.Params["inspect"].Value = @"saves a file defined by the 
value of [magix.file.save].&nbsp;&nbsp;the file to save is 
given as the [value] node.&nbsp;&nbsp;
will overwrite an existing file, if any,
otherwise it'll create a new file.&nbsp;&nbsp;if you 
pass in null as [value] Node, or no [value] node
at all, any existing file will be deleted, and no 
new created.&nbsp;&nbsp;thread safe";
				return;
			}

            string file = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(file))
				throw new ArgumentException("You need to define which file to save, as value of [magix.file.save]");

            if (!Ip(e.Params).Contains("value") || Ip(e.Params)["value"].Get<string>() == null)
			{
				// Deletes an existing file
				File.Delete(HttpContext.Current.Server.MapPath(file));
			}
			else
			{
                string fileContent = Ip(e.Params)["value"].Get<string>();
                File.Delete(HttpContext.Current.Server.MapPath(file));

				using (TextWriter writer = 
				       new StreamWriter(File.OpenWrite(HttpContext.Current.Server.MapPath(file))))
				{
					writer.Write(fileContent);
				}
			}
		}

		/**
		 * creates a new directory
		 */
		[ActiveEvent(Name = "magix.file.create-directory")]
		public static void magix_file_create_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.create-directory"].Value = "media/tmp";
				e.Params["inspect"].Value = @"creates the directory from value of [magix.file.create-directory].
&nbsp;&nbsp;thread safe";
				return;
			}

            string path = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("you need to define which directory to create, as value of [magix.file.create-directory]");

			Directory.CreateDirectory(HttpContext.Current.Server.MapPath(path));
		}

		/**
		 * deletes the current directory
		 */
		[ActiveEvent(Name = "magix.file.delete-directory")]
		public static void magix_file_delete_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.delete-directory"].Value = "media/tmp";
				e.Params["inspect"].Value = @"deletes the given directory from disc.
&nbsp;&nbsp;thread safe";
				return;
			}

            string path = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("you need to define which directory to delete, as value of [magix.file.delete-directory]");

			Directory.Delete(HttpContext.Current.Server.MapPath(path));
		}

		/**
		 * moves the given directory
		 */
		[ActiveEvent(Name = "magix.file.move-directory")]
		public static void magix_file_move_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.move-directory"].Value = "media/tmp";
				e.Params["magix.file.move-directory"]["to"].Value = "media/some-new-directory";
				e.Params["inspect"].Value = @"moves the given directory to [to].
&nbsp;&nbsp;thread safe";
				return;
			}

            string from = HttpContext.Current.Server.MapPath(Ip(e.Params).Get<string>());

			if (string.IsNullOrEmpty(from))
				throw new ArgumentException("you need to define which directory to copy, as value of [magix.files.move-directory]");

            if (!Ip(e.Params).Contains("to") || Ip(e.Params)["to"].Get<string>("") == "")
				throw new ArgumentException("you need to define which directory to copy to, as [to] node");

            string to = HttpContext.Current.Server.MapPath(Ip(e.Params)["to"].Get<string>());

			Directory.Move(from, to);
		}

		/**
		 * lists all files in directory
		 */
		[ActiveEvent(Name = "magix.file.list-files")]
		public static void magix_file_list_files(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.list-files"].Value = "system42";
				e.Params["magix.file.list-files"]["filter"].Value = "*.hl";
				e.Params["inspect"].Value = @"lists the files in the given
path, which must be relative to the app's main path.&nbsp;&nbsp;
use [filter] as a search pattern.&nbsp;&nbsp;thread safe";
				return;
			}

            string dir = Ip(e.Params).Value != null ? Ip(e.Params).Get<string>().TrimStart('/') : "~/";
            string filter = Ip(e.Params).Contains("filter") ? Ip(e.Params)["filter"].Get<string>() : null;

			string[] files = null;

			if (string.IsNullOrEmpty(filter))
				files = Directory.GetFiles(HttpContext.Current.Server.MapPath(dir));
			else
				files = Directory.GetFiles(HttpContext.Current.Server.MapPath(dir), filter);

			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
                Ip(e.Params)["files"][idxFile.Substring(rootDir.Length)].Value = null;
			}
		}

		/**
		 * lists all child directories
		 */
		[ActiveEvent(Name = "magix.file.list-directories")]
		public static void magix_file_list_directories(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["magix.file.list-directories"].Value = "system42";
				e.Params["magix.file.list-directories"]["filter"].Value = "*s*";
				e.Params["inspect"].Value = @"lists the directories in [directories] in the given
[directory], which must be relative to the app's main path.&nbsp;&nbsp;
use [filter] as a search pattern.&nbsp;&nbsp;thread safe";
				return;
			}

            string dir = Ip(e.Params).Value != null ? Ip(e.Params).Get<string>().TrimStart('/') : "~/";
            string filter = Ip(e.Params).Contains("filter") ? Ip(e.Params)["filter"].Get<string>() : null;

			string[] files = null;

			if (string.IsNullOrEmpty(filter))
				files = Directory.GetDirectories(HttpContext.Current.Server.MapPath(dir));
			else
				files = Directory.GetDirectories(HttpContext.Current.Server.MapPath(dir), filter);

			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
                Ip(e.Params)["directories"][idxFile.Substring(rootDir.Length)].Value = null;
			}
		}
	}
}

