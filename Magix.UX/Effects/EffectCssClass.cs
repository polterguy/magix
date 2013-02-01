/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectCssClass : Effect
    {
        private readonly string _cssClass;

        public EffectCssClass() 
            : base(null, 0)
        { }

        public EffectCssClass(Control control, string cssClass)
            : base(control, 1)
        {
            _cssClass = cssClass;
        }

        protected override string RenderImplementation(
            bool topLevel, 
            List<Effect> chainedEffects)
        {
            BaseWebControl ctrl = Control as BaseWebControl;
            if (ctrl != null)
            {
                ctrl.SetViewStateValue("CssClass", _cssClass);
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.CssClass"; }
        }

        protected override string GetOptions()
        {
            return "cssClass: '" + this._cssClass + "',";
        }

        protected override void ValidateEffect()
        {
            if (Control == null)
                throw new ArgumentException("Cannot have a CssClass effect which doesn't affect any Controls");
        }
    }
}
