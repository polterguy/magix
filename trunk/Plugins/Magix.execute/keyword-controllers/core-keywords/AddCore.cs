/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
    /*
     * hyperlisp add logic
     */
    internal sealed class AddCore : ActiveController
    {
        /*
         * add hyperlisp keyword
         */
        [ActiveEvent(Name = "magix.execute.add")]
        public static void magix_execute_add(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.add-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.add-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            Node destinationNode = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, true);
            if (destinationNode == null)
                throw new HyperlispExecutionErrorException("[add] must return an existing node-list, [add] value returned null, expression was; " + ip.Get<string>());

            if (ip.ContainsValue("value"))
                AddSingleValue(ip, dp, destinationNode);
            else if (ip.ContainsValue("values"))
                AddMultipleValues(ip, dp, destinationNode);
            else
                destinationNode.AddRange(ip.Clone());
        }

        /*
         * adds a single value to destination node
         */
        private static void AddSingleValue(Node ip, Node dp, Node destinationNode)
        {
            object sourceObject = Expressions.GetExpressionValue<object>(ip["value"].Get<string>(), dp, ip, false);
            Node sourceObjectAsNode = sourceObject as Node;
            if (sourceObjectAsNode != null)
                destinationNode.Add(sourceObjectAsNode.Clone());
            else
            {
                string nodeName = sourceObject.ToString();
                object nodeValue = null;
                if (ip["value"].ContainsValue("value"))
                {
                    nodeValue = Expressions.GetExpressionValue<object>(ip["value"]["value"].Get<string>(), dp, ip, false);
                    if (ip["value"]["value"].Count > 0)
                        nodeValue = Expressions.FormatString(dp, ip, ip["value"]["value"], nodeValue.ToString());
                }
                destinationNode.Add(new Node(nodeName, nodeValue));
            }
        }

        /*
         * adds all children from source into destination
         */
        private static void AddMultipleValues(Node ip, Node dp, Node destinationNode)
        {
            object sourceObject = Expressions.GetExpressionValue<object>(ip["values"].Get<string>(), dp, ip, false);
            Node sourceObjectAsNode = sourceObject as Node;
            if (sourceObjectAsNode != null)
                destinationNode.AddRange(sourceObjectAsNode.Clone());
            else
                throw new HyperlispExecutionErrorException("[values] didn't return nodes in [add], expression was; '" + ip["values"].Get<string>() + "'");
        }
    }
}

