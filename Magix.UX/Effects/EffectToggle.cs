/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System.Web.UI;
using System.Collections.Generic;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectToggle : Effect
    {
        private bool _isFade;

        public EffectToggle()
            : base(null, 0)
        { }

        public EffectToggle(Control control, int milliseconds)
            : base(control, milliseconds)
        { }

        public EffectToggle(Control control, int milliseconds, bool isFade)
            : base(control, milliseconds)
        {
            _isFade = isFade;
        }

        protected override string RenderImplementation(bool topLevel, List<Effect> chainedEffects)
        {
            if ((Control as BaseWebControl).Style[Styles.display] == "none")
            {
                (Control as BaseWebControl).Style.SetStyleValueViewStateOnly("display", "block");
            }
            else
            {
                (Control as BaseWebControl).Style.SetStyleValueViewStateOnly("display", "none");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Toggle"; }
        }

        protected override string GetOptions()
        {
            if (_isFade)
                return "isFade:true,";
            return "";
        }
    }
}
