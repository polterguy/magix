/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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

		// Overridden to make sure we extract the value form the HTTP request.
		protected override void OnInit(EventArgs e)
		{
			// Please notice that this logic only kicks in if ViewState is disabled.
			// If ViewState is turned on, then LoadViewState will take
			// care of de-serializing the value sent from the client.
			if (!IsViewStateEnabled && Page.IsPostBack)
				SetValue();
			base.OnInit(e);
		}

		private void SetValue()
		{
			string valueOfHiddenField = Page.Request.Params[ClientID];
			if (valueOfHiddenField != Value)
			{
				ViewState["Value"] = valueOfHiddenField;
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
    }
}
