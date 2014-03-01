/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.Core
{
    /**
     * Mark your methods with this attribute to make then handle Magix.Brix Active Events. 
     * The Name property is the second argument to the RaiseEvent, or the "name" of the 
     * event being raised. You can mark your methods with multiple instances of this 
     * attribute to catch multiple events in the same event handler. However, as a general
     * rule of thumb it's often better to have one method handling one event. Only
     * methods inside of Controllers and Modules will be handled as active events
     */
    [AttributeUsage(AttributeTargets.Method, AllowMultiple=true, Inherited=true)]
    public class ActiveEventAttribute : Attribute
    {
        /**
         * Name of event. Notice that if this is "", your Active Event Handler
         * will handle ALL Active Events. This is an easy way to create hooks into
         * your code, for creating debuggers, and similar types of "meta components".
         */
        public string Name;
    }
}
