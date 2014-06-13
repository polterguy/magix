/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /*
     * text input ajax control
     */
    public class TextBox : BaseWebControlFormElementInputText, IValueControl
    {
        /*
         * type of text
         */
        public enum TextBoxType
        {
            Color,
            Email,
            DateTime,
            DateTimeLocal,
            Date,
            Month,
            Text,
            Number,
            Password,
            Tel,
            Range,
            Search,
            Time,
            Url,
            Week
        };

        /*
         * raised when cariage return is pressed
         */
        public event EventHandler EnterPressed;

        /*
         * type of input
         */
        public TextBoxType Type
        {
            get { return ViewState["Type"] == null ? TextBoxType.Text : (TextBoxType)ViewState["Type"]; }
            set
            {
                if (value != Type)
                    SetJsonGeneric("type", value.ToString().ToLower());
                ViewState["Type"] = value;
            }
        }

        /*
         * if true, will automatically capitalize input
         */
        public bool AutoCapitalize
        {
            get { return ViewState["AutoCapitalize"] == null ? false : (bool)ViewState["AutoCapitalize"]; }
            set
            {
                if (value != AutoCapitalize)
                    SetJsonGeneric("autocapitalize", value ? "off" : "on");
                ViewState["AutoCapitalize"] = value;
            }
        }

        /*
         * if true, will automatically correct spelling errors
         */
        public bool AutoCorrect
        {
            get { return ViewState["AutoCorrect"] == null ? false : (bool)ViewState["AutoCorrect"]; }
            set
            {
                if (value != AutoCorrect)
                    SetJsonGeneric("autocorrect", value ? "off" : "on");
                ViewState["AutoCorrect"] = value;
            }
        }

        /*
         * if true, will attempt to autmatically complete input
         */
        public bool AutoComplete
        {
            get { return ViewState["AutoComplete"] == null ? false : (bool)ViewState["AutoComplete"]; }
            set
            {
                if (value != AutoComplete)
                    SetJsonGeneric("autocomplete", value ? "on" : "off");
                ViewState["AutoComplete"] = value;
            }
        }

        /*
         * max number of characters
         */
        public int MaxLength
        {
            get { return ViewState["MaxLength"] == null ? 0 : (int)ViewState["MaxLength"]; }
            set 
            {
                if (value != MaxLength)
                    SetJsonGeneric("maxlength", value.ToString());
                ViewState["MaxLength"] = value;
            }
        }

        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "enter":
                    if (EnterPressed != null)
                        EnterPressed(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetEventsRegisterScript()
        {
            string evts = base.GetEventsRegisterScript();
            if (EnterPressed != null)
            {
                if (evts.Length != 0)
                    evts += ",";
                evts += "['enter']";
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
            el.AddAttribute("type", Type.ToString().ToLower());
            if (MaxLength > 0)
                el.AddAttribute("maxlength", MaxLength.ToString());
            if (AutoCapitalize)
                el.AddAttribute("autocapitalize", "on");
            if (AutoComplete)
                el.AddAttribute("autocomplete", "on");
            else
                el.AddAttribute("autocomplete", "off");
            if (AutoCorrect)
                el.AddAttribute("autocorrect", "on");
            if (!string.IsNullOrEmpty(PlaceHolder))
                el.AddAttribute("placeholder", PlaceHolder);
            base.AddAttributes(el);
        }

        object IValueControl.ControlValue
        {
            get { return Value; }
            set { Value = value as string; }
        }

        bool IValueControl.IsTrueValue
        {
            get { return true; }
        }
    }
}
