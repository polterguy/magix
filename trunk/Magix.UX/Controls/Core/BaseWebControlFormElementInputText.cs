/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Web.UI;
using Magix.UX.Builder;
using Magix.UX.Helpers;

namespace Magix.UX.Widgets.Core
{
    /**
     * Abstract base class for widgets being BaseWebControlFormElementText type
     * of controls, but also uses the Text portions as an input value which the user
     * can change himself through interacting with the widget.
     */
    public abstract class BaseWebControlFormElementInputText : BaseWebControlFormElementText
    {
        private static readonly string[] _handlerNames = new string[]
        {
            "esc",
            "change"
        };

        private bool _hasSetSelect;

        /**
         * Raised when widget changes its Text property. Normally a TextBox and a TextArea
         * won't raise this event before they're loosing focus, since that's when we know
         * what the Text property is actually changed to.
         */
        public event EventHandler TextChanged;

        /**
         * Raise when widget has focus and user clicks ESC. Useful for being able to create
         * 'discardable operations' types of logic.
         */
        public event EventHandler EscPressed;

        /**
         * Selects the whole text portions of the widget and gives the widget focus. Useful
         * for widgets where you know the user will want to change its entire Text content
         * when returned.
         */
        public void Select()
        {
            _hasSetSelect = true;
            if (AjaxManager.Instance.IsCallback)
            {
                SetJsonValue("Select", "");
            }
        }

        protected override void SetValue()
        {
            string valueOfTextBox = Page.Request.Params[ClientID];
            if (valueOfTextBox != Text)
            {
                ViewState["Text"] = valueOfTextBox;
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
                case "esc":
                    if (EscPressed != null)
                        EscPressed(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetClientSideScriptOptions()
        {
            string retVal = base.GetClientSideScriptOptions();
            if (_hasSetSelect)
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

        // Helper method for serializing events into the JS initialization script
        // which goes to the client.
        private string GetEventsInitializationString()
        {
            string evts = string.Empty;
            EventHandler[] handlers = new EventHandler[]
            {
                EscPressed,
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
            get { return true; }
        }

        protected override void AddAttributes(Element el)
        {
            if (ShouldAddValue)
                el.AddAttribute("value", Text);
            el.AddAttribute("name", ClientID);
            base.AddAttributes(el);
        }
    }
}
