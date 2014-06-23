/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Reflection;
using System.Configuration;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * helper for loading files from resources
	 */
    public class ResourceLoaderCore : ActiveController
	{
        /*
         * loads a file from a dll as a resource
         */
        [ActiveEvent(Name = "magix.file.load-from-resource")]
        public static void magix_file_load_from_resource(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.load-from-resource-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.file",
                    "Magix.file.hyperlisp.inspect.hl",
                    "[magix.file.load-from-resource-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("you need to supply a [file] parameter");

            if (!ip["file"].ContainsValue("assembly"))
                throw new ArgumentException("you need to supply which assembly to load the file from as the [file]/[assembly] parameter");
            string assembly = Expressions.GetExpressionValue<string>(ip["file"]["assembly"].Get<string>(), dp, ip, false);

            if (!ip["file"].ContainsValue("resource-name"))
                throw new ArgumentException("you need to define which resource to load as [file]/[resource-name]");
            string resourceName = Expressions.GetExpressionValue<string>(ip["file"]["resource-name"].Get<string>(), dp, ip, false);

            Assembly asm = null;
            foreach (Assembly idxAsm in ModuleControllerLoader.ModuleAssemblies)
            {
                try
                {
                    if (idxAsm.CodeBase.Substring(idxAsm.CodeBase.LastIndexOf('/') + 1).ToLower().Replace(".dll", "") == assembly.ToLower())
                    {
                        asm = idxAsm;
                        break;
                    }
                }
                catch
                {
                    ; // do nothing, some assemblies throws for weird reasons
                }
            }
            using (Stream stream = asm.GetManifestResourceStream(resourceName))
            {
                using (TextReader reader = new StreamReader(stream))
                {
                    ip["value"].Value = reader.ReadToEnd();
                }
            }
        }
    }
}

