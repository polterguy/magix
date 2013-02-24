/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;

namespace Magix.UX.Effects
{
    public class EffectTimeout : Effect
    {
        public EffectTimeout(int milliseconds)
            : base(null, milliseconds)
        { }

        protected override void ValidateEffect()
        { }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Timeout"; }
        }

        protected override string GetOptions()
        {
            return "";
        }
    }
}
