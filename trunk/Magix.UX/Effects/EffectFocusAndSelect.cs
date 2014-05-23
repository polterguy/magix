/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectFocusAndSelect : Effect
    {
        private bool isOnlyFocus;

        public EffectFocusAndSelect(Control control)
            : base(control, 1)
        {
        }

        public EffectFocusAndSelect(Control control, bool onlyFocus)
            : base(control, 1)
        {
            isOnlyFocus = onlyFocus;
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.FocusSelect"; }
        }

        protected override string GetOptions()
        {
            if (isOnlyFocus)
                return "isFocus:true,";
            return "";
        }
    }
}
