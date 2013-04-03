/*
 * Magix UX - A Managed Ajax Library for ASP.NET
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
     * A 'text widget'. The basic purpose of this widget is purely to display
     * text and nothing else, though through the CssClass property and the Style
     * property you can easily manipulate this to do mostly anything you wish.
     * If a more 'complex widget' is needed, for instance to host other widgets,
     * then the Panel widget is more appropriate to use than the Label. Unless
     * the Tag property is changed, this widget will render as a span...
     */
    public class Label : AttributeControl
    {
        /**
         * The text property of your label. This is what the user will see of your widget.
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
         * The HTML tag element type used to render your widget. You can set this property
         * to anything you wish, including 'address', 'p' or any other types of HTML tags
         * you wish to use to render your widget. If you need a 'div' HTML element though,
         * or you need to render a widget with child widgets, you should rather use the Panel.
         */
        public string Tag
        {
            get { return ViewState["Tag"] == null ? "span" : (string)ViewState["Tag"]; }
            set { ViewState["Tag"] = value; }
        }

        /**
         * Useful for associating a label with an HTML FORM input element, such as 
         * a CheckBox or a RadioButton etc. Notice that this property can only be
         * legally set if the Tag property is of type "label", which is NOT the 
         * default value.
         */
        public string For
        {
            get { return ViewState["For"] == null ? "" : (string)ViewState["For"]; }
            set
            {
                string toSetValue = value;
                // Assuming 'ID' and not 'ClientID' ...
                PreRender +=
                    delegate
                    {
                        // Cheating a little bit ... ;)
                        Control ctrl = Selector.FindControl<Control>(Page, toSetValue);
                        if (ctrl != null)
                            For = ctrl.ClientID;
                    };
                if (value != toSetValue)
                    SetJsonGeneric("for", toSetValue.ToString());
                ViewState["For"] = toSetValue;
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
                el.Write(Text);
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
