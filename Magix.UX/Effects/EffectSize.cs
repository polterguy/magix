/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Effects
{
    public class EffectSize : Effect
    {
        private int _height;
        private int _width;

        public EffectSize()
            : this(null, 0)
        { }

        public EffectSize(Control control, int milliseconds)
            : this(control, milliseconds, -1, -1)
        { }

        public EffectSize(Control control, int milliseconds, int width, int height)
            : base(control, milliseconds)
        {
            _height = height;
            _width = width;
        }

        public EffectSize(int width, int height)
            : this(null, 0, width, height)
        { }

        public int Width
        {
            get { return _width; }
            set { _width = value; }
        }

        public int Height
        {
            get { return _height; }
            set { _height = value; }
        }

        protected override string NameOfEffect
        {
            get { return "MUX.Effect.Size"; }
        }

        protected override string GetOptions()
        {
            return "x:" + _width + ",y:" + _height + ",";
        }

        protected override string RenderImplementation(bool topLevel, 
            List<Effect> chainedEffects)
        {
            BaseWebControl tmp = this.Control as BaseWebControl;
            if (tmp != null)
            {
                if (_height != -1)
                    tmp.Style.SetStyleValueViewStateOnly("height", this._height.ToString() + "px");
                if (_width != -1)
                    tmp.Style.SetStyleValueViewStateOnly("width", this._width.ToString() + "px");
            }
            return base.RenderImplementation(topLevel, chainedEffects);
        }
    }
}
