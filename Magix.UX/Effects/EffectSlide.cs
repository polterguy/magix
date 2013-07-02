/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System.Web.UI;
using System.Collections.Generic;

namespace Magix.UX.Effects
{
    public class EffectSlide : Effect
    {
        private readonly int _offset;

        public EffectSlide()
            : base(null, 0)
        { }

        public EffectSlide(Control control, int milliseconds)
            : base(control, milliseconds)
        { }

        public EffectSlide(Control control, int milliseconds,int offset)
			: base(control, milliseconds)
		{
            _offset = offset;
        }

        public EffectSlide(int offset)
			: base(null, 0)
		{
            _offset = offset;
		}
		
        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Slide"; }
        }

        protected override string GetOptions()
        {
            return "offset:" + _offset + ",";
        }
    }
}
