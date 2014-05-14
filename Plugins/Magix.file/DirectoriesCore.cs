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
	 * contains logic for traversing directories
	 */
	public class DirectoriesCore : ActiveController
	{
        /*
         * lists all child directories
         */
        [ActiveEvent(Name = "magix.file.list-directories")]
        public static void magix_file_list_directories(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>lists the directories in [directories] in the given
directory, given through the value of [directory], which can be either a relative path to the app's 
main path, or an absolute path</p><p>use [filter] as a search pattern.&nbsp;&nbsp;both [filter] and 
[directory] node can be both expressions or constants.&nbsp;&nbsp;if you don't supply a [directory] 
node, the event will list all directories on the root folder of your web application</p><p>thread 
safe</p>";
                e.Params["magix.file.list-directories"]["directory"].Value = "system42";
                e.Params["magix.file.list-directories"]["filter"].Value = "*s*";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

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
                directory = HttpContext.Current.Server.MapPath(directory);
                absolutePath = false;
            }

            if (string.IsNullOrEmpty(filter))
                files = Directory.GetDirectories(directory);
            else
                files = Directory.GetDirectories(directory, filter);

            string rootDir = HttpContext.Current.Server.MapPath("~");
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
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>creates the directory from value of 
[directory]</p><p>the directory path can be either an expression, or a constant.&nbsp;
&nbsp;the [directory] can be either a relative path, underneath your web application 
folder, or an absolute path</p><p>thread safe</p>";
                e.Params["magix.file.create-directory"]["directory"].Value = "media/tmp";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("directory"))
                throw new ArgumentException("[create-directory] needs a [directory] argument to know where to create the directory");

            string path = Expressions.GetExpressionValue(ip["directory"].Get<string>(), dp, ip, false) as string;
			if (string.IsNullOrEmpty(path))
				throw new ArgumentException("you need to define which directory to create, as value of [directory]");

            if (!path.Contains(":"))
                path = HttpContext.Current.Server.MapPath(path);

			Directory.CreateDirectory(path);
		}

		/*
		 * deletes the current directory
		 */
		[ActiveEvent(Name = "magix.file.delete-directory")]
		public static void magix_file_delete_directory(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>deletes the directory from value of 
[directory]</p><p>the directory path can be either an expression, or a constant.&nbsp;
&nbsp;also the directory can be either a relative path, beneath your web application, 
or an absolute path on disc</p><p>thread safe</p>";
                e.Params["magix.file.delete-directory"]["directory"].Value = "media/tmp";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("directory"))
                throw new ArgumentException("you must supply a [directory] to [delete-directory]");

            string path = Expressions.GetExpressionValue(ip["directory"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(path))
                throw new ArgumentException("you need to define which directory to delete, as value of [magix.file.delete-directory]");

            if (!path.Contains(":"))
                path = HttpContext.Current.Server.MapPath(path);

            Directory.Delete(path);
		}

        /*
         * moves the given directory
         */
        [ActiveEvent(Name = "magix.file.move-directory")]
        public static void magix_file_move_directory(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>moves the given directory in [from] to [to]</p>
<p>both the value of the [from] and the [to] node can be either expressions or constants.&nbsp;
&nbsp;both the [from] and [to] nodes can either point to a relative directory beneath the web 
application folder, or an absolute path on disc</p><p>thread safe</p>";
                e.Params["magix.file.move-directory"]["from"].Value = "media/tmp";
                e.Params["magix.file.move-directory"]["to"].Value = "media/some-new-directory";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("from"))
                throw new ArgumentException("you need to supply a [from] node to [move-directory]");

            if (string.IsNullOrEmpty(ip["from"].Get<string>()))
                throw new ArgumentException("you need to define which directory to copy as value of [from]");

            string from = Expressions.GetExpressionValue(ip["from"].Get<string>(), dp, ip, false) as string;

            if (string.IsNullOrEmpty(from))
                throw new ArgumentException("you need to define which directory to copy, as value of [from]");

            if (!from.Contains(":"))
                from = HttpContext.Current.Server.MapPath(from);

            if (!ip.Contains("to") || string.IsNullOrEmpty(ip["to"].Get<string>()))
                throw new ArgumentException("you need to define which directory to copy to, as [to] node");

            string to = Expressions.GetExpressionValue(ip["to"].Get<string>(), dp, ip, false) as string;

            if (!to.Contains(":"))
                to = HttpContext.Current.Server.MapPath(to);

            Directory.Move(from, to);
        }

        /*
         * return true if directory exists
         */
        [ActiveEvent(Name = "magix.file.directory-exist")]
        public static void magix_file_directory_exist(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>returns true as [value] if the directory in the value 
of [directory] node exist.&nbsp;&nbsp;the value of [directory] can be both an expression or a constant.
&nbsp;&nbsp;in adition, the [directory] can either be a relative path, beneath the web application's 
main folder, or an absolute path on disc</p><p>thread safe</p>";
                e.Params["magix.file.directory-exist"]["directory"].Value = "system42";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            if (!ip.Contains("directory") || string.IsNullOrEmpty(ip["directory"].Get<string>()))
                throw new ArgumentException("you didn't supply a [directory] to search for to [magix.file.directory-exist]");

            string dir = Expressions.GetExpressionValue(ip["directory"].Get<string>().TrimStart('/'), dp, ip, false) as string;

            if (!dir.Contains(":"))
                dir = HttpContext.Current.Server.MapPath(dir);

            ip["value"].Value = Directory.Exists(dir);
        }
    }
}

