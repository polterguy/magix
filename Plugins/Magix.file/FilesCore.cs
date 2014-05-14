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
	/*
	 * contains logic for loading, saving and re-organizing files
	 */
	public class FilesCore : ActiveController
	{
        /*
         * lists all files in directory
         */
        [ActiveEvent(Name = "magix.file.list-files")]
        public static void magix_file_list_files(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>lists the files in the given [directory], which can 
be either relative to the app's main path, or absolute</p><p>use [filter] as a search pattern.&nbsp;
&nbsp;the directory to list the files from is given through the [directory] node's value, and can be 
both an expression, or a constant.&nbsp;&nbsp;if no path is given, it will list all the files from the 
root directory of your web application.&nbsp;&nbsp;both the path, and the filter, can be both constants 
or expressions</p><p>thread safe</p>";
                e.Params["magix.file.list-files"]["directory"].Value = "system42";
                e.Params["magix.file.list-files"]["filter"].Value = "*.hl";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            string dir = ip.Contains("directory") && ip["directory"].Value != null ?
                Expressions.GetExpressionValue(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false) as string :
                "~/";
            string filter = ip.Contains("filter") ?
                Expressions.GetExpressionValue(ip["filter"].Get<string>(), dp, ip, false) as string :
                null;

            string[] files = null;

            bool absolutePath = true;
            if (!dir.Contains(":"))
            {
                dir = HttpContext.Current.Server.MapPath(dir);
                absolutePath = false;
            }

            if (string.IsNullOrEmpty(filter))
                files = Directory.GetFiles(dir);
            else
                files = Directory.GetFiles(dir, filter);

            string rootDir = HttpContext.Current.Server.MapPath("~");
            foreach (string idxFile in files)
            {
                string fileName = idxFile;
                if (!absolutePath)
                    fileName = idxFile.Substring(rootDir.Length);

                ip["files"][fileName].Value = null;
            }
        }

        /*
         * loads a file from disc, relatively from the root of the web application
         */
		[ActiveEvent(Name = "magix.file.load")]
		public static void magix_file_load(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>loads the file from value of [file] into the [value] node 
as text</p><p>the file can be a relative path, to fetch a document beneath your web application directory 
structure, or an absolute path, to a different directory, which the iis process of course must have access 
to.&nbsp;&nbsp;the value in [file] can be either a constant or an expression</p><p>in addition, you can 
supply a plugin loader to load your files, which is done by setting the [file] parameter to the text;
'plugin:' and appending the name of the active event you whish to use as the file loader after 'plugin:'.
&nbsp;&nbsp; for instance 'plugin:microsoft.sql.select-as-text' or 'plugin:magix.web.get-file'.&nbsp;&nbsp;
this will expect an active event capable of returning text as [value].&nbsp;&nbsp;if you supply a plugin 
loader, then all parameters beneath the [magix.file.load] will be passed into the plugin, and used as 
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
                string activeEvent = filepath.Substring(7);

                RaiseActiveEvent(
                    activeEvent,
                    e.Params);
            }
            else
			{
                using (TextReader reader = File.OpenText(HttpContext.Current.Server.MapPath(filepath)))
                {
                    ip["value"].Value = reader.ReadToEnd();
                }
            }
		}

		/*
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
[file], and [value] can be either expressions or constants.&nbsp;&nbsp;the [file] can 
be either a relative path beneath the web application's folder, or an absolute path</p>
<p>thread safe</p>";
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

            if (!file.Contains(":"))
                file = HttpContext.Current.Server.MapPath(file);

            // Deletes an existing file
            File.Delete(file);

            if (ip.Contains("value") && ip["value"].Value != null)
			{
                string fileContent = Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as string;
				using (TextWriter writer = new StreamWriter(File.OpenWrite(file)))
				{
					writer.Write(fileContent);
				}
			}
		}

        /*
         * moves the given file
         */
        [ActiveEvent(Name = "magix.file.move-file")]
        public static void magix_file_move_file(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>moves the given [from] file to the [to] directory</p>
<p>both the file to move, and the where to move it, can be either expressions, or constants.&nbsp;
&nbsp;both the [from] and [to] can be either relative path's beneath the web application's folder, or 
absolute paths</p><p>thread safe</p>";
                e.Params["magix.file.move-file"]["from"].Value = "media/tmp";
                e.Params["magix.file.move-file"]["to"].Value = "media/some-new-directory";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("from") || string.IsNullOrEmpty(ip["from"].Get<string>()))
                throw new ArgumentException("you need to tell the engine which file to move as the value of the [from]");

            string from = Expressions.GetExpressionValue(ip.Get<string>(), dp, ip, false) as string;
            if (!from.Contains(":"))
                from = HttpContext.Current.Server.MapPath(from);

            if (string.IsNullOrEmpty(from))
                throw new ArgumentException("you need to define which file to move, as value of [magix.files.move-directory]");

            if (!ip.Contains("to") || string.IsNullOrEmpty(ip["to"].Get<string>()))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");

            string to = Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string;
            if (!to.Contains(":"))
                to = HttpContext.Current.Server.MapPath(to);

            File.Move(from, to);
        }

        /*
         * return true if directory exists
         */
        [ActiveEvent(Name = "magix.file.file-exist")]
        public static void magix_file_file_exist(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns true as [value] if the file in the value of 
[file] exist.&nbsp;&nbsp;the value of [magix.file.file-exist] can be both an expression, or a constant.
&nbsp;&nbsp;the [file] can be either a relative path beneath the web application's folder, or an 
absolute path on disc</p><p>thread safe</p>";
                e.Params["magix.file.file-exist"]["file"].Value = "system42/help/index.mml";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("file") || string.IsNullOrEmpty(ip["file"].Get<string>()))
                throw new ArgumentException("you didn't supply a [file] for [file-exist]");

            string file = Expressions.GetExpressionValue(ip["file"].Get<string>().TrimStart('/'), dp, ip, false) as string;
            if (!file.Contains(":"))
                file = HttpContext.Current.Server.MapPath(file);

            ip["value"].Value = File.Exists(file);
        }
    }
}

