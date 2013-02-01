/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Web;
using System.Diagnostics;
using System.Collections.Generic;

namespace Magix.Core
{
    /**
     * Level3: Inherit your Active Controllers from this class
     */
    [ActiveController]
	public abstract class ActiveController
    {
        /**
         * Level3: Loads the given module and puts it into your default container
         */
        [DebuggerStepThrough]
        protected Node LoadModule(string name)
        {
            Node node = new Node();
            LoadModule(name, null, node);
            return node;
        }

        /**
         * Level3: Loads the given module and puts it into the given container. Will return the node
         * created and passed into creation
         */
        [DebuggerStepThrough]
        protected Node LoadModule(string name, string container)
        {
            Node node = new Node();
            LoadModule(name, container, node);
            return node;
        }

        /**
         * Level3: Shorthand method for Loading a specific Module and putting it into
         * the given container, with the given Node structure.
         */
        [DebuggerStepThrough]
        protected void LoadModule(string name, string container, Node node)
        {
            if (string.IsNullOrEmpty(name))
                throw new ApplicationException("You have to specify which Module you want to load");

			node["Container"].Value = container;
			node["Name"].Value = name;
			RaiseEvent ("Magix.Core.LoadModule", node);
        }

        /**
         * Level3: Shorthand for raising events. Will return a node, initially created empty, 
         * but passed onto the Event Handler(s)
         */
        [DebuggerStepThrough]
        protected Node RaiseEvent(string eventName)
        {
            Node node = new Node();
            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                eventName,
                ref node);
            return node;
        }

        /**
         * Level3: Shorthand for raising events.
         */
        [DebuggerStepThrough]
        protected void RaiseEvent(string eventName, Node node)
        {
            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                eventName,
                ref node);
        }

        /**
         * Level3: Will return the 'base' URL of your application
         */
        protected string GetApplicationBaseUrl()
        {
            return string.Format(
                "{0}://{1}{2}",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.ServerVariables["HTTP_HOST"],
                (Page.Request.ApplicationPath.Equals("/")) ? 
                    "/" : 
                    HttpContext.Current.Request.ApplicationPath + "/").ToLowerInvariant();
        }

        /**
         * Level3: Shorthand for getting access to our "Page" object.
         */
        protected Page Page
        {
            get
            {
                return HttpContext.Current.Handler as Page;
            }
        }
    }
}
