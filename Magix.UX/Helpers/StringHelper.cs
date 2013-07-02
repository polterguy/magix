/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
