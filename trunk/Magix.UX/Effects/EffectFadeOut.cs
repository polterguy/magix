/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Globalization;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectFadeOut : Effect
    {
        private decimal _from = 1.0M;
        private decimal _to = 0.0M;

        public EffectFadeOut()
            : base(null, 0)
        { }

        public EffectFadeOut(Control control, int milliseconds)
            : base(control, milliseconds)
        { }

        public EffectFadeOut(Control control, int milliseconds, decimal from, decimal to)
            : base(control, milliseconds)
        {
            _from = from;
            _to = to;
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Opacity"; }
        }

        protected override string GetOptions()
        {
            return string.Format("from:{0}, to:{1},",
                _from.ToString(CultureInfo.InvariantCulture),
                _to.ToString(CultureInfo.InvariantCulture));
        }

        protected override string RenderImplementation(bool topLevel, System.Collections.Generic.List<Effect> chainedEffects)
        {
            BaseWebControl tmp = this.Control as BaseWebControl;
            if (tmp != null)
            {
                tmp.Style.SetStyleValueViewStateOnly("display", "none");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }
    }
}
