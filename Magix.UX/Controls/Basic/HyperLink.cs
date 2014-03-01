/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Helpers;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /**
     * A wrapper around a hyper link or anchor HTML element (anchor HTML element ...)
     * Sometimes you will need to create links that might change or needs
     * changes after initially created. For such scenarios, this widget
     * is highly useful.
     */
    public class HyperLink : BaseWebControl
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
         * The anchor text for your hyperlink. This is the text that will be visible in
         * the browser.
         */
        public string Text
        {
            get { return ViewState["Text"] == null ? "" : (string)ViewState["Text"]; }
            set
            {
                if (value != Text)
                    SetJsonValue("Text", value);
                ViewState["Text"] = value;
            }
        }

        /**
         * The URL for your link. This is where the user ends up if he clicks 
         * your anchor text.
         */
        public string URL
        {
            get { return ViewState["URL"] == null ? "" : (string)ViewState["URL"]; }
            set
            {
                if (value != Text)
                    this.SetJsonGeneric("href", value);
                ViewState["URL"] = value;
            }
        }

        /**
         * Which browser window the URL will be opened in. _blank will assure it'll always be a new
         * window, while most other names will then only open a new window if there's no window 
         * already opened with that name
         */
        public string Target
        {
            get { return ViewState["Target"] == null ? "" : (string)ViewState["Target"]; }
            set
            {
                if (value != Target)
                    this.SetJsonGeneric("target", value);
                ViewState["Target"] = value;
            }
        }

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

		protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("a"))
            {
                AddAttributes(el);
                el.Write(Text);
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("href", URL);
            if (!string.IsNullOrEmpty(Target))
                el.AddAttribute("target", Target);
            if (!string.IsNullOrEmpty(AccessKey))
                el.AddAttribute("accesskey", AccessKey);
            base.AddAttributes(el);
        }
	}
}
