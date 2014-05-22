/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using System.Globalization;

namespace Magix.execute
{
    internal class DataBaseRemoval
    {
        /*
         * helper for above
         */
        internal static void Remove(string activeEvent, string type)
        {
            Node removeNode = new Node();
            removeNode["prototype"]["event"].Value = activeEvent;
            removeNode["prototype"]["type"].Value = type;

            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(DataBaseRemoval),
                "magix.data.remove",
                removeNode);
        }
    }
}
