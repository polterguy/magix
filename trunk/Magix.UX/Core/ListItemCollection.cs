/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Collections.Generic;
using System.Collections;
using System.Web.UI;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    public class ListItemCollection : ICollection, IList<ListItem>, IStateManager
    {
        private readonly List<ListItem> _list = new List<ListItem>();
        private readonly BaseWebControlListFormElement _control;
        private bool _trackingViewState;

        public ListItemCollection(BaseWebControlListFormElement control)
        {
            _control = control;
        }

        public ListItem Find(Predicate<ListItem> functor)
        {
            foreach (ListItem idx in _list)
            {
                if (functor(idx))
                    return idx;
            }
            return null;
        }

        public void AddRange(IEnumerable<ListItem> items)
        {
            foreach (ListItem item in items)
            {
                Add(item);
            }
        }

        public ListItem FindByValue(string value)
        {
            return Find(
                delegate(ListItem idx)
                {
                    return idx.Value == value;
                });
        }

        public void CopyTo(Array array, int index)
        {
            _list.CopyTo(array as ListItem[], index);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return _list; }
        }

        public IEnumerator GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public int IndexOf(ListItem item)
        {
            return _list.IndexOf(item);
        }

        public void Insert(int index, ListItem item)
        {
            _list.Insert(index, item);
            item.SelectList = this._control;
            _control.ReRender();
        }

        public void RemoveAt(int index)
        {
            _list.RemoveAt(index);
            _control.ReRender();
        }

        public ListItem this[int index]
        {
            get
            {
                return _list[index];
            }
            set
            {
                _list[index] = value;
                value.SelectList = this._control;
                _control.ReRender();
            }
        }

        public void Add(ListItem item)
        {
            _list.Add(item);
            item.SelectList = this._control;
            _control.ReRender();
        }

        public void Clear()
        {
            _list.Clear();
            _control.ReRender();
        }

        public bool Contains(ListItem item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(ListItem[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ListItem item)
        {
            _control.ReRender();
            return _list.Remove(item);
        }

        IEnumerator<ListItem> IEnumerable<ListItem>.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        public bool IsTrackingViewState
        {
            get { return _trackingViewState; }
        }

        public void LoadViewState(object state)
        {
            _list.Clear();
            object[] values = state as object[];
            int count = (int)values[0];
            for (int idx = 0; idx < count; idx++)
            {
                object[] listItemViewState = values[idx + 1] as object[];
                ListItem idxItem = new ListItem();
                idxItem.Enabled = (bool)listItemViewState[0];
                idxItem.Text = listItemViewState[1].ToString();
                idxItem.Value = listItemViewState[2].ToString();
                idxItem.SelectList = this._control;
                _list.Add(idxItem);
            }
        }

        public object SaveViewState()
        {
            object[] retVal = new object[Count + 1];
            retVal[0] = Count;
            int idxNo = 1;
            foreach (ListItem idxItem in this)
            {
                object[] listItemViewState = new object[3];
                listItemViewState[0] = idxItem.Enabled;
                listItemViewState[1] = idxItem.Text;
                listItemViewState[2] = idxItem.Value;
                retVal[idxNo++] = listItemViewState;
            }
            return retVal;
        }

        public void TrackViewState()
        {
            _trackingViewState = true;
        }
    }
}
