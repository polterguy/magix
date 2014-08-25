/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.execute
{
    /**
     * for-each hyperlisp keyword
     */
    public class ForEachCore : ActiveController
    {
        /**
         * for-each hyperlisp keyword
         */
        [ActiveEvent(Name = "magix.execute.for-each")]
        public static void magix_execute_for_each(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.for-each-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.for-each-sample]");
                return;
            }

            Node dp = Dp(e.Params);

            if (string.IsNullOrEmpty(ip.Get<string>()))
                throw new HyperlispSyntaxErrorException("you must supply an expression to [for-each]");

            Node nodeList = Expressions.GetExpressionValue<Node>(ip.Get<string>(), dp, ip, false);

            if (nodeList != null && nodeList.Count > 0)
                IterateNodeListChecked(e.Params, ip, nodeList);
        }

        /*
         * actual implementation of for-each loop, wrapping execution inside of try/catch/finally blocks
         */
        private static void IterateNodeListChecked(Node pars, Node ip, Node nodeList)
        {
            object oldDp = pars["_dp"].Value;
            try
            {
                IterateNodeListUnChecked(pars, ip, nodeList);
            }
            catch (Exception err)
            {
                // checking to see if we should rethrow exception, or if this was a [stop] keyword
                if (ShouldRethrow(err))
                    throw;
            }
            finally
            {
                pars["_dp"].Value = oldDp;
            }
        }

        /*
         * actual iteration of node list
         */
        private static void IterateNodeListUnChecked(Node pars, Node ip, Node nodeList)
        {
            // iterating as long as we have items in our data source
            Node iterationNode = nodeList[0];
            while (iterationNode != null)
            {
                // saving ip, to make [for-each] "immutable"
                Node originalIp = ip.Clone();

                // fetching next node before invocation, in case [for-each] body removes the iterated node from the node list
                Node nextIdx = iterationNode.Next();

                // invokes [for-each] body for current node
                ExecuteForEachCodeBlock(pars, iterationNode);

                // cleaning up [for-each] body, to make sure it's "immutable"
                ip.Clear();
                ip.AddRange(originalIp);

                // advancing the iteration pointer
                iterationNode = nextIdx;
            }
        }

        /*
         * invokes code block for one node within our data source
         */
        private static void ExecuteForEachCodeBlock(Node pars, Node iterationNode)
        {
            pars["_dp"].Value = iterationNode;
            RaiseActiveEvent(
                "magix.execute",
                pars);
        }

        /*
         * checks to see if this is a [stop] keyword, at which point we "swallow" the exception
         */
        private static bool ShouldRethrow(Exception err)
        {
            while (err.InnerException != null)
                err = err.InnerException;

            if (err is StopCore.HyperLispStopException)
                return false; // do nothing, execution stopped

            // re-throw all other exceptions ...
            return true;
        }
    }
}

