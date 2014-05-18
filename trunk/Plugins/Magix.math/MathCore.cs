/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using System.Globalization;

namespace Magix.math
{
	/**
	 * contains the magix.math main active events
	 */
	public class MathCore : ActiveController
	{
        private delegate decimal ExecuteFirstExpression(decimal input);
        private delegate decimal ExecuteSecondExpression(decimal input, decimal result);

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

            RunMathExpression(e.Params,
                delegate(decimal input)
                {
                    return input;
                },
                delegate(decimal input, decimal result)
                {
                    return result + input;
                });
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

            RunMathExpression(e.Params,
                delegate(decimal input)
                {
                    return input;
                },
                delegate(decimal input, decimal result)
                {
                    return result - input;
                });
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

            RunMathExpression(e.Params,
                delegate(decimal input)
                {
                    return input;
                },
                delegate(decimal input, decimal result)
                {
                    return result * input;
                });
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

            RunMathExpression(e.Params,
                delegate(decimal input)
                {
                    return input;
                },
                delegate(decimal input, decimal result)
                {
                    return result / input;
                });
        }

        /**
         * modulo the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.modulo")]
        public static void magix_math_modulo(object sender, ActiveEventArgs e)
        {
            if (ShouldInspect(e.Params))
            {
                e.Params["event:magix.math.add"].Value = null;
                e.Params["inspect"].Value = @"divides the underlaying values together and 
returns the remainer in value of [magix.math.add] node.&nbsp;&nbsp;
[add] nodes can be nested with other magix.math nodes.
&nbsp;&nbsp;thread safe";
                return;
            }

            RunMathExpression(e.Params,
                delegate(decimal input)
                {
                    return input;
                },
                delegate(decimal input, decimal result)
                {
                    return result % input;
                });
        }

        /*
         * actual implementation of calculation, with delegates taking operators to be used
         */
        private static void RunMathExpression(Node pars, ExecuteFirstExpression first, ExecuteSecondExpression second)
        {
            Node ip = Ip(pars);
            Node dp = Dp(pars);

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

                    Node parsInner = new Node();
                    parsInner["_ip"].Value = idx;
                    parsInner["_dp"].Value = dp;

                    RaiseActiveEvent(
                        activeEvent,
                        parsInner);

                    // Adding up sub-expression result
                    if (isFirst)
                    {
                        isFirst = false;
                        result = first(
                            Convert.ToDecimal(
                                Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false), 
                                CultureInfo.InvariantCulture));
                    }
                    else
                        result = second(
                            Convert.ToDecimal(
                                Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false), 
                                CultureInfo.InvariantCulture), 
                            result);
                }
                else
                {
                    // number
                    if (isFirst)
                    {
                        isFirst = false;
                        result = first(
                            Convert.ToDecimal(
                                Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false), 
                                CultureInfo.InvariantCulture));
                    }
                    else
                        result = second(
                            Convert.ToDecimal(
                                Expressions.GetExpressionValue(idx.Get<string>(), dp, ip, false), 
                                CultureInfo.InvariantCulture), 
                            result);
                }
            }
            ip.Value = result;
        }
    }
}

