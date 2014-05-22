/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * hyperlisp lambda keyword
	 */
	public class LambdaCore : ActiveController
	{
		/*
		 * hyper lisp lambda keyword
		 */
		[ActiveEvent(Name = "magix.execute.lambda")]
		public static void magix_execute_lambda(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params, true);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.lambda-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.execute",
                    "Magix.execute.hyperlisp.inspect.hl",
                    "[magix.execute.lambda-sample]");
                return;
			}

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

            ip.Clear();
            ip.AddRange(lambdaCodeBlock["$"]);
        }
	}
}

