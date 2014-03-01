/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.ComponentModel;
using Magix.UX.Builder;
using Magix.UX.Helpers;
using Magix.UX.Effects;
using System.Web.UI;

namespace Magix.UX.Widgets.Core
{
    /**
     * Abstract base class for widgets which are HTML FORM or multi-choice type of elements
     */
    public abstract class BaseWebControlListFormElement : BaseWebControlFormElement
    {
        private ListItemCollection _listItems;
        private string _selectedItemValue;

        public BaseWebControlListFormElement()
        {
            _listItems = new ListItemCollection(this);
        }

        /**
         * A collection of all the items you have inside of your select list.
         * This will be automatically parse through your .ASPX syntax if you
         * declare items inside of the .ASPX file, or you can also programmatically 
         * add items in your codebehind file.
         */
        [PersistenceMode(PersistenceMode.InnerDefaultProperty)]
        public ListItemCollection Items
        {
            get
            {
                return _listItems;
            }
            internal set
            {
                _listItems = value;
            }
        }

        /**
         * Will return the currently selected item or set the currently selected item.
         * There are multiple duplicates of this property, like for instance the 
         * SelectedIndex property. The default SelectedItem will always be the
         * first (zero'th) element, regardless of which property you're using to
         * retrieve it.
         */
        public ListItem SelectedItem
        {
            get
            {
                if (_selectedItemValue == null)
                {
                    if (Items.Count == 0)
                        return null;
                    return Items[0];
                }
                return Items.Find(
                    delegate(ListItem idx)
                    {
                        return idx.Value == _selectedItemValue;
                    });
            }
            set
            {
                _selectedItemValue = value.Value;
                if (IsTrackingViewState)
                {
                    this.SetJsonValue("Value", value.Value);
                }
                SelectedIndex = Items.IndexOf(value);
            }
        }

        /**
         * Will return the index of the currently selected item or set the currently 
         * selected item based on its index.
         * There are multiple duplicates of this property, like for instance the 
         * SelectedItem property. The default SelectedItem will always be the
         * first (zero'th) element, regardless of which property you're using to
         * retrieve it. Meaning that the default value of this property will always
         * be '0'.
         */
        public int SelectedIndex
        {
            get
            {
                if (Items == null || Items.Count == 0)
                    return -1;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (Items[i].Selected)
                        return i;
                }
                return 0;
            }
            set
            {
                if (value == SelectedIndex)
                    return;
                for (int i = 0; i < Items.Count; i++)
                {
                    if (i == value)
                        _selectedItemValue = Items[i].Value;
                }
                if (IsTrackingViewState)
                {
                    SetValue();
                }
            }
        }

        protected override void TrackViewState()
        {
            Items.TrackViewState();
            base.TrackViewState();
        }

        protected override object SaveViewState()
        {
            object[] retVal = new object[2];
            retVal[0] = Items.SaveViewState();
            ViewState["_selectedItemValue"] = _selectedItemValue;
            retVal[1] = base.SaveViewState();
            return retVal;
        }

        protected override void LoadViewState(object savedState)
        {
            object[] content = savedState as object[];
            Items.LoadViewState(content[0]);
            base.LoadViewState(content[1]);
            if (_selectedItemValue == null)
                _selectedItemValue = (string)ViewState["_selectedItemValue"];
        }

        protected override void SetValue()
        {
            string newVal = Page.Request.Params[ClientID];
            if (newVal != null)
                _selectedItemValue = newVal;
        }

        public void SetSelectedItemAccordingToValue(string val)
        {
            foreach (ListItem idx in Items)
            {
                if (idx.Value == val)
                {
                    idx.Selected = true;
                    break;
                }
            }
        }
    }
}