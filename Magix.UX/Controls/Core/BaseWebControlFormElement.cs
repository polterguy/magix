/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Helpers;
using Magix.UX.Effects;

namespace Magix.UX.Widgets.Core
{
    /**
     * Abstract base class for widgets which are HTML FORM type of elements. Abstracts
     * away things such as blur and focus event handling, enabled, keyboard shortcuts
     * and other internals things. Inherit from this one if you intend to create an
     * Ajax Widget which wraps an HTML FORM element.
     */
    public abstract class BaseWebControlFormElement : BaseWebControl
    {
        private static readonly string[] _handlerNames = new string[]
        {
            "blur",
            "focus"
        };

        private Effect _blurEffect;
        private Effect _focusedEffect;

        /**
         * Event being raised when the widget does no longer have focus. 
         * The exact opposite of the Focused event. Will trigger whenever the
         * widget loses focus for some reasons. Notice that the widget (obviously)
         * needs to *have* focus before it can 'lose' it.
         * Some ways a widget may lose focus is when the user tabs through 
         * the widgets on the screen. Another way is clicking another widget, 
         * and thereby loosing focus for the previously focused widget.
         */
        public event EventHandler Blur;

        /**
         * The Focus event. The exact opposite of the Blur event. This one will be
         * raised whenever the widget gains focus. Gaining focus can happen several
         * ways, first of all the user might click the specific widget, which will
         * give that widget focus. Secondly the user might tab through the widgets
         * until the specific widget is reached. When the widget gains focus, this
         * event will be raised.
         */
        public event EventHandler Focused;

        /**
         * What keyboard shortcut the user will have to use to mimick a 'click' on 
         * the widget. Often the keyboard shortcut will be mixed up with other
         * keys, depending upon your operating system or browser. For FireFox 
         * on Windows for instance the keyboard combination is ALT+SHIFT+whatever
         * key you choose here. So if you choose 'J' as the AccessKey, the user
         * will have to hold down ALT+SHIFT and press 'J' to use the keyboard 
         * shortcut.
         */
        public string AccessKey
        {
            get { return ViewState["AccessKey"] == null ? "" : (string)ViewState["AccessKey"]; }
            set
            {
                if (value != AccessKey)
                    SetJsonValue("AccessKey", value);
                ViewState["AccessKey"] = value;
            }
        }

        /**
         * Boolean value indicating whether or not the widget is enabled. If this one 
         * is false, then the user cannot change the value of the widget by interacting 
         * normally with it. Please note that this is not safe and can be overridden
         * by tools and manipulating the HTTP stream directly and so on. Do not trust
         * values from controls that are disabled to be the value you explicitly gave 
         * them earlier.
         */
        public bool Enabled
        {
            get { return ViewState["Enabled"] == null ? true : (bool)ViewState["Enabled"]; }
            set
            {
                if (value != Enabled)
                    SetJsonGeneric("disabled", (value ? "" : "disabled"));
                ViewState["Enabled"] = value;
            }
        }

        /**
         * What Effect will run when user is making the widget lose focus.
         */
        public Effect BlurEffect
        {
            get { return _blurEffect; }
            set { _blurEffect = value; }
        }

        /**
         * What Effect will run when user is making the widget gain focus.
         */
        public Effect FocusedEffect
        {
            get { return _focusedEffect; }
            set { _focusedEffect = value; }
        }

        /**
         * Abstract method you need to implement to receive the value form the client
         * and serialize it into the server-side web control. Override this one and
         * extract the value of the control from the HTTP Request if you inherit from 
         * this class.
         */
        protected abstract void SetValue();

        // Overridden to make sure we extract the value form the HTTP request.
        protected override void OnInit(EventArgs e)
        {
            // Please notice that this logic only kicks in if ViewState is disabled.
            // If ViewState is turned on, then LoadViewState will take
            // care of de-serializing the value sent from the client.
            if (Enabled && !IsViewStateEnabled && Page.IsPostBack)
                SetValue();
            base.OnInit(e);
        }

        // Overridden to make sure we extract the value form the HTTP request.
        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            if (Enabled && Page.IsPostBack)
                SetValue();
        }

        // Overridden to handle the special events this class implements...
        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "blur":
                    if (Blur != null)
                        Blur(this, new EventArgs());
                    break;
                case "focus":
                    if (Focused != null)
                        Focused(this, new EventArgs());
                    break;
                default:
                    base.RaiseEvent(name);
                    break;
            }
        }

        protected override string GetEventsRegisterScript()
        {
            string evts = GetEventsInitializationString();
            string effects = GetEffectsInitializationString();
            string retVal = StringHelper.ConditionalAdd(evts, "", ",", effects);
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
                Blur,
                Focused
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

        // Helper method for serializing client-side effects into the 
        // JS initialization script which goes to the client.
        private string GetEffectsInitializationString()
        {
            string evts = string.Empty;
            Effect[] effects = new Effect[]
            {
                BlurEffect,
                FocusedEffect
            };
            for (int idx = 0; idx < effects.Length; idx++)
            {
                if (effects[idx] != null)
                {
                    evts = StringHelper.ConditionalAdd(
                        evts,
                        "",
                        ",",
                        "['" + _handlerNames[idx] + "', 'effect', " + effects[idx].RenderString() + "]");
                }
            }
            return evts;
        }

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(AccessKey))
                el.AddAttribute("accesskey", AccessKey);
            if (!Enabled)
                el.AddAttribute("disabled", "disabled");
            base.AddAttributes(el);
        }
    }
}
