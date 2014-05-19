/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX.Core;
using Magix.UX.Builder;
using Magix.UX.Effects;
using Magix.UX.Helpers;

namespace Magix.UX.Widgets.Core
{
    /*
     * web ajax control. has visible html
     */
    public abstract class BaseWebControl : BaseControl, IAttributeAccessor
    {
        private static readonly string[] _handlerNames = new string[]
        {
            "click",
            "dblclick",
            "mousedown",
            "mouseup",
            "mouseover",
            "mouseout",
            "keypress",
            "esc"
        };

        private StyleCollection _styles;
        private Effect _clickedEffect;
        private Effect _dblClickEffect;
        private Effect _mouseOverEffect;
        private Effect _mouseOutEffect;
        private Effect _mouseDownEffect;
        private Effect _mouseUpEffect;
        private Effect _keyPressEffect;
        private Effect _escEffect;

        /*
         * raised upon click
         */
        public event EventHandler Click;

        /*
         * raised upon double-click
         */
        public event EventHandler DblClick;

        /*
         * raised upon left mouse button pressed down on control
         */
        public event EventHandler MouseDown;

        /*
         * raised upon left mouse button pressed down and released on control
         */
        public event EventHandler MouseUp;

        /*
         * raised when mouse is hovered over control
         */
        public event EventHandler MouseOver;

        /*
         * raised when mouse is hovered over control and then moved out of control
         */
        public event EventHandler MouseOut;

        /*
         * raised when control has focus and key is pressed
         */
        public event EventHandler KeyPress;

        /*
         * raised when control has focus and escape is pressed
         */
        public event EventHandler Esc;

        public BaseWebControl()
        {
            _styles = new StyleCollection(this);
        }

        /*
         * css classes for control
         */
        public virtual string Class
        {
            get { return ViewState["Class"] == null ? "" : (string)ViewState["Class"]; }
            set
            {
                if (value != Class)
                    SetJsonGeneric("className", value);
                ViewState["Class"] = value;
            }
        }

        /*
         * reading direction of control
         */
        public virtual string Dir
        {
            get { return ViewState["Dir"] == null ? "" : (string)ViewState["Dir"]; }
            set
            {
                if (value != "rtl" && value != "ltr" && !string.IsNullOrEmpty(value))
                    throw new ArgumentException("only ltr and rtl are legal values");
                if (value != Dir)
                    SetJsonGeneric("dir", value);
                ViewState["Dir"] = value;
            }
        }

        /*
         * tooltip of control
         */
        public virtual string Title
        {
            get { return ViewState["Title"] == null ? "" : (string)ViewState["Title"]; }
            set
            {
                if (value != Title)
                    SetJsonGeneric("title", value);
                ViewState["Title"] = value;
            }
        }

        /*
         * tab order of control
         */
        public string TabIndex
        {
            get { return ViewState["TabIndex"] as string; }
            set
            {
                if (value != TabIndex)
                    SetJsonGeneric("tabindex", value);
                ViewState["TabIndex"] = value;
            }
        }

        /*
         * css hardcoded style of control
         */
        public virtual StyleCollection Style
        {
            get { return _styles; }
        }

        /*
         * effect to run when clicked
         */
        public Effect ClickEffect
        {
            get { return _clickedEffect; }
            set { _clickedEffect = value; }
        }

        /*
         * effect to run when double-clicked
         */
        public Effect DblClickEffect
        {
            get { return _dblClickEffect; }
            set { _dblClickEffect = value; }
        }

        /*
         * effect to run when mouse is hovered over
         */
        public Effect MouseOverEffect
        {
            get { return _mouseOverEffect; }
            set { _mouseOverEffect = value; }
        }

        /*
         * effect to run when mouse is hovered over and out
         */
        public Effect MouseOutEffect
        {
            get { return _mouseOutEffect; }
            set { _mouseOutEffect = value; }
        }

        /*
         * effect to run when left mouse button is pressed down on control
         */
        public Effect MouseDownEffect
        {
            get { return _mouseDownEffect; }
            set { _mouseDownEffect = value; }
        }

        /*
         * effect to run when left mouse button is pressed down and released on control
         */
        public Effect MouseUpEffect
        {
            get { return _mouseUpEffect; }
            set { _mouseUpEffect = value; }
        }

        /*
         * effect to run when control has focus and key is pressed
         */
        public Effect KeyPressEffect
        {
            get { return _keyPressEffect; }
            set { _keyPressEffect = value; }
        }

        /*
         * effect to run when control has focus and escape key is pressed
         */
        public Effect EscKeyEffect
        {
            get { return _escEffect; }
            set { _escEffect = value; }
        }

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(Title))
                el.AddAttribute("title", Title);
            if (!string.IsNullOrEmpty(Class))
                el.AddAttribute("class", Class);
            if (!string.IsNullOrEmpty(Dir))
                el.AddAttribute("dir", Dir);
            string style = Style.GetStylesForResponse();
            if (!string.IsNullOrEmpty(style))
                el.AddAttribute("style", style);
            if (!string.IsNullOrEmpty(TabIndex))
                el.AddAttribute("tabindex", TabIndex);
            base.AddAttributes(el);
        }

		protected virtual bool CanRaiseEvent()
		{
			return true;
		}

        public override void RaiseEvent(string name)
        {
			if (!CanRaiseEvent())
				return;
            switch (name)
            {
                case "click":
                    if (Click != null)
                        Click(this, new EventArgs());
                    break;
                case "dblclick":
                    if (DblClick != null)
                        DblClick(this, new EventArgs());
                    break;
                case "mousedown":
                    if (MouseDown != null)
                        MouseDown(this, new EventArgs());
                    break;
                case "mouseup":
                    if (MouseUp != null)
                        MouseUp(this, new EventArgs());
                    break;
                case "mouseover":
                    if (MouseOver != null)
                        MouseOver(this, new EventArgs());
                    break;
                case "mouseout":
                    if (MouseOut != null)
                        MouseOut(this, new EventArgs());
                    break;
                case "keypress":
                    if (KeyPress != null)
                        KeyPress(this, new EventArgs());
                    break;
                case "esc":
                    if (Esc != null)
                        Esc(this, new EventArgs());
                    break;
                default:
                    throw new ArgumentException("unknown event fired for control");
            }
        }

        protected override string GetEventsRegisterScript()
        {
			if (!CanRaiseEvent())
				return "";
            string evts = GetEventsInitializationString();
            string effects = GetEffectsInitializationString();
            return StringHelper.ConditionalAdd(
                evts, "", ",", effects);
        }

        private string GetEventsInitializationString()
        {
            string evts = string.Empty;
            EventHandler[] handlers = new EventHandler[]
            {
                Click,
                DblClick,
                MouseDown,
                MouseUp,
                MouseOver,
                MouseOut,
                KeyPress,
                Esc
            };
            for (int idx = 0; idx < handlers.Length; idx++)
            {
                if (handlers[idx] != null)
                {
                    string xtra = _handlerNames[idx] == "click" ? ", true" : "";
                    evts = StringHelper.ConditionalAdd(
                        evts,
                        "",
                        ",",
                        "['" + _handlerNames[idx] + "'" + xtra + "]");
                }
            }
            return evts;
        }

        private string GetEffectsInitializationString()
        {
            string evts = string.Empty;
            Effect[] effects = new Effect[]
            {
                ClickEffect,
                DblClickEffect,
                MouseDownEffect,
                MouseUpEffect,
                MouseOverEffect,
                MouseOutEffect,
                KeyPressEffect,
                EscKeyEffect
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

        protected override void TrackViewState()
        {
            Style.TrackViewState();
            base.TrackViewState();
        }

        protected override object SaveViewState()
        {
            object[] state = new object[2];
            state[0] = Style.SaveViewState();
            state[1] = base.SaveViewState();
            return state;
        }

        protected override void LoadViewState(object objState)
        {
            if (objState == null)
                return;

            object[] state = objState as object[];
            Style.LoadViewState(state[0] as string);
            base.LoadViewState(state[1]);
        }

        public string GetAttribute(string key)
        {
            return ViewState[key].ToString();
        }

        public void SetAttribute(string key, string value)
        {
            if (key.ToLower() == "style")
            {
                string[] styleValues =
                    value.Split(
                        new char[] { ';' },
                        StringSplitOptions.RemoveEmptyEntries);
                foreach (string idx in styleValues)
                {
                    string[] keyValue = idx.Split(':');
                    string idxKey = keyValue[0];
                    string idxValue = keyValue[1];
                    Style[idxKey] = idxValue;
                }
            }
            else
            {
                ; // TODO: implement
            }
        }
    }
}
