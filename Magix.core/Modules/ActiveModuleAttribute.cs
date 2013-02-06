﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
     * Level3: Mark your Active Modules with this attribute. If you mark your Modules with this attribute
     * you can load them using the PluginLoader.LoadControl method. This is the main
     * attribute for being able to create ActiveModules
     */
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class ActiveModuleAttribute : Attribute
    {
    }
}