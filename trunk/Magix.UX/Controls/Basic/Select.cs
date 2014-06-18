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
     * select ajax control
     */
    [ParseChildren(true, "Items")]
    public class Select : BaseWebControlListFormElement, IValueControl
    {
        /*
         * raised when selected item changes
         */
        public event EventHandler SelectedIndexChanged;

        /*
         * height of control
         */
        public int Size
        {
            get { return ViewState["Size"] == null ? 1 : (int)ViewState["Size"]; }
            set
            {
                if (value != Size)
                    SetJsonGeneric("size", value.ToString());
                ViewState["Size"] = value;
            }
        }

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("select"))
            {
                AddAttributes(el);
                foreach (ListItem idx in Items)
                {
                    using (Element l = builder.CreateElement("option"))
                    {
                        l.AddAttribute("value", idx.Value);
                        if (!idx.Enabled)
                            l.AddAttribute("disabled", "disabled");
                        if (idx.Selected)
                            l.AddAttribute("selected", "selected");
                        l.Write(idx.Text);
                    }
                }
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("name", ClientID);
            if (Size != -1)
                el.AddAttribute("size", Size.ToString());
            base.AddAttributes(el);
        }

        protected void RaiseChange(int set)
        {
            SelectedIndex = set;
            if (SelectedIndexChanged != null)
                SelectedIndexChanged(this, new EventArgs());
        }

        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "change":
                    if (SelectedIndexChanged != null)
                        SelectedIndexChanged(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetEventsRegisterScript()
        {
            string evts = base.GetEventsRegisterScript();
            if (SelectedIndexChanged != null)
            {
                if (evts.Length != 0)
                    evts += ",";
                evts += "['change']";
            }
            return evts;
        }

        object IValueControl.ControlValue
        {
            get { return SelectedItem == null ? null : SelectedItem.Value; }
            set { SetSelectedItemAccordingToValue(value == null ? null : Convert.ToString(value)); }
        }

        bool IValueControl.IsTrueValue
        {
            get { return true; }
        }
    }
}
