/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /*
     * a div ajax control for wrapping child controls
     */
    public class Panel : AttributeControl, INamingContainer
    {
        /*
         * html tag to render control with
         */
        virtual public string Tag
        {
            get { return ViewState["Tag"] == null ? "div" : (string)ViewState["Tag"]; }
            set { ViewState["Tag"] = value; }
        }

        /*
         * widget clicked upon enter inside of panel
         */
        public string DefaultWidget
        {
            get { return ViewState["DefaultWidget"] == null ? null : (string)ViewState["DefaultWidget"]; }
            set { ViewState["DefaultWidget"] = value; }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement(Tag))
            {
                AddAttributes(el);
                RenderChildren(builder.Writer as System.Web.UI.HtmlTextWriter);
            }
        }

        protected override string GetClientSideScriptOptions()
        {
            string retVal = base.GetClientSideScriptOptions();
            if (!string.IsNullOrEmpty(DefaultWidget))
            {
                Control widg = Selector.FindControl<Control>(this, DefaultWidget);
                if (widg != null)
                {
                    if (!string.IsNullOrEmpty(retVal))
                        retVal += ",";
                    retVal +=
                        string.Format(
                            "defaultWidget:'{0}'",
                            widg.ClientID);
                }
            }
            return retVal;
        }
	}
}
