/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /**
     * Technically this widget is completely redundant towards the Image Ajax Widget. But
     * it is here for cases where you want to be semantically highly correct and need
     * something that gives clues in regards to that it is 'clickable'. If you have
     * an Image which is clickable, then semantically this widget is more correct to use.
     */
    public class ImageButton : BaseWebControlFormElement
    {
        /**
         * The URL of where your image is. This is the image that will be displayed to
         * the end user.
         */
        public string ImageUrl
        {
            get { return ViewState["ImageUrl"] == null ? "" : (string)ViewState["ImageUrl"]; }
            set
            {
                if (value != ImageUrl)
                    SetJsonGeneric("src", value);
                ViewState["ImageUrl"] = value;
            }
        }

        /**
         * This is the text that will be displayed if the link to the image is broken.
         * It is also the text that most screen-readers will read up loud to the end
         * user.
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

        protected override void SetValue()
        { }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("input"))
            {
                AddAttributes(el);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("onclick", "return false;");
            el.AddAttribute("type", "image");
            el.AddAttribute("src", ImageUrl);
            el.AddAttribute("alt", AlternateText);
            base.AddAttributes(el);
        }
    }
}
