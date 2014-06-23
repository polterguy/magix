/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
            return node.Contains("inspect");
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

            Node ip = Ip(node);
			ip["container"].Value = container;
			ip["name"].Value = name;

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
        protected static string GetApplicationBaseUrl()
        {
            return string.Format(
                "{0}://{1}{2}",
                HttpContext.Current.Request.Url.Scheme,
                HttpContext.Current.Request.ServerVariables["HTTP_HOST"],
                (HttpContext.Current.Request.ApplicationPath.Equals("/")) ? 
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
            return Ip(pars, false);
        }

        /*
         * returns the instruction pointer
         */
        protected static Node Ip(Node pars, bool mandatoryIp)
        {
            if (mandatoryIp && !pars.Contains("inspect") && (!pars.Contains("_ip") || !(pars["_ip"].Value is Node)))
                throw new ApplicationException("tried to access an active event where the instruction pointer was mandatory, with no ip, and no inspect");
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
        protected static void AppendInspect(Node node, string value)
        {
            AppendInspect(node, value, false);
        }

        /*
         * html formats and appends the given value to the given node's value
         */
        protected static void AppendInspect(Node node, string value, bool dropInitialHeader)
        {
            if (!dropInitialHeader)
                dropInitialHeader = node.ContainsValue("type") && node["type"].Get("") == "drop-header";
            bool dropHtml = node.ContainsValue("type") && node["type"].Get("") == "no-html";

            int noCharsSinceCR = 0;
            StringBuilder builder = new StringBuilder(node.Get<string>());
            if (!dropInitialHeader || dropHtml)
            {
                noCharsSinceCR += 4;
                builder.Append("<h3>");
            }
            else if (!dropHtml)
            {
                noCharsSinceCR += 3;
                builder.Append("<p>");
            }
            bool hasClosedH3 = false;

            char lastChar = char.MinValue, secondLastChar = char.MinValue;
            foreach (char idxChar in value)
            {
                noCharsSinceCR += 1;
                switch (idxChar)
                {
                    case '\r':
                    case '\t':
                        continue;
                    case '\n':
                        if (!dropHtml && lastChar == '\n')
                        {
                            builder.Remove(builder.Length - 1, 1);
                            if (!hasClosedH3 && !dropInitialHeader)
                            {
                                hasClosedH3 = true;
                                builder.Append("</h3>\r\n<p>");
                                noCharsSinceCR = 3;
                            }
                            else
                            {
                                noCharsSinceCR = 3;
                                builder.Append("</p>\r\n<p>");
                            }
                        }
                        else
                        {
                            if (noCharsSinceCR >= 33)
                            {
                                builder.Append("\n");
                                noCharsSinceCR = 0;
                            }
                            else
                                builder.Append(' ');
                        }
                        break;
                    case '[':
                        if (!dropHtml)
                        {
                            noCharsSinceCR += 8;
                            builder.Append("<strong>[");
                        }
                        else
                            builder.Append("[");
                        break;
                    case ']':
                        if (!dropHtml)
                        {
                            noCharsSinceCR += 9;
                            builder.Append("]</strong>");
                        }
                        else
                            builder.Append("]");
                        break;
                    case ' ':
                        if (noCharsSinceCR >= 33)
                        {
                            builder.Append("\n");
                            noCharsSinceCR = 0;
                        }
                        else if (lastChar == ' ' && secondLastChar == '.')
                        {
                            builder.Remove(builder.Length - 1, 1);
                            if (!dropHtml)
                            {
                                noCharsSinceCR += 12;
                                builder.Append("&nbsp;&nbsp;");
                            }
                            else
                                builder.Append("  ");
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
            if (!dropHtml)
                builder.Append("</p>");
            node.Value = builder.ToString().Replace("<p></p>", "");
        }

        /*
         * loads string from resource, html formats string and appends it into the given node
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
         * loads string from resource, html formats string and appends it into the given node
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
