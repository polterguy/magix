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
    /*
     * exception type thrown when [throw] keyword is invoked
     */
    public class HyperlispSyntaxErrorException : Exception
    {
        public HyperlispSyntaxErrorException(string msg)
            : base(msg)
        { }
    }
}
