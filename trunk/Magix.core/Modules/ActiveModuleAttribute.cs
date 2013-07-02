/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
     * Mark your Active Modules with this attribute, unless you inherit your UserControl
     * from the ActiveModule class. If you mark your Modules with this attribute
     * you can load them using the PluginLoader.LoadControl method. This is the main
     * attribute for being able to create Active Modules. If you inherit your class from ActiveModule,
     * it will automatically embed this attribute to it
     */
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActiveModuleAttribute : Attribute
    {
    }
}
