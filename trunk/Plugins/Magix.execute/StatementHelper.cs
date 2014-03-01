/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
            string oper = ip.Get<string>();
            object objLhsVal;
            object objRhsVal;
            ExtractValues(ip, dp, oper, out objLhsVal, out objRhsVal);
            return RunComparison(oper, objLhsVal, objRhsVal);
        }

        private static void ExtractValues(
            Node ip,
            Node dp,
            string oper,
            out object objLhsVal,
            out object objRhsVal)
        {
            string lhsRawValue = ip["lhs"].Get<string>();
            string rhsRawValue = null;
            objLhsVal = null;
            objRhsVal = null;

            ChangeType(lhsRawValue, out objLhsVal, ip, dp);

            if (oper != "exist" && oper != "not-exist")
            {
                if (!ip.Contains("rhs"))
                    throw new ArgumentException("missing [rhs] node in expression");

                rhsRawValue = ip["rhs"].Get<string>();
                ChangeType(rhsRawValue, out objRhsVal, ip, dp);
            }
            else if (ip.Contains("rhs"))
                throw new ArgumentException("meaningless [rhs] node in expression since operator is 'exist' or 'not-exist'");
        }

        private static void ChangeType(
            string stringValue,
            out object objectValue,
            Node ip,
            Node dp)
        {
            object objectValueOfExpression = Expressions.GetExpressionValue(stringValue, dp, ip, false);
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

        private static bool RunComparison(string oper, object objLhsVal, object objRhsVal)
        {
            bool expressionIsTrue = false;

            switch (oper)
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
                case "more-then":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) == 1;
                    break;
                case "less-then":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) == -1;
                    break;
                case "more-then-equals":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) != -1;
                    break;
                case "less-then-equals":
                    expressionIsTrue = CompareValues(objLhsVal, objRhsVal) != 1;
                    break;
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
                    return ((int)objLhsValue).CompareTo(objRhsValue);
                case "System.Decimal":
                    return ((decimal)objLhsValue).CompareTo(objRhsValue);
                case "System.DateTime":
                    return ((DateTime)objLhsValue).CompareTo(objRhsValue);
                case "System.Boolean":
                    return ((bool)objLhsValue).CompareTo(objRhsValue);
                default:
                    return (objLhsValue.ToString()).CompareTo(objRhsValue);
            }
        }
    }
}
