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
    /*
     * hyperlink ajax control
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

        /*
         * blur event
         */
        public event EventHandler Blur;

        /*
         * focus event
         */
        public event EventHandler Focused;

        /*
         * anchor text
         */
        public string Value
        {
            get { return ViewState["Value"] == null ? "" : (string)ViewState["Value"]; }
            set
            {
                if (value != Value)
                    SetJsonValue("InnerHtml", value);
                ViewState["Value"] = value;
            }
        }

        /*
         * url
         */
        public string URL
        {
            get { return ViewState["URL"] == null ? "" : (string)ViewState["URL"]; }
            set
            {
                if (value != Value)
                    this.SetJsonGeneric("href", value);
                ViewState["URL"] = value;
            }
        }

        /*
         * target browser window
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

        /*
         * keyboard shortcut
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

        /*
         * effects to run on blur
         */
        public Effect BlurEffect
        {
            get { return _blurEffect; }
            set { _blurEffect = value; }
        }

        /*
         * effects to run on focus
         */
        public Effect FocusedEffect
        {
            get { return _focusedEffect; }
            set { _focusedEffect = value; }
        }

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
                el.Write(Value);
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
