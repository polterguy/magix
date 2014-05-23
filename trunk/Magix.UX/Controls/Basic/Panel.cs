/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
        public string Default
        {
            get { return ViewState["Default"] == null ? null : (string)ViewState["Default"]; }
            set { ViewState["Default"] = value; }
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
            if (!string.IsNullOrEmpty(Default))
            {
                Control ctrl = Selector.FindControl<Control>(this, Default);
                if (ctrl != null)
                {
                    if (!string.IsNullOrEmpty(retVal))
                        retVal += ",";
                    retVal +=
                        string.Format(
                            "def:'{0}'",
                            ctrl.ClientID);
                }
            }
            return retVal;
        }
	}
}
