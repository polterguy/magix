/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectRollUp : Effect
    {
        private string _overflow;

        public EffectRollUp()
            : this(null, 0)
        { }

        public EffectRollUp(Control control, int milliseconds)
            : this(control, milliseconds, null)
        { }

        public EffectRollUp(Control control, int milliseconds, string overflow)
            : base(control, milliseconds)
        {
            _overflow = overflow;
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.RollUp"; }
        }

        protected override string GetOptions()
        {
            if (!string.IsNullOrEmpty(_overflow))
                return "overflow:'" + _overflow + "',";
            return "";
        }

        protected override string RenderImplementation(bool topLevel, List<Effect> chainedEffects)
        {
            BaseWebControl tmp = this.Control as BaseWebControl;
            if (tmp != null)
            {
                tmp.Style.SetStyleValueViewStateOnly("height", "");
                tmp.Style.SetStyleValueViewStateOnly("display", "none");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }
    }
}
