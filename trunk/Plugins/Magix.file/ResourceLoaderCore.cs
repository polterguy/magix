/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
	/**
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
            if (ShouldInspect(e.Params))
            {
                e.Params["inspect"].Value = @"<p>plugin for loading embedded resources 
as files</p><p>this is useful for using as a plugin loader for the [magix.file.load] 
active event</p><p>thread safe</p>";
                e.Params["magix.file.load"]["file"].Value = "plugin:magix.file.load-from-resource";
                e.Params["magix.file.load"]["file"]["assembly"].Value = "name.of.your.assembly";
                e.Params["magix.file.load"]["file"]["resource-name"].Value = "resource.name.of.your.file.inside.your.assembly.txt";
                return;
            }

            Node ip = Ip(e.Params);
            Node dp = Dp(e.Params);

            if (!ip.Contains("file"))
                throw new ArgumentException("you need to supply a [file] parameter");

            if (!ip["file"].Contains("assembly"))
                throw new ArgumentException("you need to supply which assembly to load the file from as the [file]/[assembly] parameter");

            string assembly = Expressions.GetExpressionValue(ip["file"]["assembly"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(assembly))
                throw new ArgumentException("you need to define which assembly to load file from as [file]/[assembly]");

            string resourceName = Expressions.GetExpressionValue(ip["file"]["resource-name"].Get<string>(), dp, ip, false) as string;
            if (string.IsNullOrEmpty(resourceName))
                throw new ArgumentException("you need to define which resource to load as [file]/[resource-name]");

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

