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
     * label ajax control
     */
    public class Label : AttributeControl, IValueControl
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
                        Control ctrl = Selector.FindControl<Control>(Page, associatedControl);
                        if (ctrl != null)
                            For = ctrl.ClientID;
                    };
                if (value != associatedControl)
                    SetJsonGeneric("for", associatedControl.ToString());
                ViewState["For"] = associatedControl;
            }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement(Tag))
            {
                AddAttributes(el);
                el.Write(Value);
            }
        }

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(For))
                el.AddAttribute("for", For);
            base.AddAttributes(el);
        }

        object IValueControl.ControlValue
        {
            get { return Value; }
            set { Value = Convert.ToString(value); }
        }
    }
}
