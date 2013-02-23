/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.ComponentModel.Design.Serialization;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    public class StyleCollection
    {
        private class StyleValue
        {
            private string _beforeViewStateTrackingValue;
            private string _viewStateValue;
            private string _afterViewStateTrackingValue;
            private string _onlyViewStateValue;

            public StyleValue()
            { }

            public string BeforeViewStateTrackingValue
            {
                get { return _beforeViewStateTrackingValue; }
                set { _beforeViewStateTrackingValue = value; }
            }

            public string ViewStateValue
            {
                get { return _viewStateValue; }
                set { _viewStateValue = value; }
            }

            public string AfterViewStateTrackingValue
            {
                get { return _afterViewStateTrackingValue; }
                set { _afterViewStateTrackingValue = value; }
            }

            public string OnlyViewStateValue
            {
                get { return _onlyViewStateValue; }
                set { _onlyViewStateValue = value; }
            }
        }

        private BaseWebControl _control;
        private bool _trackingViewState;
        private Dictionary<string, StyleValue> _styleValues = new Dictionary<string, StyleValue>();

        internal StyleCollection(BaseWebControl control)
        {
            _control = control;
            _control.PreRender += new EventHandler(_control_PreRender);
        }

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (string idx in _styleValues.Keys)
                {
                    yield return idx;
                }
            }
        }

        private void _control_PreRender(object sender, EventArgs e)
        {
            if (_control.HasRendered && _styleValues.Count > 0)
            {
                Dictionary<string, string> styles = _control.GetJsonValues("AddStyle");
                foreach (string idxKey in _styleValues.Keys)
                {
                    if (_styleValues[idxKey].AfterViewStateTrackingValue != null)
                    {
                        string idx = idxKey;
                        while (idx.IndexOf('-') != -1)
                        {
                            int where = idx.IndexOf('-');
                            idx = idx.Substring(0, where) + idx.Substring(where + 1, 1).ToUpper() + idx.Substring(where + 2);
                        }
                        styles[idx] = _styleValues[idxKey].AfterViewStateTrackingValue;
                    }
                }
            }
        }

        public void SetStyleValueViewStateOnly(string idx, string value)
        {
            SetStyleValue(idx, value, true);
        }

        private void SetStyleValue(string idx, string value, bool viewStateOnly)
        {
            if (this[idx] == value)
                return;
            AddStyleToCollection(idx, value, viewStateOnly);
        }

        private void AddStyleToCollection(string idx, string value, bool viewStateOnly)
        {
            string styleName = idx.Trim().ToLowerInvariant();
            string styleValue = value.Trim ();
            
            if (styleName == "opacity")
            {
                decimal opacity;
                
                // use full opacity if the user didn't specify a correct value
                if (!decimal.TryParse(styleValue, NumberStyles.Float, CultureInfo.InvariantCulture, out opacity))
                    opacity = 1.0M;
                styleValue = opacity.ToString(CultureInfo.InvariantCulture);
            }

            if (_styleValues.ContainsKey(styleName))
            {
                // Key exists from before
                StyleValue oldValue = _styleValues[styleName];
                if (viewStateOnly)
                {
                    oldValue.OnlyViewStateValue = styleValue;
                }
                else
                {
                    if (_trackingViewState)
                        oldValue.AfterViewStateTrackingValue = styleValue;
                    else
                        oldValue.BeforeViewStateTrackingValue = styleValue;
                }
            }
            else
            {
                // Key doesn't exist from before
                StyleValue nValue = new StyleValue();
                if (viewStateOnly)
                {
                    nValue.OnlyViewStateValue = styleValue;
                }
                else
                {
                    if (_trackingViewState)
                        nValue.AfterViewStateTrackingValue = styleValue;
                    else
                        nValue.BeforeViewStateTrackingValue = styleValue;
                }
                _styleValues[styleName] = nValue;
            }
        }

		public string this[string idx]
		{
			get
			{
                if (idx.ToLower() != idx)
                    throw new ApplicationException("Cannot have a style property which contains uppercase letters");

                if (_styleValues.ContainsKey(idx))
                {
                    if (_styleValues[idx].AfterViewStateTrackingValue != null)
                        return _styleValues[idx].AfterViewStateTrackingValue;
                    else if (_styleValues[idx].OnlyViewStateValue != null)
                        return _styleValues[idx].OnlyViewStateValue;
                    else if (_styleValues[idx].ViewStateValue != null)
                        return _styleValues[idx].ViewStateValue;
                    else
                        return _styleValues[idx].BeforeViewStateTrackingValue;
                }

                return null;
			}
			set { this.SetStyleValue(idx, value, false); }
		}

        public string this[Styles idx]
        {
            get
            {
                string styleString = TransformStyleToString(idx);
                return this[styleString];
            }
            set
            {
                string styleString = TransformStyleToString(idx);
                this[styleString] = value;
            }
        }

        private static string TransformStyleToString(Styles idx)
        {
            string tmp = "";
            switch (idx)
            {
                case Styles.floating:
                    tmp = "float";
                    break;
                default:
                    tmp = idx.ToString();
                    break;
            }
            string styleString = "";
            foreach (char idxChar in tmp)
            {
                if (char.IsUpper(idxChar))
                {
                    styleString += "-";
                    styleString += char.ToLower(idxChar);
                }
                else
                    styleString += idxChar;
            }
            return styleString;
        }

        public string GetViewStateOnlyStyles()
        {
            string retVal = "";
            foreach (string idxKey in _styleValues.Keys)
            {
                string value = null;
                if (_styleValues[idxKey].OnlyViewStateValue != null)
                {
                    value = _styleValues[idxKey].OnlyViewStateValue;
                }
                else if (_styleValues[idxKey].AfterViewStateTrackingValue != null)
                {
                    value = _styleValues[idxKey].AfterViewStateTrackingValue;
                }
                else if (_styleValues[idxKey].ViewStateValue != null)
                {
                    value = _styleValues[idxKey].ViewStateValue;
                }
                if (value != null)
                {
                    retVal +=
                        TransformToViewStateShorthand(idxKey) +
                        ":" +
                        value +
                        ";";
                }
            }
            return retVal;
        }

        public string GetStylesForResponse()
        {
            string retVal = "";
            foreach (string idxKey in _styleValues.Keys)
            {
                string value = null;
                if (_styleValues[idxKey].AfterViewStateTrackingValue != null)
                {
                    value = _styleValues[idxKey].AfterViewStateTrackingValue;
                }
                else if (_styleValues[idxKey].ViewStateValue != null)
                {
                    value = _styleValues[idxKey].ViewStateValue;
                }
                else if (_styleValues[idxKey].BeforeViewStateTrackingValue != null)
                {
                    value = _styleValues[idxKey].BeforeViewStateTrackingValue;
                }

                if (value != null)
                {
                    string key = idxKey;
                    switch (idxKey)
                    {
                        case "box-shadow":
                            key = GetBrowserPrefix() + key;
                            break;
                        case "background-image":
                        {
                            if (value.Contains("linear-gradient"))
                            {
                                if (HttpContext.Current.Request.Browser.MobileDeviceManufacturer == "Apple" ||
                                    HttpContext.Current.Request.Browser.MobileDeviceManufacturer == "Google")
                                {
                                    string value2 = "";
                                    if (value.Contains("url"))
                                    {
                                        value2 = "url(" + value.Split('(')[1].Split(')')[0] + "), ";
                                    }
                                    string col1 = "#" + value.Split('#')[1].Substring(0, 6);
                                    string col2 = "#" + value.Split('#')[2].Substring(0, 6);
                                    value = value2 + 
                                        string.Format(
                                            "-webkit-gradient(linear, 0% 0%, 0% 100%, from({0}), to({1}))", 
                                            col1, 
                                            col2);
                                }
                                else
                                {
                                    value = value.Replace(
                                        "linear-gradient",
                                        GetBrowserPrefix() + "linear-gradient");
                                }
                            }
                        } break;
                    }
                    retVal += key + ":" + value + ";";
                }
            }
            return retVal;
        }

        public static string GetBrowserPrefix()
        {
            if (HttpContext.Current.Request.Browser.MobileDeviceManufacturer == "Apple" ||
                HttpContext.Current.Request.Browser.MobileDeviceManufacturer == "Google")
                return "-webkit-";

            switch (HttpContext.Current.Request.Browser.Browser.ToLower())
            {
                case "webkit":
                case "applewebkit":
                case "ipad":
                case "iphone":
                case "android":
                case "safari":
                case "chrome":
                case "applemac-safari":
                    return "-webkit-";
                case "firefox":
                    return "-moz-";
                case "opera":
                    return "-o-";
                case "ie":
                    return "-ms-";
                default:
                    return ""; // Assuming standard compliance ...
            }
        }

        internal bool IsTrackingViewState
        {
            get { return _trackingViewState; }
        }

        internal void LoadViewState(string state)
        {
            if (string.IsNullOrEmpty(state))
                return;
            state = state.Trim();

            string[] stylePairs = state.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            
            foreach (string idx in stylePairs)
            {
                string[] raw = idx.Split(':');
                StyleValue v = new StyleValue();
                v.ViewStateValue = raw[1];
                _styleValues[TransformFromViewStateShorthand(raw[0])] = v;
            }
        }

        private string TransformFromViewStateShorthand(string key)
        {
            switch (key)
            {
                case "a":
                    return "background";
                case "b":
                    return "background-color";
                case "c":
                    return "border";
                case "d":
                    return "cursor";
                case "e":
                    return "display";
                case "f":
                    return "position";
                case "g":
                    return "height";
                case "h":
                    return "width";
                case "i":
                    return "font";
                case "j":
                    return "margin";
                case "k":
                    return "padding";
                case "l":
                    return "left";
                case "m":
                    return "overflow";
                case "n":
                    return "right";
                case "o":
                    return "top";
                case "p":
                    return "z-index";
                case "q":
                    return "color";
                case "r":
                    return "text-align";
                case "s":
                    return "opacity";
                case "t":
                    return "bottom";
                case "u":
                    return "line-height";
                case "v":
                    return "background-image";
                case "w":
                    return "background-position";
                case "x":
                    return "border-color";
                case "y":
                    return "border-width";
                case "z":
                    return "font-family";
                case "1":
                    return "font-size";
                default:
                    return key;
            }
        }

        private string TransformToViewStateShorthand(string key)
        {
            switch (key)
            {
                case "background":
                    return "a";
                case "background-color":
                    return "b";
                case "border":
                    return "c";
                case "cursor":
                    return "d";
                case "display":
                    return "e";
                case "position":
                    return "f";
                case "height":
                    return "g";
                case "width":
                    return "h";
                case "font":
                    return "i";
                case "margin":
                    return "j";
                case "padding":
                    return "k";
                case "left":
                    return "l";
                case "overflow":
                    return "m";
                case "right":
                    return "n";
                case "top":
                    return "o";
                case "z-index":
                    return "p";
                case "color":
                    return "q";
                case "text-align":
                    return "r";
                case "opacity":
                    return "s";
                case "bottom":
                    return "t";
                case "line-height":
                    return "u";
                case "background-image":
                    return "v";
                case "background-position":
                    return "w";
                case "border-color":
                    return "x";
                case "border-width":
                    return "y";
                case "font-family":
                    return "z";
                case "font-size":
                    return "1";
                default:
                    return key;
            }
        }

        internal object SaveViewState()
        {
            return GetViewStateOnlyStyles();
        }

        internal void TrackViewState()
        {
            _trackingViewState = true;
        }
    }
}
