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
	 * hyper lisp join keyword
	 */
	public class JoinCore : ActiveController
	{
		/**
		 * hyepr lisp join keyword
		 */
		[ActiveEvent(Name = "magix.execute.join")]
		public static void magix_execute_join(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>joins the given expression's nodes, 
and puts the result into the [result] return node as value</p><p>thread safe</p>";
                e.Params["_data"]["", 0].Value = "some ";
                e.Params["_data"]["", 1].Value = "text ";
                e.Params["_data"]["", 2].Value = "that ";
                e.Params["_data"]["", 3].Value = "will ";
                e.Params["_data"]["", 4].Value = "be ";
                e.Params["_data"]["", 5].Value = "joined!";
                e.Params["join"].Value = "[_data]";
				return;
			}

			if (!e.Params.Contains("_ip") || !(e.Params["_ip"].Value is Node))
				throw new ArgumentException("you cannot raise [join] directly, except for inspect purposes");

			Node ip = e.Params["_ip"].Value as Node;

			Node dp = e.Params["_dp"].Value as Node;

			string left = ip.Get<string>();
            Node whatToJoin = Expressions.GetExpressionValue(left, dp, ip, false) as Node;
            if (whatToJoin == null)
                throw new ArgumentException("couldn't make '" + ip.Get<string>() + "' into a node expression in [join]");

            string result = "";
            foreach (Node idx in whatToJoin)
            {
                result += idx.Get<string>();
            }
            ip["result"].Value = result;
		}
	}
}

