/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System.Web.UI;
using System.Collections.Generic;
using Magix.UX.Widgets;

namespace Magix.UX.Effects
{
    public class EffectToggleElements : Effect
    {
        private List<Control> _elements = new List<Control>();
        private string _animationMode;

        public EffectToggleElements()
            : base(null, 0)
        { }

        public EffectToggleElements(Control control, int milliseconds)
            : base(control, milliseconds)
        { }

        public EffectToggleElements(Control control, int milliseconds, string animationMode, IEnumerable<Control> elements)
			: base(control, milliseconds)
		{
            _elements = new List<Control>(elements);
            _animationMode = animationMode;
		}

        public EffectToggleElements(IEnumerable<Control> elements)
            : base(null, 0)
        {
            _elements = new List<Control>(elements);
        }

        public EffectToggleElements(Control parent, IEnumerable<Control> elements)
            : base(parent, 0)
        {
            _elements = new List<Control>(elements);
        }

        public EffectToggleElements(Control parent, IEnumerable<Control> elements, int milliseconds)
            : base(parent, milliseconds)
        {
            _elements = new List<Control>(elements);
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.ToggleElements"; }
        }

        protected override string GetOptions()
        {
            string elements = "";
            foreach(Control idx in _elements)
            {
                elements += "'" + idx.ClientID + "',";
            }
            elements = elements.Trim(',');
            return "elements: [" + elements + "],animationMode:'" + _animationMode + "',";
        }
    }
}
