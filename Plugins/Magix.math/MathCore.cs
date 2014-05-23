/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using System.Globalization;

namespace Magix.math
{
	/*
	 * contains the magix.math main active events
	 */
	public class MathCore : ActiveController
	{
        private delegate decimal ExecuteFirstExpression(decimal input);
        private delegate decimal ExecuteSecondExpression(decimal input, decimal result);

        /*
         * adds the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.add")]
        public static void magix_math_add(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.add-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.add-sample]");
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

        /*
         * subtracts the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.subtract")]
        public static void magix_math_subtract(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.subtract-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.subtract-sample]");
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

        /*
         * multiplies the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.multiply")]
        public static void magix_math_multiply(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.multiply-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.multiply-sample]");
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

        /*
         * divides the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.divide")]
        public static void magix_math_divide(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.divide-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.divide-sample]");
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

        /*
         * modulo the values of all underlaying nodes
         */
        [ActiveEvent(Name = "magix.math.modulo")]
        public static void magix_math_modulo(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.modulo-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.math",
                    "Magix.math.hyperlisp.inspect.hl",
                    "[magix.math.modulo-sample]");
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
                // Checking to see if name is not null
                if (!string.IsNullOrEmpty(idx.Name))
                {
                    pars["_ip"].Value = idx;
                    pars["_root-only-execution"].Value = true;
                    try
                    {
                        RaiseActiveEvent(
                            "magix.execute",
                            pars);
                    }
                    finally
                    {
                        pars["_root-only-execution"].UnTie();
                    }
                }

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
            ip.Value = result;
        }
    }
}

