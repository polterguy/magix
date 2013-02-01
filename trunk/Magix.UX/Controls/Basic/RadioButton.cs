/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
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
    /**
     * A two-state widget type that can be grouped together with other widgets to
     * mimick multiple selections. Like for instance; "Would you like to have coffee, 
     * tea or water" is a perfect example of where you would want to use RadioButtons.
     * If you want to group specific radio buttons together, you must give them the same
     * GroupName property. If you do, then only one of these RadioButtons can at any 
     * time be 'selected'.
     */
    public class RadioButton : BaseWebControlFormElement
    {
        /**
         * Event raised when the checked state of the widget changes. Use the
         * Checked property to determine if the CheckBox is 'on' or 'off'.
         */
        public event EventHandler CheckedChanged;

        /**
         * If multiple RadioButtons are given the same GroupName, then these RadioButtons
         * will be grouped together, and only one of them can at any time be selected.
         * By playing with this property, you can create multiple groups of radio buttons
         * that will be grouped together accordingly to their GroupName property.
         */
        public string GroupName
        {
            get { return ViewState["GroupName"] == null ? "" : (string)ViewState["GroupName"]; }
            set
            {
                if (value != GroupName)
                    SetJsonGeneric("name", value);
                ViewState["GroupName"] = value;
            }
        }

        /**
         * Use this property to determine if the widget is checked or not. 
         * If this property is true, then the widget is checked. The default
         * value is 'false'. You can also set this value in your code to change 
         * the state of the checked value.
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
            bool valueOfChecked =
                Page.Request.Params[
                    string.IsNullOrEmpty(GroupName) ? ClientID : GroupName] == ClientID;
            if (valueOfChecked != Checked)
            {
                ViewState["Checked"] = valueOfChecked;
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
            el.AddAttribute("type", "radio");
            el.AddAttribute("value", ClientID);
            if (!string.IsNullOrEmpty(GroupName))
                el.AddAttribute("name", GroupName);
            else
                el.AddAttribute("name", ClientID);
            if (Checked)
                el.AddAttribute("checked", "checked");
            base.AddAttributes(el);
        }
    }
}
