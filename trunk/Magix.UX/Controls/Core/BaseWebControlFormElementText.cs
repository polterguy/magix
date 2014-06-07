/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.ComponentModel;
using Magix.UX.Builder;

namespace Magix.UX.Widgets.Core
{
    /*
     * classs for for elements with text
     */
    public abstract class BaseWebControlFormElementText : BaseWebControlFormElement
    {
        /*
         * text of control
         */
        public virtual string Value
        {
            get { return ViewState["Value"] == null ? "" : (string)ViewState["Value"]; }
            set
            {
                if (value != Value)
                    SetJsonGeneric("value", value);
                ViewState["Value"] = value;
            }
        }
    }
}
