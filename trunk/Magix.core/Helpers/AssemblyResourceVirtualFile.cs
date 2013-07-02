/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Reflection;
using System.Web.Hosting;

namespace Magix.Core
{
    /*
     * The internal implementation of our VirtualFile or VPP (Virtual Path Provider).
     * Not intended for direct usage yourself, but will exist in 'the background' making
     * sure everything you do turns out right
     */
    internal class AssemblyResourceVirtualFile : VirtualFile
    {
        readonly string _path;

        /*
         * CTOR taking the path and storing to later...
         */
        public AssemblyResourceVirtualFile(string virtualPath)
            : base(virtualPath)
        {
            _path = VirtualPathUtility.ToAppRelative(virtualPath);
        }

        /*
         * Expects either a relative DLL coming from the bin folder of our
         * application or a complete path pointing to a DLL another place. Will split
         * the "path" string into two different parts where the first is the assembly name
         * and the second is the fully qaulified resource identifier of the resource to load.
         */
        public override Stream Open()
        {
            string[] parts;

            if (_path.IndexOf(":") == -1)
                parts = _path.Split('/');
            else
            {
                parts = _path.ToLower().Split(
                    new[] { ".dll" }, 
                    StringSplitOptions.RemoveEmptyEntries);
                parts[0] += ".dll";
            }

            string assemblyName = parts[2];
            string resourceName = parts[3];

            // Checking to see if assmebly is already loaded...
            foreach (Assembly idx in ModuleControllerLoader.ModuleAssemblies)
            {
                if (idx.GlobalAssemblyCache)
                    continue;
                if (idx.CodeBase.Substring(idx.CodeBase.LastIndexOf("/") + 1).ToLower() ==
                    assemblyName.ToLower())
                {
                    Stream retVal = idx.GetManifestResourceStream(resourceName);

                    if (retVal == null)
                        throw new ArgumentException(
                            "Could not find the Virtual File; '" + 
                            _path + 
                            "'. Resource didn't exist within Assembly: " + 
                            assemblyName);

                    return retVal;
                }
            }

            throw new ArgumentException(
                "Could not find the assembly pointed to by the Virtual File; '" + _path + "'");
        }
    }
}
