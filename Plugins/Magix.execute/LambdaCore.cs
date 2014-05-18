/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * hyper lisp lambda keyword
	 */
	public class LambdaCore : ActiveController
	{
		/**
		 * hyepr lisp lambda keyword
		 */
		[ActiveEvent(Name = "magix.execute.lambda")]
		public static void magix_execute_lambda(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                ip["inspect"].Value = @"<p>executes the expression in the 
[lambda] node creating a deep copy of the expression's nodes</p><p>[lambda] can 
be given parameters, which will be accessible in the lambda code through the [$] 
collection.&nbsp;&nbsp;anything you return as [$] from inside the code being 
executed, will become directly accessible as children nodes from outside of the 
lambda execution, after the execution is finished</p><p>thread safe</p>";
                ip["_data"]["set"].Value = "[$][output].Value";
                ip["_data"]["set"]["value"].Value = "{0} {1}";
                ip["_data"]["set"]["value"]["v0"].Value = "[$][input].Value";
                ip["_data"]["set"]["value"]["v1"].Value = "hansen";
                ip["lambda"].Value = "[_data]";
                ip["lambda"]["input"].Value = "thomas";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [magix.execute.lambda] directly, except for inspect purposes");

            Node dp = Dp(e.Params);

			string lambdaExpression = ip.Get<string>();
            Node lambdaCodeBlock = (Expressions.GetExpressionValue(lambdaExpression, dp, ip, false) as Node);

            if (lambdaCodeBlock == null)
                throw new ArgumentException("[lambda] couldn't find a block of code to execute from its expression");

            lambdaCodeBlock = lambdaCodeBlock.Clone();

            foreach (Node idx in ip)
            {
                lambdaCodeBlock["$"].Add(idx);
            }

            RaiseActiveEvent(
                "magix.execute",
                lambdaCodeBlock);

            ip.ReplaceChildren(lambdaCodeBlock["$"]);
        }
	}
}

