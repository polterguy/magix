/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    // TODO: Implement Blur and Focused events ...
    /**
     * A wrapper around a hyper link or anchor HTML element (anchor HTML element ...)
     * Sometimes you will need to create links that might change or needs
     * changes after initially created. For such scenarios, this widget
     * is highly useful.
     */
    public class HyperLink : BaseWebControl
    {
        /**
         * The anchor text for your hyperlink. This is the text that will be visible in
         * the browser.
         */
        public string Text
        {
            get { return ViewState["Text"] == null ? "" : (string)ViewState["Text"]; }
            set
            {
                if (value != Text)
                    SetJsonValue("Text", value);
                ViewState["Text"] = value;
            }
        }

        /**
         * The URL for your link. This is where the user ends up if he clicks 
         * your anchor text.
         */
        public string URL
        {
            get { return ViewState["URL"] == null ? "" : (string)ViewState["URL"]; }
            set
            {
                if (value != Text)
                    this.SetJsonGeneric("href", value);
                ViewState["URL"] = value;
            }
        }

        /**
         * Which browser window the URL will be opened in. _blank will assure it'll always be a new
         * window, while most other names will then only open a new window if there's no window 
         * already opened with that name
         */
        public string Target
        {
            get { return ViewState["Target"] == null ? "" : (string)ViewState["Target"]; }
            set
            {
                if (value != Target)
                    this.SetJsonGeneric("target", value);
                ViewState["Target"] = value;
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
            using (Element el = builder.CreateElement("a"))
            {
                AddAttributes(el);
                el.Write(Text);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("href", URL);
            if (!string.IsNullOrEmpty(Target))
                el.AddAttribute("target", Target);
            if (!string.IsNullOrEmpty(AccessKey))
                el.AddAttribute("accesskey", AccessKey);
            base.AddAttributes(el);
        }
	}
}
