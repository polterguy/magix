/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;

namespace Magix.UX.Widgets
{
    /**
     * Control for making it easier to dynamically instantiate new controls and add 
     * them into your page. Completely abstracts away the entire hassle of storing 
     * whether or not a control has been previously loaded into the page or not. 
     * Make sure you handle the Reload event, and dependent upon the key given 
     * will load the exact same control and add it up into the DynamicPanel, 
     * and you should experience a very
     * smooth experience in regards to dynamically loading controls into your page.
     * Call the method LoadControl or AppendControl with a unique ID defining
     * which control you wish to load, for instance the name of a UserControl file
     * on disc, and until you explicitly clear your DynamicControl, the same control
     * will be automatically loaded every time. If you do not create a Reload Event 
     * Handler for your widget, and you call LoadControl, then the DynamicControl
     * will assume that what you're passing in is a fully qualifed path to a 
     * UserControl, and attempt to load it as such. You can use the Extra parameter
     * to add extra initialization parameters into the control upon its first load.
     */
    public class DynamicPanel : Panel
    {
        /**
         * EventArgs passed into your Reload event. The Key should be the unique ID
         * given when calling LoadControl. Extra is the extra parameter passed into
         * LoadControl/AppendControl.
         */
        public class ReloadEventArgs : EventArgs
        {
            private string _key;
            private object _extra;
            private bool _firstReload;
            private bool _insertAtBeginning;

            /**
             * The unique key or ID given while calling LoadControl or AppendControl.
             */
            public string Key
            {
                get { return _key; }
            }

            /**
             * Extra initialization parameters. Only passed in the first time. Meaning
             * when you call LoadControl from your code.
             */
            public object Extra
            {
                get { return _extra; }
            }

            /**
             * True if this is the first time the control is being loaded.
             */
            public bool FirstReload
            {
                get { return _firstReload; }
            }

            /**
             * If True, will load this control in at the beginning ...
             */
            public bool InsertAtBeginning
            {
                get { return _insertAtBeginning; }
            }

            internal ReloadEventArgs(string key, object extra, bool firstReload, bool insertAtBeginning)
            {
                _key = key;
                _extra = extra;
                _firstReload = firstReload;
                _insertAtBeginning = insertAtBeginning;
            }
        }

        private string _key;

        /**
         * Raised when the DynamicControl for some reasons need to load its control(s).
         * Make sure you load the exact same control every time you're given the same
         * Key value in the ReloadEventArgs parameter of your event handler. Also
         * make sure that the Key is uniquely identifying a control such that you
         * cannot get name clashes.
         */
        public event EventHandler<ReloadEventArgs> Reload;

        /**
         * The entire key for the controls loaded into your DynamicControl. Notice
         * that this might be a list of Keys, separated by pipe (|). Really only 
         * exposed for 'advanced functionality'. Do not in general terms use this
         * property for anything. It may be removed in future versions of MUX.
         */
        public string Key
        {
            get { return _key; }
            internal set { _key = value; }
        }

        protected override void LoadControlState(object savedState)
        {
            if (savedState != null)
            {
                object[] controlState = savedState as object[];
                if (controlState != null)
                {
                    if (controlState[0] != null && controlState[0].GetType() == typeof(string))
                        _key = controlState[0].ToString();
                    base.LoadControlState(controlState[1]);
                }
            }
        }

        protected override object SaveControlState()
        {
            object[] controlState = new object[2];
            controlState[0] = _key;
            controlState[1] = base.SaveControlState();
            return controlState;
        }

        protected override void OnLoad(EventArgs e)
        {
            if (Page.IsPostBack)
            {
                LoadDynamicControl(false, null);
            }
            base.OnLoad(e);
        }

        private void LoadDynamicControl(bool firstReload, object extra)
        {
            if (Reload != null && _key != null)
            {
                foreach (string idx in _key.Split('|'))
                {
                    if (!string.IsNullOrEmpty(idx))
                    {
                        ReloadEventArgs e = 
                            new ReloadEventArgs(
                                idx.IndexOf("<") == 0 ? idx.Substring(1) : idx, 
                                extra, 
                                firstReload,
                                idx.IndexOf("<") == 0);
                        Reload(this, e);
                    }
                }
            }
            else if(Reload == null && !string.IsNullOrEmpty(_key))
            {
                foreach (string idx in _key.Split('|'))
                {
                    if (!string.IsNullOrEmpty(idx))
                    {
                        if (idx.IndexOf("<") == 0)
                        {
                            Controls.AddAt(0, Page.LoadControl(idx.Substring(1)));
                        }
                        else
                        {
                            Controls.Add(Page.LoadControl(idx));
                        }
                    }
                }
            }
        }

        /**
         * Loads a new set of control(s) into the DynamicControl according
         * to the key given. This key will be passed into the Reload event
         * every time onwards until you call ClearControls or leave the page
         * or somehow makes the DynamicControl in-visible or destroy it.
         * The key parameter must be uniquely identifying a specific control
         * type. Make sure you reload the exact same control, every time, in 
         * your Reload event handler. If you have not defined a Reload 
         * Event Handler, the system will assume you're sending it the complete
         * path and name to a UserControl and attempt to load the given key
         * as a UserControl. See also AppendControl for having multiple controls
         * within the same DynamicControl. Notice that LoadControl
         * will clear any previously loaded controls from the DynamicControl.
         */
        public void LoadControl(string key)
        {
            LoadControl(key, null);
        }

        /**
         * Loads a new set of control(s) into the DynamicControl according
         * to the key given. This key will be passed into the Reload event
         * every time onwards until you call ClearControls or leave the page
         * or somehow makes the DynamicControl in-visible or destroy it.
         * The key parameter must be uniquely identifying a specific control
         * type. Make sure you reload the exact same control, every time, in 
         * your Reload event handler. If you have not defined a Reload 
         * Event Handler, the system will assume you're sending it the complete
         * path and name to a UserControl and attempt to load the given key
         * as a UserControl. See also AppendControl for having multiple controls
         * within the same DynamicControl. The extra parameter
         * will be passed into the Reload event. Notice that LoadControl
         * will clear any previously loaded controls from the DynamicControl.
         */
        public void LoadControl(string key, object extra)
        {
            Controls.Clear();

            _key = key;
            LoadDynamicControl(true, extra);
            ReRender();
        }

        /**
         * Appends a new set of control(s) into the DynamicControl according
         * to the key given. This key will be passed into the Reload event
         * every time onwards until you call ClearControls or leave the page
         * or somehow makes the DynamicControl in-visible or destroy it.
         * The key parameter must be uniquely identifying a specific control
         * type. Make sure you reload the exact same control, every time, in 
         * your Reload event handler. If you have not defined a Reload 
         * Event Handler, the system will assume you're sending it the complete
         * path and name to a UserControl and attempt to load the given key
         * as a UserControl. See also the LoadControl method. 
         */
        public void AppendControl(string key)
        {
            AppendControl(key, null);
        }

        /**
         * Appends a new set of control(s) into the DynamicControl according
         * to the key given. This key will be passed into the Reload event
         * every time onwards until you call ClearControls or leave the page
         * or somehow makes the DynamicControl in-visible or destroy it.
         * The key parameter must be uniquely identifying a specific control
         * type. Make sure you reload the exact same control, every time, in 
         * your Reload event handler. If you have not defined a Reload 
         * Event Handler, the system will assume you're sending it the complete
         * path and name to a UserControl and attempt to load the given key
         * as a UserControl. See also the LoadControl method. The extra parameter
         * will be passed into the Reload event.
         */
        public void AppendControl(string key, object extra)
        {
            AppendControl(key, extra, false);
        }

        // TODO: Document ...!!!
        private void AppendControl(string key, object extra, bool insertAtBeginning)
        {
            if (string.IsNullOrEmpty(_key))
                _key = "";
            if (insertAtBeginning)
                key = "<" + key;
            string newKey = string.IsNullOrEmpty(_key) ? key : _key + "|" + key;
            _key = key;
            LoadDynamicControl(true, extra);
            _key = newKey;
            ReRender();
        }

        /**
         * Clear the controls and makes sure they won't be (re-)loaded again on the
         * next request.
         */
        public void ClearControls()
        {
            Controls.Clear();
            _key = null;
            ReRender();
        }
    }
}
