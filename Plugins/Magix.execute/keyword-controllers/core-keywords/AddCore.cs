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
	public class AddCore : ActiveController
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
                throw new ArgumentException("[add] must return an existing node-list, [add] value returned null, expression was; " + ip.Get<string>());

            if (ip.ContainsValue("value"))
            {
                object sourceObject = Expressions.GetExpressionValue<object>(ip["value"].Get<string>(), dp, ip, false);

                if (sourceObject is Node)
                    destinationNode.Add((sourceObject as Node).Clone());
                else
                {
                    string nodeName = sourceObject.ToString();

                    object nodeValue = null;
                    if (ip["value"].Contains("value"))
                        nodeValue = Expressions.GetExpressionValue<object>(ip["value"]["value"].Get<string>(), dp, ip, false);
                    destinationNode.Add(new Node(nodeName, nodeValue));
                }
            }
            else if (ip.ContainsValue("values"))
            {
                object sourceObject = Expressions.GetExpressionValue<object>(ip["values"].Get<string>(), dp, ip, false);

                if (sourceObject is Node)
                {
                    Node sourceNode = sourceObject as Node;
                    destinationNode.AddRange(sourceNode.Clone());
                }
                else
                    throw new ArgumentException("[values] didn't return nodes in [add]");
            }
            else
            {
                destinationNode.AddRange(ip.Clone());
            }
		}
    }
}

