/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
    /**
     * Hidden field widget. Useful for having state you wish to pass on to the client
     * but don't want it to be visible for the end user. Notice that this is not a
     * safe place to put things that the user is not supposed to see, like passwords
     * and such. Do NOT trust the value of this element to not be tampered with.
     * Alternatives for using this widget is ViewState, cookies and the Session object.
     * ViewState and cookies are neither 'safe' against tampering.
     */
    public class HiddenField : BaseControl
    {
        /**
         * The value of the hidden field. The default value is string.Empty.
         * Please notice that the HiddenField is not a 'safe' place to store 
         * things you do not want to show the end user, so only use this for
         * non-secure-critical information, such as for instance UI states 
         * and such. No 'secrets' in this value, since this value is very easy 
         * to figure out for the end user.
         */
        public string Value
        {
            get { return ViewState["Value"] == null ? string.Empty : (string)ViewState["Value"]; }
            set
            {
                if (value != Value)
                    SetJsonValue("Value", value);
                ViewState["Value"] = value;
            }
        }

        protected override void OnInit(EventArgs e)
        {
            if (Page.IsPostBack)
            {
                GetValue();
            }
            base.OnInit(e);
        }

        private void GetValue()
        {
            string value = Page.Request.Params[ClientID];
            if (value != null)
                Value = value;
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
    }
}
