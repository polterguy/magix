/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Web.UI;
using Magix.UX.Builder;
using Magix.UX.Helpers;

namespace Magix.UX.Widgets.Core
{
    /*
     * class for form elements taking text input
     */
    public abstract class BaseWebControlFormElementInputText : BaseWebControlFormElementText, IValueControl
    {
        /*
         * shown when value is empty
         */
        public string PlaceHolder
        {
            get { return ViewState["PlaceHolder"] == null ? "" : (string)ViewState["PlaceHolder"]; }
            set
            {
                if (value != PlaceHolder)
                    SetJsonGeneric("placeholder", value);
                ViewState["PlaceHolder"] = value;
            }
        }

        private static readonly string[] _handlerNames = new string[]
        {
            "change"
        };

        private bool _wasSelected;

        /*
         * raised when text changes
         */
        public event EventHandler TextChanged;

        /*
         * selects all text
         */
        public void Select()
        {
            _wasSelected = true;
            if (Manager.Instance.IsAjaxCallback)
            {
                SetJsonValue("Select", "");
            }
        }

        protected override void SetValue()
        {
            string valueOfTextBox = Page.Request.Params[ClientID];
            if (valueOfTextBox != Value)
            {
                ViewState["Value"] = valueOfTextBox;
            }
        }

        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "change":
                    if (TextChanged != null)
                        TextChanged(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetClientSideScriptOptions()
        {
            string retVal = base.GetClientSideScriptOptions();
            if (_wasSelected)
            {
                if (!string.IsNullOrEmpty(retVal))
                    retVal += ",";
                retVal += "select:true";
            }
            return retVal;
        }

        protected override string GetEventsRegisterScript()
        {
            string retVal = GetEventsInitializationString();
            string baseVal = base.GetEventsRegisterScript();
            retVal = StringHelper.ConditionalAdd(retVal, "", ",", baseVal);
            return retVal;
        }

        private string GetEventsInitializationString()
        {
            string evts = string.Empty;
            EventHandler[] handlers = new EventHandler[]
            {
                TextChanged
            };
            for (int idx = 0; idx < handlers.Length; idx++)
            {
                if (handlers[idx] != null)
                {
                    evts = StringHelper.ConditionalAdd(
                        evts,
                        "",
                        ",",
                        "['" + _handlerNames[idx] + "']");
                }
            }
            return evts;
        }

        protected virtual bool ShouldAddValue
        {
            get { return !string.IsNullOrEmpty(Value); }
        }

        protected override void AddAttributes(Element el)
        {
            if (ShouldAddValue)
                el.AddAttribute("value", Value);
            if (!string.IsNullOrEmpty(PlaceHolder))
                el.AddAttribute("placeholder", PlaceHolder);
            el.AddAttribute("name", ClientID);
            base.AddAttributes(el);
        }

        object IValueControl.ControlValue
        {
            get { return Value; }
            set { Value = Convert.ToString(value); }
        }

        bool IValueControl.IsTrueValue
        {
            get { return true; }
        }
    }
}
