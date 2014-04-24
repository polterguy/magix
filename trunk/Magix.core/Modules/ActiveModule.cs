/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Web;
using System.Diagnostics;
using System.Threading;

namespace Magix.Core
{
    /**
     * Inherit your Active Modules from this class
     */
	[ActiveModule]
    public abstract class ActiveModule : UserControl
    {
        private bool _firstLoad;

        /**
         * Called internally by Viewports, if wired correctly
         */
        public virtual void InitialLoading(Node node)
        {
            _firstLoad = true;
        }

        /**
         * Is true if this was the request when the Module was loaded initially, somehow
         */
        protected bool FirstLoad
        {
            get { return _firstLoad; }
        }

        /**
         * Shorthand for raising events. Will return a node, initially created empty, 
         * but passed onto the Event Handler(s)
         */
        protected Node RaiseActiveEvent(string eventName)
        {
            Node node = new Node();
            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                eventName,
                node);
            return node;
        }

        /**
         * Shorthand for raising events
         */
        protected void RaiseActiveEvent(string eventName, Node node)
        {
            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                eventName,
                node);
        }

        /**
         * Will return the 'base' URL of your application. Meaning if your application
         * is installed on x.com/f then x.com/f will always be returned from this method.
         * Useful for using as foundation for finding specific files and so on
         */
        protected string GetApplicationBaseUrl()
        {
            return string.Format(
                "{0}://{1}{2}",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.ServerVariables["HTTP_HOST"],
                (Page.Request.ApplicationPath.Equals("/")) ? 
                    "/" : 
                    HttpContext.Current.Request.ApplicationPath + "/")
                        .Replace("Default.aspx", "").Replace("default.aspx", "");
        }

        /**
         * returns the instruction pointer. will by default return the given Node if no IP is found in node
         */
        protected static Node Ip(Node pars)
        {
            if (pars.Contains("_ip"))
                return pars["_ip"].Value as Node;
            return pars;
        }
    }
}
