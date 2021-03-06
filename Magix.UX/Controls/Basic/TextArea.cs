/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
    /*
     * textarea ajax control
     */
    public class TextArea : BaseWebControlFormElementInputText, IValueControl
    {
        /*
         * height of control
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
                el.Write(Value);
            }
        }

        protected override bool ShouldAddValue
        {
            get { return false; }
        }

        protected override void AddAttributes(Element el)
        {
            if (Rows != -1)
                el.AddAttribute("rows", Rows.ToString());
            base.AddAttributes(el);
        }

        object IValueControl.ControlValue
        {
            get { return Value; }
            set { Value = value == null ? "" : Convert.ToString(value); }
        }

        bool IValueControl.IsTrueValue
        {
            get { return true; }
        }
    }
}
