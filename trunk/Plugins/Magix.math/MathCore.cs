/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.math
{
	/**
	 * contains the Magix.math main active events
	 */
	public class MathCore : ActiveController
	{
		/**
		 */
        [ActiveEvent(Name = "magix.math.add")]
        public static void magix_math_add(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.math.add"].Value = null;
                e.Params["inspect"].Value = @"Adds the underlaying values together and 
puts the result in value of [magix.math.add] node.&nbsp;&nbsp;thread safe";
                return;
            }

            decimal result = 0M;
            foreach (Node idx in e.Params)
            {
                result += idx.Get<decimal>();
            }
            e.Params.Value = result.ToString();
        }
	}
}

