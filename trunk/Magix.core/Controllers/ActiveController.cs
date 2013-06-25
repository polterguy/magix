/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
     * Inherit your Active Controllers from this class. Contains helper
     * methods for you, for your own controllers
     */
    [ActiveController]
	public abstract class ActiveController
    {
		/**
		 * return true if we are supposed to inspect the active event, and not execute it
		 */
		protected static bool ShouldInspect(Node node)
		{
			return node.Contains("inspect") && node["inspect"].Value == null;
		}

        /**
         * Loads the given module and puts it into your default container
         */
        protected Node LoadModule(string name)
        {
            Node node = new Node();
            LoadModule(name, null, node);
            return node;
        }

        /**
         * Loads the given module and puts it into the given container. 
         * Will return the node created and passed into creation
         */
        protected Node LoadModule(string name, string container)
        {
            Node node = new Node();
            LoadModule(name, container, node);
            return node;
        }

        /**
         * Shorthand method for Loading a specific Module and putting it into
         * the given container, with the given Node structure.
         */
        protected void LoadModule(string name, string container, Node node)
        {
            if (string.IsNullOrEmpty(name))
                throw new ApplicationException("You have to specify which Module you want to load");

			node = node.Clone();

			node["container"].Value = container;
			node["name"].Value = name;

			RaiseActiveEvent(
				"magix.viewport.load-module", 
				node);
        }

        /**
         * Shorthand for raising events. Will return a node, initially created empty, 
         * but passed onto the Event Handler(s)
         */
        protected static Node RaiseActiveEvent(string eventName)
        {
            Node node = new Node();

			ActiveEvents.Instance.RaiseActiveEvent(
                typeof(ActiveController),
                eventName,
                node);

			return node;
        }

        /**
         * Shorthand for raising events.
         */
        protected static void RaiseActiveEvent(string eventName, Node node)
        {
			RaiseActiveEvent(
				eventName, 
				node, 
				false);;
        }

        /**
         * Shorthand for raising events.
         */
        protected static void RaiseActiveEvent(string eventName, Node node, bool forceNoOverride)
        {
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(ActiveController),
                eventName,
                node,
				forceNoOverride);
        }

        /**
         * Will return the 'base' URL of your application
         */
        protected static string GetApplicationBaseUrl()
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
         * Shorthand for getting access to our "Page" object.
         */
        protected static Page Page
        {
            get
            {
                return HttpContext.Current.Handler as Page;
            }
        }
    }
}
