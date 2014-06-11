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
	 * contains logic for loading, saving and loading files
	 */
	public class FilesCore : ActiveController
	{
        private static string _basePath;

        static FilesCore()
        {
            _basePath = HttpContext.Current.Server.MapPath("~");
        }

        /*
         * lists all files in directory
         */
        [ActiveEvent(Name = "magix.file.list-files")]
        public static void magix_file_list_files(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(e.Params))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.list-files-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.list-files-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string dir = ip.ContainsValue("directory") ?
                Expressions.GetExpressionValue(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false) as string :
                "";
            string filter = ip.ContainsValue("filter") ?
                Expressions.GetExpressionValue(ip["filter"].Get<string>(), dp, ip, false) as string :
                null;

            string[] files = null;

            bool absolutePath = true;
            if (!dir.Contains(":"))
            {
                dir = _basePath + dir;
                absolutePath = false;
            }

            if (string.IsNullOrEmpty(filter))
                files = Directory.GetFiles(dir);
            else
                files = Directory.GetFiles(dir, filter);

            string rootDir = _basePath;
            foreach (string idxFile in files)
            {
                string fileName = idxFile;
                if (!absolutePath)
                    fileName = idxFile.Substring(rootDir.Length);
                fileName = fileName.Replace("\\", "/");

                ip["files"][fileName].Value = null;
            }
        }

        /*
         * loads a file from disc, relatively from the root of the web application
         */
		[ActiveEvent(Name = "magix.file.load")]
		public static void magix_file_load(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.load-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.load-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("you need to supply which file to load as the [file] parameter");

            string filepath = Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string;

			if (filepath.StartsWith("plugin:"))
            {
                string activeEvent = filepath.Substring(7);
                RaiseActiveEvent(
                    activeEvent,
                    e.Params);
            }
            else
			{
                using (TextReader reader = File.OpenText(_basePath + filepath))
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
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.save-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.save-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("you need to define which file to save, as [file]");

            string file = Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string;

            if (!file.Contains(":"))
                file = _basePath + file;

            if (ip.ContainsValue("value"))
            {
                string fileContent = Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as string;
                using (Stream fileStream = File.Open(file, FileMode.Create))
                {
                    using (TextWriter writer = new StreamWriter(fileStream))
                    {
                        writer.Write(fileContent);
                    }
                }
            }
            else
            {
                // Deletes an existing file
                File.Delete(file);
            }
		}

        /*
         * moves the given file
         */
        [ActiveEvent(Name = "magix.file.move-file")]
        public static void magix_file_move_file(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.move-file-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.move-file-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("from"))
                throw new ArgumentException("you need to tell the engine which file to move as the value of the [from]");

            string from = Expressions.GetExpressionValue(ip["from"].Get<string>(), dp, ip, false) as string;
            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define where to move file to, as [to] node");

            string to = Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string;
            if (!to.Contains(":"))
                to = _basePath + to;

            File.Move(from, to);
        }

        /*
         * moves the given file
         */
        [ActiveEvent(Name = "magix.file.copy-file")]
        public static void magix_file_copy_file(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.copy-file-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.copy-file-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("from"))
                throw new ArgumentException("you need to tell the engine which file to copy as the value of the [from]");

            string from = Expressions.GetExpressionValue(ip["from"].Get<string>(), dp, ip, false) as string;
            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define which file to copy to, as [to] node");

            string to = Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string;
            if (!to.Contains(":"))
                to = _basePath + to;

            File.Copy(from, to);
        }

        /*
         * return true if directory exists
         */
        [ActiveEvent(Name = "magix.file.file-exist")]
        public static void magix_file_file_exist(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.file-exist-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.file-exist-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("you didn't supply a [file] for [file-exist]");

            string file = Expressions.GetExpressionValue(ip["file"].Get<string>().TrimStart('/'), dp, ip, false) as string;
            if (!file.Contains(":"))
                file = _basePath + file;

            ip["value"].Value = File.Exists(file);
        }
    }
}

