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
    public class EffectBorder : Effect
    {
		private int _borderTo;

        public EffectBorder()
            : base(null, 0)
        { }

        public EffectBorder(Control control, int milliseconds)
            : base(control, milliseconds)
        { }

        public EffectBorder(Control control, int milliseconds, int borderTo)
			: base(control, milliseconds)
		{
			_borderTo = borderTo;
		}

        public EffectBorder(int borderTo)
			: base(null, 0)
		{
			_borderTo = borderTo;
		}
		
		public int BorderTo
		{
			get { return _borderTo; }
			set { _borderTo = value; }
		}

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Border"; }
        }

        protected override string GetOptions()
        {
            return "border: " + BorderTo + ",";
        }

        protected override string RenderImplementation(bool topLevel, List<Effect> chainedEffects)
        {
            BaseWebControl tmp = this.Control as BaseWebControl;
            if (tmp != null)
            {
                tmp.Style.SetStyleValueViewStateOnly("border-style", "dashed");
                tmp.Style.SetStyleValueViewStateOnly("border-width", BorderTo + "px");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }
    }
}
