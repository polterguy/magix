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
    /**
     * Image Ajax Widget. Useful for showing images that needs Ajax 
     * functionality somehow. Notice that most times it's more efficient
     * to display other types of widgets, such as the Panel or a Label
     * and set it to display an image through using something such as 
     * background-image through CSS or something similar. 
     */
    public class Image : BaseWebControl
    {
        /**
         * The URL of where your image is. This is the image that will be displayed to
         * the end user.
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

        /**
         * What keyboard shortcut the user will have to use to mimick a 'click' on 
         * the widget. Often the keyboard shortcut will be mixed up with other
         * keys, depending upon your operating system or browser. For FireFox 
         * on Windows for instance the keyboard combination is ALT+SHIFT+whatever
         * key you choose here. So if you choose 'J' as the AccessKey, the user
         * will have to hold down ALT+SHIFT and press 'J' to use the keyboard 
         * shortcut.
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
