/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.UX.Helpers
{
    public static class StringHelper
    {
        public static string ConditionalAdd(
            string original, 
            string ifEmpty, 
            string ifNotEmpty, 
            string atEnd)
        {
            if (string.IsNullOrEmpty(original))
                return ifEmpty + atEnd;
            if (string.IsNullOrEmpty(atEnd))
                return original + ifEmpty;
            return original + ifNotEmpty + atEnd;
        }
    }
}
