/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.Collections.Generic;

namespace Magix.UX.Effects
{
    public abstract class Effect
    {
        public enum Transition
        {
            Linear,

            Accelerating,

            Explosive,
        };

        private Control _control;
        private int _milliseconds;
		private Transition _type = Transition.Explosive;
        private List<Effect> _joined = new List<Effect>();
        private List<Effect> _chained = new List<Effect>();
        private bool _autoRun = true;
        private string _condition;

        public Control Control
        {
            get { return _control; }
        }

        public int Milliseconds
        {
            get { return _milliseconds; }
        }

        public Transition TransitionType
        {
            get { return _type; }
            set { _type = value; }
        }

        public string Condition
        {
            get { return _condition; }
            set { _condition = value; }
        }

        public List<Effect> Joined
        {
            get { return _joined; }
        }

        public List<Effect> Chained
        {
            get { return _chained; }
        }

        protected Effect(Control control, int milliseconds)
		{
            AjaxManager.Instance.IncludeScriptFromResource("Effects.js");
            _control = control;
			_milliseconds = milliseconds;
            _type = Transition.Explosive;
        }

        public Effect ChainThese(params Effect[] chainedEffects)
        {
            _chained.AddRange(chainedEffects);
            return this;
        }

        public Effect JoinThese(params Effect[] joinedEffects)
        {
            _joined.AddRange(joinedEffects);
            return this;
        }

        public void Render()
        {
            List<Effect> chained = new List<Effect>(_chained);
            AjaxManager.Instance.WriterAtBack.WriteLine(RenderImplementation(true, chained));
        }

        public string RenderString()
        {
            this._autoRun = false;
            List<Effect> chained = new List<Effect>(_chained);
            return RenderImplementation(true, chained).Trim(';');
        }

        protected virtual string RenderImplementation(bool topLevel, List<Effect> chainedEffects)
        {
            foreach (Effect idx in Joined)
            {
                if (idx._control == null)
                    idx._control = this.Control;
                idx._milliseconds = -1;
            }
            string joined = "";
            foreach(Effect idx in Joined)
            {
                joined += idx.RenderImplementation(false, idx.Chained) + ",";
            }
            joined.Trim(',');
            string chained = "null";
            if (chainedEffects.Count > 0)
            {
                Effect next = chainedEffects[0];
                chainedEffects.RemoveAt(0);
                chained = next.RenderImplementation(false, chainedEffects);
            }

            ValidateEffect();
            return string.Format(
                "new {5}('{0}', {{{6}joined: [{1}],{9}chained: {2},duration: {3},transition: '{4}'{7}}}){8}",
                (_control == null ? null : _control.ClientID),
                joined,
                chained,
                _milliseconds,
                TransitionType,
                NameOfEffect,
                GetOptions(),
                (topLevel && _autoRun ? "" : ",autoStart:false"),
                topLevel ? ";" : "",
                string.IsNullOrEmpty(Condition) ? "" : ("condition:" + Condition + ","));
        }

        protected abstract string NameOfEffect
        {
            get;
        }

        protected abstract string GetOptions();

        protected virtual void ValidateEffect()
        {
            if (this._control == null || this._milliseconds == 0)
                throw new ArgumentException("Cannot have an effect which affects no Control or has zero value for Duration property");
        }
    }
}
