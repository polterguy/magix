/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Helpers;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /*
     * hyperlink ajax control
     */
    public class HyperLink : BaseWebControlFormElement, IValueControl
    {
        /*
         * anchor text
         */
        public string Value
        {
            get { return ViewState["Value"] == null ? "" : (string)ViewState["Value"]; }
            set
            {
                if (value != Value)
                    SetJsonValue("InnerHtml", value);
                ViewState["Value"] = value;
            }
        }

        /*
         * url
         */
        public string Href
        {
            get { return ViewState["Href"] == null ? "" : (string)ViewState["Href"]; }
            set
            {
                if (value != Value)
                    SetJsonGeneric("href", value);
                ViewState["Href"] = value;
            }
        }

        /*
         * target browser window
         */
        public string Target
        {
            get { return ViewState["Target"] == null ? "" : (string)ViewState["Target"]; }
            set
            {
                if (value != Target)
                    SetJsonGeneric("target", value);
                ViewState["Target"] = value;
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
            el.AddAttribute("href", Href);
            if (!string.IsNullOrEmpty(Target))
                el.AddAttribute("target", Target);
            base.AddAttributes(el);
        }

        protected override void SetValue()
        { }

        object IValueControl.ControlValue
        {
            get { return Value; }
            set { Value = value == null ? "" : Convert.ToString(value); }
        }

        bool IValueControl.IsTrueValue
        {
            get { return false; }
        }
    }
}
