/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
	 * contains logic for traversing directories
	 */
	public class DirectoriesCore : ActiveController
	{
        private static string _basePath;

        static DirectoriesCore()
        {
            _basePath = HttpContext.Current.Server.MapPath("~");
        }

        /*
         * lists all child directories
         */
        [ActiveEvent(Name = "magix.file.list-directories")]
        public static void magix_file_list_directories(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.list-directories-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.list-directories-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string directory = ip.Contains("directory") ?
                Expressions.GetExpressionValue(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false) as string :
                "~/";

            string filter = ip.Contains("filter") ?
                Expressions.GetExpressionValue(ip["filter"].Get<string>(), dp, ip, false) as string :
                null;

            string[] files = null;

            bool absolutePath = true;
            if (!directory.Contains(":"))
            {
                directory = _basePath + directory;
                absolutePath = false;
            }

            if (string.IsNullOrEmpty(filter))
                files = Directory.GetDirectories(directory);
            else
                files = Directory.GetDirectories(directory, filter);

            string rootDir = _basePath;
            foreach (string idxFile in files)
            {
                string fileName = idxFile;
                if (!absolutePath)
                    fileName = idxFile.Substring(rootDir.Length);
                fileName = fileName.Replace("\\", "/");
                ip["directories"][fileName].Value = null;
            }
        }

        /*
         * creates a new directory
         */
		[ActiveEvent(Name = "magix.file.create-directory")]
		public static void magix_file_create_directory(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.create-directory-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.create-directory-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("directory"))
                throw new ArgumentException("[create-directory] needs a [directory] argument to know where to create the directory");
            string path = Expressions.GetExpressionValue(ip["directory"].Get<string>(), dp, ip, false) as string;

            if (!path.Contains(":"))
                path = _basePath + path;

			Directory.CreateDirectory(path);
		}

		/*
		 * deletes the current directory
		 */
		[ActiveEvent(Name = "magix.file.delete-directory")]
		public static void magix_file_delete_directory(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.delete-directory-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.delete-directory-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("directory"))
                throw new ArgumentException("you must supply a [directory] to [delete-directory]");

            string path = Expressions.GetExpressionValue(ip["directory"].Get<string>(), dp, ip, false) as string;

            if (!path.Contains(":"))
                path = _basePath + path;

            Directory.Delete(path);
		}

        /*
         * moves the given directory
         */
        [ActiveEvent(Name = "magix.file.move-directory")]
        public static void magix_file_move_directory(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.move-directory-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.move-directory-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("from"))
                throw new ArgumentException("you need to supply a [from] node to [move-directory]");
            string from = Expressions.GetExpressionValue(ip["from"].Get<string>(), dp, ip, false) as string;

            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");
            string to = Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string;

            if (!to.Contains(":"))
                to = _basePath+ to;

            Directory.Move(from, to);
        }

        /*
         * return true if directory exists
         */
        [ActiveEvent(Name = "magix.file.directory-exist")]
        public static void magix_file_directory_exist(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.directory-exist-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.directory-exist-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("directory"))
                throw new ArgumentException("you didn't supply a [directory] to search for to [magix.file.directory-exist]");

            string dir = Expressions.GetExpressionValue(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false) as string;

            if (!dir.Contains(":"))
                dir = _basePath + dir;

            ip["value"].Value = Directory.Exists(dir);
        }
    }
}

