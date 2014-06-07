/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /*
     * button ajax control
     */
    public class Button : BaseWebControlFormElementText, IValueControl
    {
        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("input"))
            {
                AddAttributes(el);
            }
        }

        protected override void SetValue()
        { }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("type", "button");
            el.AddAttribute("value", Value);
            base.AddAttributes(el);
        }

        object IValueControl.ControlValue
        {
            get { return Value; }
            set { Value = Convert.ToString(value); }
        }

        bool IValueControl.IsTrueValue
        {
            get { return false; }
        }
    }
}
