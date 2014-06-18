/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /*
     * hidden field ajax control
     */
    public class Hidden : BaseControl, IValueControl
    {
        /*
         * value of control
         */
        public string Value
        {
            get { return ViewState["Value"] == null ? string.Empty : (string)ViewState["Value"]; }
            set
            {
                if (value != Value)
                    SetJsonGeneric("value", value);
                ViewState["Value"] = value;
            }
        }

		private void SetValue()
		{
			string value = Page.Request.Params[ClientID];
			if (value != Value)
			{
				ViewState["Value"] = value;
			}
		}

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("input"))
            {
                AddAttributes(el);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("type", "hidden");
            el.AddAttribute("name", ClientID);
            el.AddAttribute("value", Value);
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
