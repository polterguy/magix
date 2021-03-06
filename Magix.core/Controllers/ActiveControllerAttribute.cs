﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
     * Mark your controllers with this Attribute, unless you inherit them
     * from ActiveController. Notice that an Active Controller must
     * have a default constructor taking zero parameters. This constructor should also
     * ideally execute FAST since all controllers in your Magix-Brix project will be 
     * instantiated once every request. If you inherit from ActiveController, then
     * your class will automatically be attributed with this attribute class
     */
    [AttributeUsage(AttributeTargets.Class, AllowMultiple=false, Inherited=true)]
    public class ActiveControllerAttribute : Attribute
    {
    }
}
