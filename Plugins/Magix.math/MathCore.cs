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
        // TODO: implement delegates for math expressions, since basically all code is the same, except the operators ...
        /**
         * adds the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.add")]
        public static void magix_math_add(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.math.add"].Value = null;
                e.Params["inspect"].Value = @"adds the underlaying values together and 
puts the result in value of [magix.math.add] node.&nbsp;&nbsp;
[add] nodes can be nested with other magix.math nodes.
&nbsp;&nbsp;thread safe";
                return;
            }

            Node ip = e.Params;
            if (e.Params.Contains("_ip"))
                ip = e.Params["_ip"].Value as Node;

            Node dp = e.Params;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            decimal result = 0M;
            foreach (Node idx in ip)
            {
                // Checking to see if value is null
                if (!string.IsNullOrEmpty(idx.Name))
                {
                    // sub-math expression, or active event
                    string activeEvent = idx.Name;
                    if (activeEvent.IndexOf(".") == -1)
                        activeEvent = "magix.math." + activeEvent;

                    Node pars = new Node();
                    pars["_ip"].Value = idx;
                    pars["_dp"].Value = dp;

                    RaiseActiveEvent(
                        activeEvent,
                        pars);

                    // Adding up sub-expression result
                    result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
                else
                {
                    // number
                    result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
            }
            ip.Value = result;
        }

        /**
         * subtracts the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.subtract")]
        public static void magix_math_subtract(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.math.add"].Value = null;
                e.Params["inspect"].Value = @"subtracts the underlaying values together and 
puts the result in value of [magix.math.add] node.&nbsp;&nbsp;
[add] nodes can be nested with other magix.math nodes.
&nbsp;&nbsp;thread safe";
                return;
            }

            Node ip = e.Params;
            if (e.Params.Contains("_ip"))
                ip = e.Params["_ip"].Value as Node;

            Node dp = e.Params;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            decimal result = 0M;
            bool isFirst = true;
            foreach (Node idx in ip)
            {
                // Checking to see if value is null
                if (!string.IsNullOrEmpty(idx.Name))
                {
                    // sub-math expression or active event
                    string activeEvent = idx.Name;
                    if (activeEvent.IndexOf(".") == -1)
                        activeEvent = "magix.math." + activeEvent;

                    Node pars = new Node();
                    pars["_ip"].Value = idx;
                    pars["_dp"].Value = dp;

                    RaiseActiveEvent(
                        activeEvent,
                        pars);

                    // Adding up sub-expression result
                    if (isFirst)
                    {
                        isFirst = false;
                        result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                    }
                    else
                        result -= Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
                else
                {
                    // number
                    if (isFirst)
                    {
                        isFirst = false;
                        result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                    }
                    else
                        result -= Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
            }
            ip.Value = result;
        }

        /**
         * multiplies the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.multiply")]
        public static void magix_math_multiply(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.math.add"].Value = null;
                e.Params["inspect"].Value = @"multiplies the underlaying values together and 
puts the result in value of [magix.math.add] node.&nbsp;&nbsp;
[add] nodes can be nested with other magix.math nodes.
&nbsp;&nbsp;thread safe";
                return;
            }

            Node ip = e.Params;
            if (e.Params.Contains("_ip"))
                ip = e.Params["_ip"].Value as Node;

            Node dp = e.Params;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            decimal result = 0M;
            bool isFirst = true;
            foreach (Node idx in ip)
            {
                // Checking to see if value is null
                if (!string.IsNullOrEmpty(idx.Name))
                {
                    // sub-math expression or active event
                    string activeEvent = idx.Name;
                    if (activeEvent.IndexOf(".") == -1)
                        activeEvent = "magix.math." + activeEvent;

                    Node pars = new Node();
                    pars["_ip"].Value = idx;
                    pars["_dp"].Value = dp;

                    RaiseActiveEvent(
                        activeEvent,
                        pars);

                    // Adding up sub-expression result
                    if (isFirst)
                    {
                        isFirst = false;
                        result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                    }
                    else
                        result *= Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
                else
                {
                    // number
                    if (isFirst)
                    {
                        isFirst = false;
                        result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                    }
                    else
                        result *= Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
            }
            ip.Value = result;
        }

        /**
         * multiplies the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.divide")]
        public static void magix_math_divide(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.math.add"].Value = null;
                e.Params["inspect"].Value = @"divides the underlaying values together and 
puts the result in value of [magix.math.add] node.&nbsp;&nbsp;
[add] nodes can be nested with other magix.math nodes.
&nbsp;&nbsp;thread safe";
                return;
            }

            Node ip = e.Params;
            if (e.Params.Contains("_ip"))
                ip = e.Params["_ip"].Value as Node;

            Node dp = e.Params;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Value as Node;

            decimal result = 0M;
            bool isFirst = true;
            foreach (Node idx in ip)
            {
                // Checking to see if value is null
                if (!string.IsNullOrEmpty(idx.Name))
                {
                    // sub-math expression or active event
                    string activeEvent = idx.Name;
                    if (activeEvent.IndexOf(".") == -1)
                        activeEvent = "magix.math." + activeEvent;

                    Node pars = new Node();
                    pars["_ip"].Value = idx;
                    pars["_dp"].Value = dp;

                    RaiseActiveEvent(
                        activeEvent,
                        pars);

                    // Adding up sub-expression result
                    if (isFirst)
                    {
                        isFirst = false;
                        result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                    }
                    else
                        result /= Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
                else
                {
                    // number
                    if (isFirst)
                    {
                        isFirst = false;
                        result += Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                    }
                    else
                        result /= Convert.ToDecimal(Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false));
                }
            }
            ip.Value = result;
        }
    }
}

