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
using Magix.UX.Helpers;

namespace Magix.UX.Widgets
{
    /*
     * checkbox ajax control
     */
    public class CheckBox : BaseWebControlFormElement, IValueControl
    {
        /*
         * raised when checked state is changed
         */
        public event EventHandler CheckedChanged;

        /*
         * if true, then checkbox is checked
         */
        public bool Checked
        {
            get { return ViewState["Checked"] == null ? false : (bool)ViewState["Checked"]; }
            set
            {
                if (value != Checked)
                    SetJsonGeneric("checked", value ? "checked" : "");
                ViewState["Checked"] = value;
            }
        }

        protected override void SetValue()
        {
            bool value = Page.Request.Params[ClientID] == "on";
            if (value != Checked)
            {
                ViewState["Checked"] = value;
            }
        }

        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "change":
                    if (CheckedChanged != null)
                        CheckedChanged(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetEventsRegisterScript()
        {
            string evts = base.GetEventsRegisterScript();
            if (CheckedChanged != null)
            {
                evts = StringHelper.ConditionalAdd(
                    evts,
                    "",
                    ",",
                    "['change']");
            }
            return evts;
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
            el.AddAttribute("type", "checkbox");
            el.AddAttribute("name", ClientID);
            if (Checked)
                el.AddAttribute("checked", "checked");
            base.AddAttributes(el);
        }

        object IValueControl.ControlValue
        {
            get { return Checked; }
            set { Checked = Convert.ToBoolean(value); }
        }
    }
}
