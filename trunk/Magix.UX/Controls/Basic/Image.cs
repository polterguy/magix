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
    public class Image : BaseWebControl
    {
        /*
         * url of image
         */
        public string ImageUrl
        {
            get { return ViewState["ImageURL"] == null ? "" : (string)ViewState["ImageURL"]; }
            set
            {
                if (value != ImageUrl)
                    SetJsonGeneric("src", value);
                ViewState["ImageURL"] = value;
            }
        }

        /*
         * alternate text
         */
        public string AlternateText
        {
            get { return ViewState["AlternateText"] == null ? "" : (string)ViewState["AlternateText"]; }
            set
            {
                if (value != AlternateText)
                    SetJsonGeneric("alt", value);
                ViewState["AlternateText"] = value;
            }
        }

        /*
         * keyboard shortcut
         */
        public string AccessKey
        {
            get { return ViewState["AccessKey"] == null ? "" : (string)ViewState["AccessKey"]; }
            set
            {
                if (value != AccessKey)
                    SetJsonValue("AccessKey", value);
                ViewState["AccessKey"] = value;
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
            el.AddAttribute("src", ImageUrl);
            el.AddAttribute("alt", AlternateText);
            if (!string.IsNullOrEmpty(AccessKey))
                el.AddAttribute("accesskey", AccessKey);
            base.AddAttributes(el);
        }
    }
}
