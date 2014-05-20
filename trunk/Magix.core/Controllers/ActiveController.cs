/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Text;
using System.Web.UI;
using System.Diagnostics;
using System.Collections.Generic;

namespace Magix.Core
{
    /*
     * abstract active controller base class
     */
    [ActiveController]
	public abstract class ActiveController
    {
		/*
		 * return true if we are supposed to inspect the active event, and not execute it
		 */
        protected static bool ShouldInspect(Node node)
        {
            return (node.Contains("inspect") && node["inspect"].Value == null);
        }

        /*
         * loads an active module into the default container
         */
        protected Node LoadActiveModule(string name)
        {
            Node node = new Node();
            LoadActiveModule(name, null, node);
            return node;
        }

        /*
         * loads an active module into the given container
         */
        protected Node LoadActiveModule(string name, string container)
        {
            Node node = new Node();
            LoadActiveModule(name, container, node);
            return node;
        }

        /*
         * loads an active module into the given container with the given nodes as initializing paramaters
         */
        protected void LoadActiveModule(string name, string container, Node node)
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

        /*
         * raises an active event
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

        /*
         * raises an active event with the given node as parameters
         */
        protected static void RaiseActiveEvent(string eventName, Node node)
        {
            ActiveEvents.Instance.RaiseActiveEvent(
                typeof(ActiveController),
                eventName,
                node,
                false);
        }

        /*
         * returns the base url of your web application
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

        /*
         * returns the page object
         */
        protected Page Page
        {
            get
            {
                return HttpContext.Current.Handler as Page;
            }
        }

        /*
         * returns the instruction pointer
         */
        protected static Node Ip(Node pars)
        {
            if (pars.Contains("_ip"))
                return pars["_ip"].Get<Node>();
            return pars;
        }

        /*
         * returns the data pointer
         */
        protected static Node Dp(Node pars)
        {
            if (pars.Contains("_dp"))
                return pars["_dp"].Get<Node>();
            return pars;
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
        protected void AppendInspect(Node node, string value, bool dropInitialHeader)
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
                        builder.Append("</strong>]");
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
         * loads string from resource, html formats string and appends it into the given node
         */
        protected void AppendInspectFromResource(
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
         * loads string from resource, html formats string and appends it into the given node
         */
        protected void AppendInspectFromResource(
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

            string value = Expressions.GetExpressionValue(expression, loadFile["node"], loadFile["node"], false).ToString();
            AppendInspect(destinationNode, value, dropInitialHeader);
        }

        /*
         * loads node from resource, and appends into given node
         */
        protected void AppendCodeFromResource(
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

            Node value = Expressions.GetExpressionValue(expression, loadFile["node"], loadFile["node"], false) as Node;
            destinationNode.AddRange(value);
        }
    }
}
