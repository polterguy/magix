/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Threading;
using System.Collections.Generic;
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
         * returns base path for application
         */
        [ActiveEvent(Name = "magix.file.get-base-path")]
        public static void magix_file_get_base_path(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(e.Params))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.get-base-path-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.get-base-path-sample]");
                return;
            }

            ip["path"].Value = _basePath.Trim('/');
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
                    "[magix.file.list-files-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.list-files-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string dir = ip.ContainsValue("directory") ?
                Expressions.GetFormattedExpression("directory", e.Params, "").TrimStart('/') :
                "";
            string filter = ip.ContainsValue("filter") ?
                Expressions.GetExpressionValue<string>(ip["filter"].Get<string>(), dp, ip, false) :
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
            {
                List<string> searchPatterns = new List<string>(filter.Split(';'));
                List<string> filesList = new List<string>();
                foreach (string idxPattern in searchPatterns)
                {
                    filesList.AddRange(Directory.GetFiles(dir, idxPattern));
                }
                files = filesList.ToArray();
            }

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
                    "[magix.file.load-dox].value");
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

            string filepath = Expressions.GetFormattedExpression("file", e.Params, "");

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
                    "[magix.file.save-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.save-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            string file = Expressions.GetFormattedExpression("file", e.Params, null);
            if (string.IsNullOrEmpty(file))
                throw new ArgumentException("you need to define which file to save, as [file]");

            if (!file.Contains(":"))
                file = _basePath + file;

            // changing its attributes before if it is read only
            if (File.Exists(file) && new FileInfo(file).IsReadOnly)
                File.SetAttributes(file, FileAttributes.Normal);

            if (ip.ContainsValue("value"))
            {
                string fileContent = Expressions.GetExpressionValue<string>(ip["value"].Get<string>(), dp, ip, false);
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
                // deletes an existing file
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
                    "[magix.file.move-file-dox].value");
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

            string from = Expressions.GetFormattedExpression("from", e.Params, "");
            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define where to move file to, as [to] node");

            string to = Expressions.GetFormattedExpression("to", e.Params, "");
            if (!to.Contains(":"))
                to = _basePath + to;

            if (File.Exists(to))
            {
                if (new FileInfo(to).IsReadOnly)
                    File.SetAttributes(to, FileAttributes.Normal);
                File.Delete(to);
            }
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
                    "[magix.file.copy-file-dox].value");
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
            string from = Expressions.GetFormattedExpression("from", e.Params, "");
            if (!from.Contains(":"))
                from = _basePath + from;

            if (!ip.ContainsValue("to"))
                throw new ArgumentException("you need to define which file to copy to, as [to] node");
            string to = Expressions.GetFormattedExpression("to", e.Params, "");
            if (!to.Contains(":"))
                to = _basePath + to;

            File.Copy(from, to, true);
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
                    "[magix.file.file-exist-dox].value");
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

            string file = Expressions.GetFormattedExpression("file", e.Params, "");
            if (!file.Contains(":"))
                file = _basePath + file;

            ip["value"].Value = File.Exists(file);
        }
    }
}

