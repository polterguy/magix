/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
     * This widget is another type of 'button widget', though this will be rendered
     * using anchor HTML element (anchor element...)
     * Even though everything can be made 'clickable' in Magix UX, it is definitely
     * semantically much more 'correct' to constraint yourself to the ones that
     * are expected to be 'clickable', such as this widget (LinkButton), Button, 
     * ImageButton etc. Among other things screen-readers and such will recognize 
     * these types of elements as 'clickable' and present the end user with the
     * option of clicking these types of widgets.
     */
    public class LinkButton : BaseWebControlFormElementText
    {
        /**
         * The text property of the LinkButton. This is the 'anchor text' of the widget.
         * Basically what the user will 'see' on the screen.
         */
        public override string Text
        {
            get { return ViewState["Text"] == null ? "" : (string)ViewState["Text"]; }
            set
            {
                if (value != Text)
                    SetJsonValue("Text", value);
                ViewState["Text"] = value;
            }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("a"))
            {
                AddAttributes(el);
                el.Write(Text);
                RenderChildren(builder.Writer);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("href", "javascript:MUX.emptyFunction();");
            base.AddAttributes(el);
        }

        protected override void SetValue()
        { }
    }
}
