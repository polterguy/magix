/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
                e.Params["inspect"].Value = @"<p>loads the file from value of [file] into the [value] node 
as text</p><p>the file can be a relative path, to fetch a document beneath your web application directory 
structure.&nbsp;&nbsp;the value in [file] can be either a constant or an expression</p><p>in addition, you 
can supply a plugin loader to load your files, which is done by setting the [file] parameter to the text;
'plugin:' and appending the name of the active event you whish to use as the file loader after 'plugin:'.
&nbsp;&nbsp; for instance 'plugin:microsoft.sql.select-as-text' or 'plugin:magix.web.get-file'.&nbsp;&nbsp;
this will expect an active event capable of returning text as [value].&nbsp;&nbsp;if you supply a plugin 
loader, then all parameters beneath the [file] parameter will be passed into the plugin, and used as 
parameters to the plugin loader</p><p>thread safe</p>";
                e.Params["magix.file.load"]["file"].Value = "core-scripts/some-files.txt";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("file"))
                throw new ArgumentException("you need to supply which file to load as the [file] parameter");

            string filepath = Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string;
			if (string.IsNullOrEmpty(filepath))
				throw new ArgumentException("you need to define which file to load, as [file]");

			if (filepath.StartsWith("plugin:"))
            {
                Node fileContent = LoadPluginFile(ip, filepath);
                ip["value"].Value = fileContent.Value;
            }
            else
			{
                LoadLocalFile(ip, filepath);
			}
		}

        private static Node LoadPluginFile(Node ip, string filepath)
        {
            string activeEvent = filepath.Substring(7);
            Node parsToPluginLoader = ip["file"].Clone();
            
            RaiseActiveEvent(
                activeEvent,
                parsToPluginLoader);

            return parsToPluginLoader["value"];
        }

        private static void LoadLocalFile(Node ip, string filepath)
        {
            using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(filepath)))
            {
                ip["value"].Value = reader.ReadToEnd();
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
                e.Params["inspect"].Value = @"<p>saves a file with the path given
in [file]</p><p>the file to save is given as the [value] node.&nbsp;&nbsp;
[magix.file.save] will overwrite an existing file, if any exist, otherwise it will 
create a new file.&nbsp;&nbsp;if you pass in null as [value] Node, or no [value] node
at all, any existing file will be deleted, and no new file created.&nbsp;&nbsp;both the 
[file], and [value] can be either expressions or constants</p><p>thread safe</p>";
                e.Params["magix.file.save"]["file"].Value = "tmp/sample.txt";
				e.Params["magix.file.save"]["value"].Value = @"contents of file";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("file"))
                throw new ArgumentException("you need to define which file to save, as [file]");

            string file = Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string;
			if (string.IsNullOrEmpty(file))
				throw new ArgumentException("you need to define which file to save, as [file]");

            if (!ip.Contains("value") || ip["value"].Get<string>() == null)
			{
				// Deletes an existing file
				File.Delete(HttpContext.Current.Server.MapPath(file));
			}
			else
			{
                string fileContent = Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as string;
                File.Delete(HttpContext.Current.Server.MapPath(file));
				using (TextWriter writer = new StreamWriter(File.OpenWrite(HttpContext.Current.Server.MapPath(file))))
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
                e.Params["inspect"].Value = @"<p>creates the directory from value of 
[magix.file.create-directory]</p><p>the directory path can be either an expression, 
or a constant</p><p>thread safe</p>";
                e.Params["magix.file.create-directory"].Value = "media/tmp";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            string path = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
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
                e.Params["inspect"].Value = @"<p>deletes the directory from value of 
[magix.file.delete-directory]</p><p>the directory path can be either an expression, 
or a constant</p><p>thread safe</p>";
                e.Params["magix.file.delete-directory"].Value = "media/tmp";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            string path = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("You need to define which directory to delete, as value of [magix.file.delete-directory]");

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
                e.Params["inspect"].Value = @"<p>moves the given directory to [to]</p><p>both 
the value of the [magix.file.move-directory] and the [to] node can be either expressions or 
constants</p><p>thread safe</p>";
                e.Params["magix.file.move-directory"].Value = "media/tmp";
                e.Params["magix.file.move-directory"]["to"].Value = "media/some-new-directory";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you need to define which directory to copy as value of [magix.file.move-directory]");

            string from = HttpContext.Current.Server.MapPath(Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string);

            if (string.IsNullOrEmpty(from))
                throw new ArgumentException("you need to define which directory to copy, as value of [magix.files.move-directory]");

            if (!ip.Contains("to") || string.IsNullOrEmpty(ip["to"].Get<string>()))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");

            string to = HttpContext.Current.Server.MapPath(Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string);

            Directory.Move(from, to);
        }

        /**
         * moves the given directory
         */
        [ActiveEvent(Name = "magix.file.move-file")]
        public static void magix_file_move_file(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>moves the given file to the [to] directory</p>
<p>both the file to move, and the where to move it, within the [to] node, can be either expressions, 
or constants</p><p>thread safe</p>";
                e.Params["magix.file.move-file"].Value = "media/tmp";
                e.Params["magix.file.move-file"]["to"].Value = "media/some-new-directory";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you need to tell the engine which file to move as the value of the [magix.file.move-file]");

            string from = HttpContext.Current.Server.MapPath(Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string);

            if (string.IsNullOrEmpty(from))
                throw new ArgumentException("you need to define which file to move, as value of [magix.files.move-directory]");

            if (!ip.Contains("to") || string.IsNullOrEmpty(ip["to"].Get<string>()))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");

            string to = HttpContext.Current.Server.MapPath(Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string);

            File.Move(from, to);
        }

        /**
         * lists all files in directory
         */
		[ActiveEvent(Name = "magix.file.list-files")]
		public static void magix_file_list_files(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>lists the files in the given directory, which must 
be relative to the app's main path</p><p>use [filter] as a search pattern.&nbsp;&nbsp;the directory 
to list the files from is given through the [magix.file.list-files] node's value, and can be both 
an expression, or a constant.&nbsp;&nbsp;if no path is given, it will list all the files from the 
root directory of your web application.&nbsp;&nbsp;both the path, and the filter, can be both 
constants or expressions</p><p>thread safe</p>";
                e.Params["magix.file.list-files"].Value = "system42";
				e.Params["magix.file.list-files"]["filter"].Value = "*.hl";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            string dir = ip.Value != null ? 
                Expressions.GetExpressionValue(ip.Get<string>().TrimStart('/'), dp, ip, false) as string : 
                "~/";
            string filter = ip.Contains("filter") ? 
                Expressions.GetExpressionValue(ip["filter"].Get<string>(), dp, ip, false) as string : 
                null;

			string[] files = null;

			if (string.IsNullOrEmpty(filter))
				files = Directory.GetFiles(HttpContext.Current.Server.MapPath(dir));
			else
				files = Directory.GetFiles(HttpContext.Current.Server.MapPath(dir), filter);

			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
                ip["files"][idxFile.Substring(rootDir.Length)].Value = null;
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
                e.Params["inspect"].Value = @"<p>lists the directories in [directories] in the given
directory, given through the value of the [magix.file.list-directories], which must be relative to 
the app's main path</p><p>use [filter] as a search pattern.&nbsp;&nbsp;both [filter] and the value of
the [magix.file.list-directories] node can be both expressions or constants</p><p>thread safe</p>";
                e.Params["magix.file.list-directories"].Value = "system42";
				e.Params["magix.file.list-directories"]["filter"].Value = "*s*";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            string dir = ip.Value != null ? 
                Expressions.GetExpressionValue(ip.Get<string>().TrimStart('/'), dp, ip, false) as string : 
                "~/";
            string filter = ip.Contains("filter") ? 
                Expressions.GetExpressionValue(ip["filter"].Get<string>(), dp, ip, false) as string : 
                null;

			string[] files = null;

			if (string.IsNullOrEmpty(filter))
				files = Directory.GetDirectories(HttpContext.Current.Server.MapPath(dir));
			else
				files = Directory.GetDirectories(HttpContext.Current.Server.MapPath(dir), filter);

			string rootDir = HttpContext.Current.Server.MapPath("~");
			foreach (string idxFile in files)
			{
                ip["directories"][idxFile.Substring(rootDir.Length)].Value = null;
			}
		}

        /**
         * return true if directory exists
         */
        [ActiveEvent(Name = "magix.file.directory-exist")]
        public static void magix_file_directory_exist(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns true as [value] if the directory in the value of the 
node exist.&nbsp;&nbsp;the value of [magix.file.directory-exist] can be both an expression or a constant</p><p>
thread safe</p>";
                e.Params["magix.file.directory-exist"].Value = "system42";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you didn't supply a directory to search for [magix.file.directory-exist]");

            string dir = Expressions.GetExpressionValue(ip.Get<string>().TrimStart('/'), dp, ip, false) as string;

            if (Directory.Exists(HttpContext.Current.Server.MapPath(dir)))
                ip["value"].Value = true;
            else
                ip["value"].Value = false;
        }

        /**
         * return true if directory exists
         */
        [ActiveEvent(Name = "magix.file.file-exist")]
        public static void magix_file_file_exist(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns true as [value] if the file in the value of the 
node exist.&nbsp;&nbsp;the value of [magix.file.file-exist] can be both an expression, or a constant</p>
<p>thread safe</p>";
                e.Params["magix.file.file-exist"].Value = "system42";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new ArgumentException("you didn't supply a file for [file-exist]");

            string file =  Expressions.GetExpressionValue(ip.Get<string>().TrimStart('/'), dp, ip, false) as string;

            if (File.Exists(HttpContext.Current.Server.MapPath(file)))
                ip["value"].Value = true;
            else
                ip["value"].Value = false;
        }
    }
}

