/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.UX.Widgets
{
    public class StyleValue
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
}
