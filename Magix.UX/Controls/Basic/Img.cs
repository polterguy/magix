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
     * image ajax control
     */
    public class Img : BaseWebControlFormElement
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
            if (!string.IsNullOrEmpty(AccessKey))
                el.AddAttribute("accesskey", AccessKey);
            base.AddAttributes(el);
        }

        protected override void SetValue()
        {
        }
    }
}
