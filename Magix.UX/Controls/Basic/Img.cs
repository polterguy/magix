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
     * image ajax control
     */
    public class Img : BaseWebControlFormElement, IValueControl
    {
        /*
         * url of image
         */
        public string Src
        {
            get { return ViewState["Src"] == null ? "" : (string)ViewState["Src"]; }
            set
            {
                if (value != Src)
                    SetJsonGeneric("src", value);
                ViewState["Src"] = value;
            }
        }

        /*
         * alternate text
         */
        public string Alt
        {
            get { return ViewState["Alt"] == null ? "" : (string)ViewState["Alt"]; }
            set
            {
                if (value != Alt)
                    SetJsonGeneric("alt", value);
                ViewState["Alt"] = value;
            }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("img"))
            {
                AddAttributes(el);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("src", Src);
            el.AddAttribute("alt", Alt);
            base.AddAttributes(el);
        }

        protected override void SetValue()
        {
        }

        object IValueControl.ControlValue
        {
            get { return Src; }
            set { Src = value == null ? "" : Convert.ToString(value); }
        }

        bool IValueControl.IsTrueValue
        {
            get { return false; }
        }
    }
}
