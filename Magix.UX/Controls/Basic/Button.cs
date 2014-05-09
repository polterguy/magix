/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
    public class Button : BaseWebControlFormElementText
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
    }
}
