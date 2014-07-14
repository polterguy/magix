/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Configuration;
using Ionic.Zip;
using Magix.Core;

namespace Magix.tiedown
{
	/*
	 * contains the package/.zip functionality
	 */
    public sealed class PackageCore : ActiveController
	{
		/*
		 * unpacks a zip file
		 */
		[ActiveEvent(Name = "magix.package.unpack")]
        public static void magix_package_unpack(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.package.unpack-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.package.unpack-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            string zipFile = Expressions.GetExpressionValue<string>(ip["zip"].Get<string>(), dp, ip, false);
            string directory = Expressions.GetExpressionValue<string>(ip["directory"].Get<string>(), dp, ip, false);
            if (ip["directory"].Count > 0)
                directory = Expressions.FormatString(dp, ip, ip["directory"], directory);

            string zipAbsolutePath = HttpContext.Current.Server.MapPath(zipFile);
            string dirAbsolutePath = HttpContext.Current.Server.MapPath(directory);

            using (ZipFile zip = ZipFile.Read(zipAbsolutePath))
            {
                foreach (ZipEntry idxZipFile in zip)
                {
                    idxZipFile.Extract(dirAbsolutePath, ExtractExistingFileAction.OverwriteSilently);
                }
            }
        }

        /*
         * lists files inside a zip file without extracting it
         */
        [ActiveEvent(Name = "magix.package.list-files")]
        public static void magix_package_list_files(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.package.list-files-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.package.list-files-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string zipFile = Expressions.GetExpressionValue<string>(ip["zip"].Get<string>(), dp, ip, false);
            string zipAbsolutePath = HttpContext.Current.Server.MapPath(zipFile);

            using (ZipFile zip = ZipFile.Read(zipAbsolutePath))
            {
                foreach (ZipEntry idxZipFile in zip)
                {
                    if (!idxZipFile.IsDirectory)
                        ip["files"][idxZipFile.FileName].Value = null;
                }
            }
        }

        /*
         * returns file content without unpacking
         */
        [ActiveEvent(Name = "magix.package.get-content")]
        public static void magix_package_get_content(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.package.get-content-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.tiedown",
                    "Magix.tiedown.hyperlisp.inspect.hl",
                    "[magix.package.get-content-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            string zipFile = Expressions.GetExpressionValue<string>(ip["zip"].Get<string>(), dp, ip, false);
            string zipAbsolutePath = HttpContext.Current.Server.MapPath(zipFile);
            string file = Expressions.GetExpressionValue<string>(ip["file"].Get<string>(), dp, ip, false);

            using (ZipFile zip = ZipFile.Read(zipAbsolutePath))
            {
                foreach (ZipEntry idxZipFile in zip)
                {
                    if (file == idxZipFile.FileName)
                    {
                        // match
                        using (MemoryStream stream = new MemoryStream())
                        {
                            idxZipFile.Extract(stream);
                            stream.Position = 0;
                            using (StreamReader reader = new StreamReader(stream))
                            {
                                string fileContent = reader.ReadToEnd();
                                ip["value"].Value = fileContent;
                            }
                        }
                    }
                }
            }
        }
    }
}

