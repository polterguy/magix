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
     * ajax hyperlink control
     */
    public class LinkButton : BaseWebControlFormElementText, IValueControl
    {
        /*
         * anchor text
         */
        public override string Value
        {
            get { return ViewState["Value"] == null ? "" : (string)ViewState["Value"]; }
            set
            {
                if (value != Value)
                    SetJsonValue("InnerHtml", value);
                ViewState["Value"] = value;
            }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("a"))
            {
                AddAttributes(el);
                el.Write(Value);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("href", "javascript:MUX.emp();");
            base.AddAttributes(el);
        }

        protected override void SetValue()
        { }

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
