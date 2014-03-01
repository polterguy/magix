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
    /**
     * MUX WebControl equivalent. Contains a couple of additions to the MuxBaseControl. 
     * Among others a style attribute and a CssClass. Also contains most of all the
     * DOM event handlers you can possibly handle in Magix UX. In addition to most of
     * the client-side effects you can possibly run on your MUX controls. Class is 
     * abstract, and hence cannot be instantiated directly but is meant for inheriting
     * from.
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

        /**
         * Event raised when user clicks your widget.
         */
        public event EventHandler Click;

        /**
         * Event raised when user double-clicks your widget.
         */
        public event EventHandler DblClick;

        /**
         * Event raised when user pushes his (normally) left mouse button down while
         * the mouse cursor is over your widget. Normally you'd be more interested in
         * handling the Click event. But this event might have its uses too.
         */
        public event EventHandler MouseDown;

        /**
         * Event raised when user have pushed his (normally) left mouse button down while
         * the mouse cursor is over your widget, and then releases the mouse button. 
         * Normally you'd be more interested in handling the Click event. But this event 
         * might have its uses too.
         */
        public event EventHandler MouseUp;

        /**
         * Event raised when user moves his mouse over your widget. The equivalent of
         * 'hover'.
         */
        public event EventHandler MouseOver;

        /**
         * Event raised when user moves his mouse over your widget, and then moves it
         * away from your widget. The equivalent of 'hover-finished'.
         */
        public event EventHandler MouseOut;

        /** 
         * Event raised when a character is typed into the widget. Useful for
         * being able to trap typing into e.g. a TextBox or something similar.
         */
        public event EventHandler KeyPress;

        /** 
         * Event raised when the escape key is pressed
         */
        public event EventHandler EscKey;

        public BaseWebControl()
        {
            _styles = new StyleCollection(this);
        }

        /**
         * The CSS class(es) of your widget. To explain this is really out of the scope
         * of this library. Pick up any good CSS book to understand how to use this
         * property.
         */
        public virtual string CssClass
        {
            get { return ViewState["CssClass"] == null ? "" : (string)ViewState["CssClass"]; }
            set
            {
                if (value != CssClass)
                    SetJsonValue("CssClass", value);
                ViewState["CssClass"] = value;
            }
        }

        /**
         * The direction of text within your widget. Legal values are 'rtl' and 'ltr'. These
         * means 'right to left' and 'left to right' and signify the direction of your text.
         * Useful for being able to defined reading directions for non-latin based languages
         * such as Hebrew and Arabic.
         */
        public virtual string Dir
        {
            get { return ViewState["Dir"] == null ? "" : (string)ViewState["Dir"]; }
            set
            {
                if (value != "rtl" && value != "ltr" && !string.IsNullOrEmpty(value))
                    throw new ArgumentException("You cannot set the Dir property to any other value than 'rtl' and 'ltr'");
                if (value != Dir)
                    SetJsonValue("Dir", value);
                ViewState["Dir"] = value;
            }
        }

        /**
         * The tooltip of your widget. Will display a tiny box with your text whenever the
         * user is hovering his mouse over your widget. There are other and richer ways to
         * create more advanced tooltips in Magix UX, but no other ways will demand less 
         * bandwidth and less resources on your user's clients. The HTML attribute 'title'
         * is the value actually sent over to the client when the ToolTip is being used.
         */
        public virtual string ToolTip
        {
            get { return ViewState["ToolTip"] == null ? "" : (string)ViewState["ToolTip"]; }
            set
            {
                if (value != ToolTip)
                    SetJsonGeneric("title", value);
                ViewState["ToolTip"] = value;
            }
        }

        /**
         * What effect(s) will be ran when the user clicks your widget.
         */
        public Effect ClickEffect
        {
            get { return _clickedEffect; }
            set { _clickedEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user double clicks your widget.
         */
        public Effect DblClickEffect
        {
            get { return _dblClickEffect; }
            set { _dblClickEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user moves his mouse over your widget.
         */
        public Effect MouseOverEffect
        {
            get { return _mouseOverEffect; }
            set { _mouseOverEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user moves his mouse out of 
         * the surface occupied by your widget on the screen.
         */
        public Effect MouseOutEffect
        {
            get { return _mouseOutEffect; }
            set { _mouseOutEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user clicks his left mouse 
         * button on top of your widget, but before he releases it.
         */
        public Effect MouseDownEffect
        {
            get { return _mouseDownEffect; }
            set { _mouseDownEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user have clicked your widget, 
         * and then release his mouse button. In most cases the the ClickEffect
         * will be more appropriate to use, but this event effect has its uses too.
         */
        public Effect MouseUpEffect
        {
            get { return _mouseUpEffect; }
            set { _mouseUpEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user have types characters into 
         * your widget.
         */
        public Effect KeyPressEffect
        {
            get { return _keyPressEffect; }
            set { _keyPressEffect = value; }
        }

        /**
         * What effect(s) will be ran when the user clicks ESC
         */
        public Effect EscKeyEffect
        {
            get { return _escEffect; }
            set { _escEffect = value; }
        }

        /**
         * Every DOM element on the client-side has a 'tab order' which defines the
         * order widgets will be given focus while the user is tabbing through your
         * web page. The lower this number is, the earlier in the chain the specific
         * widget will be given focus when tabbing. This is the value defining the
         * tab order.
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

        /**
         * The inline styles of your widget. An exhaustive explanation of this
         * property is truly out of scope of this file, pick up any good book
         * on the subject CSS to understand how to use this property. But 
         * specifically for Magix UX, any style properties added, removed, changed
         * and so on any times during its life-cycle, will automatically, and extremely 
         * bandwidth efficient be changed/added/removed Automagixally by the core
         * engine of MUX.
         */
        public virtual StyleCollection Style
        {
            get { return _styles; }
        }

        protected override void AddAttributes(Element el)
        {
            if (!string.IsNullOrEmpty(ToolTip))
                el.AddAttribute("title", ToolTip);
            if (!string.IsNullOrEmpty(CssClass))
                el.AddAttribute("class", CssClass);
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
                    if (EscKey != null)
                        EscKey(this, new EventArgs());
                    break;
                default:
                    throw new ApplicationException("Unknown event fired for control");
            }
        }

        // TODO: Refactor...
        protected override string GetEventsRegisterScript()
        {
			if (!CanRaiseEvent())
				return "";
            string evts = GetEventsInitializationString();
            string effects = GetEffectsInitializationString();
            return StringHelper.ConditionalAdd(
                evts, "", ",", effects);
        }

        // Helper method for serializing events into the JS initialization script
        // which goes to the client.
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

                // TODO: WTF. Why is there another on in BaseWebControlFormElementInputText ...?
                // Called; EscPressed
                EscKey
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

        // Helper method for serializing client-side effects into the 
        // JS initialization script which goes to the client.
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
                ; // TODO: Implement something intelligently here ...
            }
        }

        internal void SetViewStateValue(string key, object value)
        {
            ViewState[key] = value;
        }
    }
}
