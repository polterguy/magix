/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;

namespace Magix.UX.Effects
{
    public class EffectHighlight : Effect
    {
        public EffectHighlight()
            : base(null, 0)
        { }

        public EffectHighlight(Control control, int milliseconds)
			: base(control, milliseconds)
		{ }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Highlight"; }
        }

        protected override string GetOptions()
        {
            return "";
        }
    }
}
