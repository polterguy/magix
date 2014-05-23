/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
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
            if (_control.Rendered && _styleValues.Count > 0)
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
                    throw new ArgumentException("no uppercase letters");

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
                    retVal += idxKey + ":" + value + ";";
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
                    retVal += idxKey + ":" + value + ";";
                }
            }
            return retVal;
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
                _styleValues[raw[0]] = v;
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
