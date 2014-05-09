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
     * label ajax control
     */
    public class Label : AttributeControl
    {
        /*
         * text of label
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
         * html tag to render
         */
        public string Tag
        {
            get { return ViewState["Tag"] == null ? "span" : (string)ViewState["Tag"]; }
            set { ViewState["Tag"] = value; }
        }

        /*
         * associated checkbox or radiobutton
         */
        public string For
        {
            get { return ViewState["For"] == null ? "" : (string)ViewState["For"]; }
            set
            {
                string associatedControl = value;
                PreRender +=
                    delegate
                    {
                        // Cheating a little bit ... ;)
                        Control ctrl = Selector.FindControl<Control>(Page, associatedControl);
                        if (ctrl != null)
                            For = ctrl.ClientID;
                    };
                if (value != associatedControl)
                    SetJsonGeneric("for", associatedControl.ToString());
                ViewState["For"] = associatedControl;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement(Tag))
            {
                AddAttributes(el);
                el.Write(Value);
                RenderChildren(builder.Writer as System.Web.UI.HtmlTextWriter);
            }
        }

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(For))
                el.AddAttribute("for", For);
            base.AddAttributes(el);
        }
    }
}
