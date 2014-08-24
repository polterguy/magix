/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;
using System.Globalization;

namespace Magix.execute
{
	/**
     * helper for checking statements, such as [if]/[if-else] and [while]
	 */
    public static class StatementHelper
	{
        /**
         * checks and expression to see if it evaluates as true
         */
        public static bool CheckExpressions(Node ip, Node dp)
        {
            return RecursivelyCheckExpression(ip, ip, dp);
        }

        private static bool RecursivelyCheckExpression(Node where, Node ip, Node dp)
        {
            string expressionOperator = Expressions.GetExpressionValue<string>(where.Get<string>(), dp, ip, false);
            object objLhsVal;
            object objRhsVal;
            ExtractValues(where, ip, dp, expressionOperator, out objLhsVal, out objRhsVal);

            bool retVal = RunComparison(expressionOperator, objLhsVal, objRhsVal);

            if (retVal && where.Contains("and"))
            {
                foreach (Node idx in where)
                {
                    if (idx.Name == "and")
                    {
                        retVal = RecursivelyCheckExpression(idx, ip, dp);
                        if (!retVal)
                            break;
                    }
                }
            }
            if (!retVal && where.Contains("or"))
            {
                foreach (Node idx in where)
                {
                    if (idx.Name == "or")
                    {
                        retVal = RecursivelyCheckExpression(idx, ip, dp);
                        if (retVal)
                            break;
                    }
                }
            }

            return retVal;
        }

        private static void ExtractValues(
            Node where,
            Node ip,
            Node dp,
            string expressionOperator,
            out object objLhsVal,
            out object objRhsVal)
        {
            string lhsRawValue = where["lhs"].Get<string>();
            if (where["lhs"].Count > 0)
                lhsRawValue = Expressions.FormatString(dp, ip, where["lhs"], lhsRawValue);
            string rhsRawValue = null;
            objLhsVal = null;
            objRhsVal = null;

            ChangeType(lhsRawValue, out objLhsVal, ip, dp);

            if (expressionOperator != "exist" && expressionOperator != "not-exist")
            {
                if (!where.Contains("rhs"))
                    throw new ArgumentException("missing [rhs] node in expression");

                rhsRawValue = where["rhs"].Get<string>();
                if (where["rhs"].Count > 0)
                    rhsRawValue = Expressions.FormatString(dp, ip, where["rhs"], rhsRawValue);
                ChangeType(rhsRawValue, out objRhsVal, ip, dp);
            }
        }

        private static void ChangeType(
            string stringValue,
            out object objectValue,
            Node ip,
            Node dp)
        {
            object objectValueOfExpression = Expressions.GetExpressionValue<object>(stringValue, dp, ip, false);
            if (!(objectValueOfExpression is string))
            {
                objectValue = objectValueOfExpression;
                return;
            }

            string valueOfExpression = objectValueOfExpression as string;
            if (valueOfExpression == null)
            {
                objectValue = null;
                return;
            }

            // trying to convert the thing to integer
            int intResult;
            bool successfulConvertion = int.TryParse(valueOfExpression, out intResult);
            if (successfulConvertion)
            {
                objectValue = intResult;
                return;
            }

            // trying to convert the thing to decimal
            decimal decimalResult;
            successfulConvertion = decimal.TryParse(
                valueOfExpression,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out decimalResult);
            if (successfulConvertion)
            {
                objectValue = decimalResult;
                return;
            }

            // trying to convert the thing to DateTime
            DateTime dateResult;
            successfulConvertion = DateTime.TryParseExact(
                valueOfExpression,
                "yyyy.MM.dd HH:mm:ss",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out dateResult);
            if (successfulConvertion)
            {
                objectValue = dateResult;
                return;
            }

            // trying to convert the thing to bool
            bool boolResult;
            successfulConvertion = bool.TryParse(valueOfExpression, out boolResult);
            if (successfulConvertion)
            {
                objectValue = boolResult;
                return;
            }

            // Couldn't convert the freaking thing to anything ...
            objectValue = valueOfExpression;
        }

        private static bool RunComparison(string expressionOperator, object objLhsVal, object objRhsVal)
        {
            bool expressionIsTrue = false;

            switch (expressionOperator)
            {
                case "exist":
                    expressionIsTrue = objLhsVal != null;
                    break;
                case "not-exist":
                    expressionIsTrue = objLhsVal == null;
                    break;
                case "equals":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) == 0;
                    break;
                case "not-equals":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) != 0;
                    break;
                case "more-than":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) == 1;
                    break;
                case "less-than":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) == -1;
                    break;
                case "more-than-equals":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) != -1;
                    break;
                case "less-than-equals":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) != 1;
                    break;
                default:
                    throw new ArgumentException("tried to pass in a comparison operator which doesn't exist, operator was; " + expressionOperator);
            }
            return expressionIsTrue;
        }

        private static int CompareValues(object objLhsValue, object objRhsValue)
        {
            if (objLhsValue != null && objRhsValue == null)
                return 1;
            if (objLhsValue == null && objRhsValue != null)
                return -1;
            if (objLhsValue == null && objRhsValue == null)
                return 0;

            switch (objLhsValue.GetType().ToString())
            {
                case "System.Int32":
                    if (objRhsValue.GetType() == typeof(int))
                        return ((int)objLhsValue).CompareTo(objRhsValue);
                    int tmpVal;
                    if (!int.TryParse(objRhsValue.ToString(), out tmpVal))
                        return (objLhsValue.ToString()).CompareTo(objRhsValue.ToString());
                    return ((int)objLhsValue).CompareTo(tmpVal);
                case "System.Decimal":
                    if (objRhsValue.GetType() == typeof(decimal))
                        return ((decimal)objLhsValue).CompareTo(objRhsValue);
                    decimal tmpVal2;
                    if (!decimal.TryParse(objRhsValue.ToString(), NumberStyles.None, CultureInfo.InvariantCulture, out tmpVal2))
                        return (objLhsValue.ToString()).CompareTo(objRhsValue.ToString());
                    return ((decimal)objLhsValue).CompareTo(tmpVal2);
                case "System.DateTime":
                    if (objRhsValue.GetType() == typeof(DateTime))
                        return ((DateTime)objLhsValue).CompareTo(objRhsValue);
                    DateTime tmpVal3;
                    if (!DateTime.TryParseExact(objRhsValue.ToString(), "yyyy.MM.dd hh:MM:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out tmpVal3))
                        return (objLhsValue.ToString()).CompareTo(objRhsValue.ToString());
                    return ((DateTime)objLhsValue).CompareTo(tmpVal3);
                case "System.Boolean":
                    if (objRhsValue.GetType() == typeof(bool))
                        return ((bool)objLhsValue).CompareTo(objRhsValue);
                    bool tmpVal4;
                    if (!bool.TryParse(objRhsValue.ToString(), out tmpVal4))
                        return (objLhsValue.ToString()).CompareTo(objRhsValue.ToString());
                    return ((bool)objLhsValue).CompareTo(tmpVal4);
                case "Magix.Core.Node":
                    return ((Node)objLhsValue).Equals(objRhsValue) ? 0 : -1;
                default:
                    return (objLhsValue.ToString()).CompareTo(objRhsValue.ToString());
            }
        }
    }
}
