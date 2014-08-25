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
     * exception type thrown when [stop] keyword is executed
     */
    public class HyperlispStopException : Exception
    {
        public HyperlispStopException()
        { }
    }
}
