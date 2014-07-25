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
                    "[magix.file.list-directories-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.list-directories-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string directory = ip.Contains("directory") ?
                Expressions.GetExpressionValue<string>(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false) :
                "";

            string filter = ip.Contains("filter") ?
                Expressions.GetExpressionValue<string>(ip["filter"].Get<string>(), dp, ip, false) :
                null;

            string[] directories = null;

            bool absolutePath = true;
            if (!directory.Contains(":"))
            {
                directory = _basePath + directory;
                absolutePath = false;
            }

            if (string.IsNullOrEmpty(filter))
                directories = Directory.GetDirectories(directory);
            else
                directories = Directory.GetDirectories(directory, filter);

            string rootDir = _basePath;
            foreach (string idxDirectory in directories)
            {
                string directoryPath = idxDirectory;
                if (!absolutePath)
                    directoryPath = idxDirectory.Substring(rootDir.Length);
                directoryPath = directoryPath.Replace("\\", "/");
                ip["directories"][directoryPath].Value = null;
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
                    "[magix.file.create-directory-dox].value");
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
            string path = Expressions.GetExpressionValue<string>(ip["directory"].Get<string>(), dp, ip, false);
            if (ip["directory"].Count > 0)
                path = Expressions.FormatString(dp, ip, ip["directory"], path);

            if (!path.Contains(":"))
                path = _basePath + path;

            if (!Directory.Exists(path))
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
                    "[magix.file.delete-directory-dox].value");
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

            string path = Expressions.GetExpressionValue<string>(ip["directory"].Get<string>(), dp, ip, false);
            if (ip["directory"].Count > 0)
                path = Expressions.FormatString(dp, ip, ip["directory"], path);

            if (!path.Contains(":"))
                path = _basePath + path;

            Directory.Delete(path, true);
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
                    "[magix.file.move-directory-dox].value");
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
            string from = Expressions.GetExpressionValue<string>(ip["from"].Get<string>(), dp, ip, false);
            if (ip["from"].Count > 0)
                from = Expressions.FormatString(dp, ip, ip["from"], from);

            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");
            string to = Expressions.GetExpressionValue<string>(ip["to"].Get<string>(), dp, ip, false);
            if (ip["to"].Count > 0)
                to = Expressions.FormatString(dp, ip, ip["to"], to);

            if (!to.Contains(":"))
                to = _basePath + to;

            Directory.Move(from, to);
        }

        /*
         * moves the given directory
         */
        [ActiveEvent(Name = "magix.file.copy-directory")]
        public static void magix_file_copy_directory(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.copy-directory-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.copy-directory-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("from"))
                throw new ArgumentException("you need to supply a [from] node to [move-directory]");
            string from = Expressions.GetExpressionValue<string>(ip["from"].Get<string>(), dp, ip, false);
            if (ip["from"].Count > 0)
                from = Expressions.FormatString(dp, ip, ip["from"], from);

            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");
            string to = Expressions.GetExpressionValue<string>(ip["to"].Get<string>(), dp, ip, false);
            if (ip["to"].Count > 0)
                to = Expressions.FormatString(dp, ip, ip["to"], to);

            if (!to.Contains(":"))
                to = _basePath + to;

            CopyDirectory(from, to);
        }

        /*
         * helper for above
         */
        private static void CopyDirectory(string from, string to)
        {
            DirectoryInfo dir = new DirectoryInfo(from);
            DirectoryInfo[] dirs = dir.GetDirectories();

            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("directory '" + from + "' could not be found");
            }

            if (!Directory.Exists(to))
            {
                Directory.CreateDirectory(to);
            }

            FileInfo[] files = dir.GetFiles();
            foreach (FileInfo file in files)
            {
                file.CopyTo(Path.Combine(to, file.Name), false);
            }

            foreach (DirectoryInfo subdir in dirs)
            {
                CopyDirectory(subdir.FullName, Path.Combine(to, subdir.Name));
            }
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
                    "[magix.file.directory-exist-dox].value");
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

            string directory = Expressions.GetExpressionValue<string>(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false);
            if (ip["directory"].Count > 0)
                directory = Expressions.FormatString(dp, ip, ip["directory"], directory);

            if (!directory.Contains(":"))
                directory = _basePath + directory;

            ip["value"].Value = Directory.Exists(directory);
        }
    }
}

