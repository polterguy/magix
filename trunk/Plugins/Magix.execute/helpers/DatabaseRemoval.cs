/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
        internal static void Remove(string activeEvent, string type, Node pars)
        {
            Node removeNode = new Node("magix.data.remove");
            removeNode["prototype"]["event"].Value = activeEvent;
            removeNode["prototype"]["type"].Value = type;

            Node oldIp = pars["_ip"].Get<Node>();
            Node oldDp = pars["_dp"].Get<Node>();
            try
            {
                pars["_ip"].Value = removeNode;
                pars["_dp"].Value = removeNode;
                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(DataBaseRemoval),
                    "magix.execute",
                    removeNode);
            }
            finally
            {
                pars["_ip"].Value = oldIp;
                pars["_dp"].Value = oldDp;
            }
        }
    }
}
