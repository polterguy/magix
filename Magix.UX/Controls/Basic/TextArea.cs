/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /**
     * A multiple line type of 'give me some text input' type of widget. It wraps the
     * textarea HTML element. If you only need single lines of input, you should
     * probably rather use the TextBox widget. However this widget is useful for cases
     * when you need multiple lines of text input. See also the RichEdit widget if
     * you need rich formatting of your text.
     */
    public class TextArea : BaseWebControlFormElementInputText
    {
        /**
         * Ghost text displayed only if there is no value in the textbox. Useful for giving 
         * end user hints and clues about what type of field this is.
         */
        public string PlaceHolder
        {
            get { return ViewState["PlaceHolder"] == null ? "" : (string)ViewState["PlaceHolder"]; }
            set
            {
                if (value != PlaceHolder)
                    SetJsonGeneric("placeholder", value);
                ViewState["PlaceHolder"] = value;
            }
        }

        /**
         * Number of rows on textarea HTML element
         */
        public int Rows
        {
            get { return ViewState["Rows"] == null ? -1 : (int)ViewState["Rows"]; }
            set
            {
                if (value != Rows)
                    SetJsonGeneric("rows", value.ToString());
                ViewState["Rows"] = value;
            }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("textarea"))
            {
                AddAttributes(el);
                el.Write(Text);
            }
        }

        protected override bool ShouldAddValue
        {
            get { return false; }
        }

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(PlaceHolder))
                el.AddAttribute("placeholder", PlaceHolder);
            if (Rows != -1)
                el.AddAttribute("rows", Rows.ToString());
            base.AddAttributes(el);
        }
    }
}
