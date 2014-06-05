/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Helpers;
using Magix.UX.Effects;

namespace Magix.UX.Widgets.Core
{
    /*
     * form element control base class
     */
    public abstract class BaseWebControlFormElement : AttributeControl
    {
        private static readonly string[] _handlerNames = new string[]
        {
            "blur",
            "focus"
        };

        private Effect _blurEffect;
        private Effect _focusedEffect;

        /*
         * raised when control looses focus
         */
        public event EventHandler Blur;

        /*
         * raised when control gains focus
         */
        public event EventHandler Focused;

        /*
         * keyboard shortcut of control
         */
        public string AccessKey
        {
            get { return ViewState["AccessKey"] == null ? "" : (string)ViewState["AccessKey"]; }
            set
            {
                if (value != AccessKey)
                    SetJsonGeneric("AccessKey", value);
                ViewState["AccessKey"] = value;
            }
        }

        /*
         * if true then control is disabled
         */
        public bool Disabled
        {
            get { return ViewState["Disabled"] == null ? false : (bool)ViewState["Disabled"]; }
            set
            {
                if (value != Disabled)
                    SetJsonGeneric("disabled", (value ? "disabled" : ""));
                ViewState["Disabled"] = value;
            }
        }

		protected override bool CanRaiseEvent()
		{
            return !Disabled;
		}

        /*
         * efffect to run when control looses focus
         */
        public Effect BlurEffect
        {
            get { return _blurEffect; }
            set { _blurEffect = value; }
        }

        /*
         * efffect to run when control gains focus
         */
        public Effect FocusedEffect
        {
            get { return _focusedEffect; }
            set { _focusedEffect = value; }
        }

        /*
         * sets value of control
         */
        protected abstract void SetValue();

        protected override void LoadViewState(object savedState)
        {
            base.LoadViewState(savedState);
            if (!Disabled && Page.IsPostBack)
                SetValue();
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

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(AccessKey))
                el.AddAttribute("accesskey", AccessKey);
            if (Disabled)
                el.AddAttribute("disabled", "disabled");
            base.AddAttributes(el);
        }
    }
}
