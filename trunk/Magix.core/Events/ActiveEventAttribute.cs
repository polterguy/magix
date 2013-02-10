﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
     * Level3: Mark your methods with this attribute to make then handle Magix.Brix Active Events. 
     * The Name property is the second argument to the RaiseEvent, or the "name" of the 
     * event being raised. You can mark your methods with multiple instances of this 
     * attribute to catch multiple events in the same event handler. However, as a general
     * rule of thumb it's often better to have one method handling one event
     */
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    public class ActiveEventAttribute : Attribute
    {
        /**
         * Level3: Name of event
         */
        public string Name;

        public ActiveEventAttribute()
        { }
    }
}