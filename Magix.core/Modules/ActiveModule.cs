﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Text;
using System.Web.UI;
using System.Threading;
using System.Diagnostics;

namespace Magix.Core
{
    /*
     * base class for active modules
     */
	[ActiveModule]
    public abstract class ActiveModule : UserControl
    {
        private bool _firstLoad;

        /*
         * called upon first load of module to initialize
         */
        public virtual void InitialLoading(Node node)
        {
            _firstLoad = true;
        }

        /*
         * true if this was initial loading of module
         */
        protected bool FirstLoad
        {
            get { return _firstLoad; }
        }

        /*
         * helper for raising active events
         */
        protected static Node RaiseActiveEvent(string eventName)
        {
            Node node = new Node();
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(ActiveModule),
                eventName,
                node);
            return node;
        }

        /*
         * helper for raising active events
         */
        protected static void RaiseActiveEvent(string eventName, Node node)
        {
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(ActiveModule),
                eventName,
                node);
        }

        /*
         * returns the base url of your website
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

        /*
         * returns the instruction pointer. will by default return the given node if no _ip is found in node
         */
        protected static Node Ip(Node pars)
        {
            if (pars.Contains("_ip"))
                return pars["_ip"].Get<Node>();
            return pars;
        }

        /*
         * returns the data pointer. will by default return the given node if no _dp is found in node
         */
        protected static Node Dp(Node pars)
        {
            if (pars.Contains("_dp"))
                return pars["_dp"].Get<Node>();
            return pars;
        }

        /*
         * return true if we are supposed to inspect the active event, and not execute it
         */
        protected static bool ShouldInspect(Node node)
        {
            return node.Contains("inspect");
        }

        /*
         * html formats and appends the given value to the given node's value
         */
        protected void AppendInspect(Node node, string value)
        {
            AppendInspect(node, value, false);
        }

        /*
         * html formats and appends the given value to the given node's value
         */
        protected static void AppendInspect(Node node, string value, bool dropInitialHeader)
        {
            StringBuilder builder = new StringBuilder(node.Get<string>());
            if (!dropInitialHeader)
                builder.Append("<p><h3>");
            bool hasClosedH3 = false;

            char lastChar = char.MinValue, secondLastChar = char.MinValue;
            foreach (char idxChar in value)
            {
                switch (idxChar)
                {
                    case '\r':
                    case '\t':
                        continue;
                    case '\n':
                        if (lastChar == '\n')
                        {
                            builder.Remove(builder.Length - 1, 1);
                            if (!hasClosedH3 && !dropInitialHeader)
                            {
                                hasClosedH3 = true;
                                builder.Append("</h3>");
                            }
                            builder.Append("</p><p>");
                        }
                        else
                            builder.Append(' ');
                        break;
                    case '[':
                        builder.Append("<strong>[");
                        break;
                    case ']':
                        builder.Append("]</strong>");
                        break;
                    case ' ':
                        if (lastChar == ' ' && secondLastChar == '.')
                        {
                            builder.Remove(builder.Length - 1, 1);
                            builder.Append("&nbsp;&nbsp;");
                        }
                        else
                            builder.Append(" ");
                        break;
                    default:
                        builder.Append(idxChar);
                        break;
                }
                secondLastChar = lastChar;
                lastChar = idxChar;
            }
            builder.Append("</p>");
            node.Value = builder.ToString();
        }

        /*
         * loads string from resource, html formats string, and appends it into the given node
         */
        protected static void AppendInspectFromResource(
            Node destinationNode,
            string assemblyName,
            string resourceName,
            string expression)
        {
            AppendInspectFromResource(
                destinationNode,
                assemblyName,
                resourceName,
                expression,
                false);
        }

        /*
         * loads string from resource, html formats string, and appends it into the given node
         */
        protected static void AppendInspectFromResource(
            Node destinationNode,
            string assemblyName,
            string resourceName,
            string expression,
            bool dropInitialHeader)
        {
            Node loadFile = new Node();
            loadFile["file"].Value = "plugin:magix.file.load-from-resource";
            loadFile["file"]["assembly"].Value = assemblyName;
            loadFile["file"]["resource-name"].Value = resourceName;
            RaiseActiveEvent(
                "magix.execute.code-2-node",
                loadFile);

            string value = Expressions.GetExpressionValue<string>(expression, loadFile["node"], loadFile["node"], false);
            AppendInspect(destinationNode, value, dropInitialHeader);
        }

        /*
         * loads node from resource, and appends into given node
         */
        protected static void AppendCodeFromResource(
            Node destinationNode,
            string assemblyName,
            string resourceName,
            string expression)
        {
            Node loadFile = new Node();
            loadFile["file"].Value = "plugin:magix.file.load-from-resource";
            loadFile["file"]["assembly"].Value = assemblyName;
            loadFile["file"]["resource-name"].Value = resourceName;
            RaiseActiveEvent(
                "magix.execute.code-2-node",
                loadFile);

            Node value = Expressions.GetExpressionValue<Node>(expression, loadFile["node"], loadFile["node"], false);
            destinationNode.AddRange(value);
        }
    }
}
