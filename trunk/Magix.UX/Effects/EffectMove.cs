/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectMove : Effect
    {
        private int _left;
        private int _top;

        public EffectMove()
            : this(null, 0)
        { }

        public EffectMove(Control control, int milliseconds)
            : this(control, milliseconds, -1, -1)
        { }

        public EffectMove(Control control, int milliseconds, int left, int top)
            : base(control, milliseconds)
        {
            _left = left;
            _top = top;
        }

        public EffectMove(int left, int top)
            : base(null, 0)
        {
            _left = left;
            _top = top;
        }

        public int Top
        {
            get { return _top; }
            set { _top = value; }
        }

        public int Left
        {
            get { return _left; }
            set { _left = value; }
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Move"; }
        }

        protected override string GetOptions()
        {
            return "x:" + _left + ",y:" + _top + ",";
        }

        protected override string RenderImplementation(bool topLevel, System.Collections.Generic.List<Effect> chainedEffects)
        {
            BaseWebControl tmp = this.Control as BaseWebControl;
            if (tmp != null)
            {
                if (_left != -1)
                    tmp.Style.SetStyleValueViewStateOnly("left", _left.ToString() + "px");
                if (_top != -1)
                    tmp.Style.SetStyleValueViewStateOnly("top", _top.ToString() + "px");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }
    }
}
