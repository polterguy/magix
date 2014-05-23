/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.UX.Widgets.Core
{
    /*
     * controls which have a "value" implements this interface
     */
    public interface IValueControl
    {
        object ControlValue { get; set; }
    }
}
