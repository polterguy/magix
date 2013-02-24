/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Collections.Generic;
using System.Web;

namespace Magix.UX
{
    public static class Selector
    {
        public static T SelectFirst<T>(Control from, Predicate<Control> predicate) where T : Control
        {
            if (predicate(from))
                return from as T;
            foreach (Control idx in from.Controls)
            {
                T tmpRetVal = SelectFirst<T>(idx, predicate);
                if (tmpRetVal != null)
                    return tmpRetVal;
            }
            if (from is Page)
            {
                // User tries to locate a control from the page object, and no control was found
                // Checking to see if a MasterPage is attaced, and if so loop through the MasterPage
                if ((HttpContext.Current.CurrentHandler as Page).Master != null)
                {
                    foreach (Control idx in (HttpContext.Current.CurrentHandler as Page).Master.Controls)
                    {
                        T tmpRetVal = SelectFirst<T>(idx, predicate);
                        if (tmpRetVal != null)
                            return tmpRetVal;
                    }
                }
            }
            return null;
        }

        public static IEnumerable<T> Select<T>(Control from, Predicate<Control> predicate) where T : Control
        {
            if (from is T)
            {
                if (predicate(from))
                    yield return from as T;
            }
            foreach (Control idx in from.Controls)
            {
                foreach (T idxInner in Select<T>(idx, predicate))
                {
                    yield return idxInner;
                }
            }
        }

        public static T SelectFirst<T>(Control from) where T : Control
        {
            return SelectFirst<T>(from,
                delegate(Control idx)
                {
                    return idx is T;
                });
        }

        public static T FindControl<T>(Control from, string id) where T : Control
        {
            return SelectFirst<T>(from,
                delegate(Control idx)
                {
                    return idx.ID == id;
                });
        }

        public static T FindControlClientID<T>(Control from, string clientId) where T : Control
        {
            return SelectFirst<T>(from,
                delegate(Control idx)
                {
                    return idx.ClientID == clientId;
                });
        }

        public static IEnumerable<T> Select<T>(Control from) where T : Control
        {
            return Select<T>(from,
                delegate(Control idx)
                {
                    return idx is T;
                });
        }
    }
}
