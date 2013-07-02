/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.ComponentModel;
using System.Web.UI;
using System.Web.UI.WebControls;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    public class ListItem
    {
        private string _value;
        private string _text;
        private bool _enabled = true;
        private BaseWebControlListFormElement _selectList;
        private bool _hasSetSelectedTrue;

        public ListItem()
        { }

        public ListItem(string text, string value)
        {
            _text = text;
            _value = value;
        }

        public bool Selected
        {
            get
            {
                if (_selectList.SelectedItem == null)
                    return false;
                return _selectList.SelectedItem.Equals(this);
            }
            set
            {
                if (value)
                {
                    if (_selectList == null)
                        _hasSetSelectedTrue = true;
                    else
                        _selectList.SelectedItem = this;
                }
                else if (this.Selected)
                    _selectList.SelectedIndex = 0;
            }
        }

        internal BaseWebControlListFormElement SelectList
        {
            get { return _selectList; }
            set
            {
                _selectList = value;
                if (_hasSetSelectedTrue)
                    _selectList.SelectedItem = this;
            }
        }

        public bool Enabled
        {
            get { return _enabled; }
            set { _enabled = value; }
        }

        public string Text
        {
            get { return string.IsNullOrEmpty(_text) ? "" : _text; }
            set { _text = value; }
        }

        public string Value
        {
            get { return string.IsNullOrEmpty(_value) ? "" : _value; }
            set { _value = value; }
        }

        public override bool Equals(object obj)
        {
            ListItem rhs = obj as ListItem;
            if (rhs == null)
                return false;
            return rhs.Value == Value && rhs.Text == Text;
        }

        public override int GetHashCode()
        {
            return (Value + Text).GetHashCode();
        }
    }
}
