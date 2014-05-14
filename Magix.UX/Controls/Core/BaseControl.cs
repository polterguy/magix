/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Text;
using System.Web.UI;
using System.Globalization;
using System.ComponentModel;
using System.Collections.Generic;
using System.Web;
using Magix.UX.Builder;
using System.Drawing;
using Magix.UX.Helpers;

namespace Magix.UX.Widgets.Core
{
    /*
     * base classe for all ajax controls
     */
    [ViewStateModeById]
    public abstract class BaseControl : Control
	{
        private bool _rendered;
        private bool _reRender;
        private Dictionary<string, object> _ajaxUpdates = new Dictionary<string, object>();
        protected bool _hasFocus;

        /*
         * raised when control is initially rendered
         */
        public event EventHandler InitiallyLoaded;

        /*
         * extra information
         */
        public string Info
        {
            get { return ViewState["Info"] == null ? "" : (string)ViewState["Info"]; }
            set { ViewState["Info"] = value; }
        }

        protected override void LoadControlState(object savedState)
		{
            if (savedState != null)
            {
                object[] controlState = savedState as object[];
                if (controlState != null)
                {
                    if (controlState[0] != null && controlState[0].GetType() == typeof(bool))
                        _rendered = (bool)controlState[0];
                    base.LoadControlState(controlState[1]);
                }
            }
		}

		protected override object SaveControlState()
		{
            object[] controlState = new object[2];
            controlState[0] = Visible;
            controlState[1] = base.SaveControlState();
            return controlState;
		}

        /*
         * returns true if control has been rendered before
         */
        public bool Rendered
        {
            get { return _rendered; }
        }

        /*
         * forces a re-rendering of control as plain html
         */
        public void ReRender()
		{
            if (Rendered && Manager.Instance.IsAjaxCallback)
            {
                // We only set this flag if the control has *not* been rendered
                // from before, and the current request is a MUX Request.
                _reRender = true;
            }
		}

        /*
         * raises events
         */
        public virtual void RaiseEvent(string name)
        {
            throw new ArgumentException(
                "oops, '" + name + "' for '" + this.ClientID + "' wasn't handled");
        }

		public override void RenderControl(HtmlTextWriter writer)
		{
            if (Visible)
            {
                RenderVisibleControl(writer);
            }
            else
            {
                RenderInVisibleControl(writer);
            }
		}

        private void RenderVisibleControl(HtmlTextWriter writer)
        {
            if (Manager.Instance.IsAjaxCallback)
            {
                RenderVisibleControlInCallback(writer);
            }
            else
            {
                HtmlBuilder builder = new HtmlBuilder(writer);
                RenderMuxControl(builder);
                Manager.Instance.InternalJavaScriptWriter.Write(GetClientSideScript());
            }
        }

        private void RenderVisibleControlInCallback(HtmlTextWriter writer)
        {
            if (IsReRendered())
                AncestorReRendering(writer);
            else if (_reRender)
                ReRenderWidget(writer);
            else if (Rendered)
                RenderJson(writer);
            else
                ControlVisibleForFirstTime(writer);
        }

        private bool IsReRendered()
        {
            Control idx = Parent;
            while (idx != null)
            {
                BaseControl tmp = idx as BaseControl;
                if (tmp != null)
                {
                    if (tmp._reRender || !tmp._rendered)
                        return true;
                }
                idx = idx.Parent;
            }
            return false;
        }

        protected virtual void RenderJson(HtmlTextWriter writer)
        {
            string json = SerializeJson();
            if (!string.IsNullOrEmpty(json))
            {
                Manager.Instance.InternalJavaScriptWriter.Write(
                    "MUX.Control.$('{1}').JSON({0});",
                    json,
                    ClientID);
            }
            RenderChildren(writer);
        }

        private void ReRenderWidget(HtmlTextWriter writer)
        {
            // discarding incoming writer
            using (HtmlBuilder builder = new HtmlBuilder())
            {
                RenderMuxControl(builder);
                Manager.Instance.InternalJavaScriptWriter.Write(
                    "MUX.Control.$('{0}').reRender('{1}');{2}",
                    ClientID,
                    builder.ToStringJson(),
                    GetChildrenClientSideScript(Controls));
            }
        }

        private void AncestorReRendering(HtmlTextWriter writer)
        {
            HtmlBuilder builder = new HtmlBuilder(writer);
            RenderMuxControl(builder);
        }

        private void ControlVisibleForFirstTime(HtmlTextWriter writer)
        {
            using (HtmlBuilder builder = new HtmlBuilder())
            {
                RenderMuxControl(builder);
                Manager.Instance.InternalJavaScriptWriter.Write(
                    "MUX.$('{0}').repl('{1}');{2}{3}",
                    ClientID,
                    builder.ToStringJson(),
                    GetClientSideScript(),
                    GetChildrenClientSideScript(Controls));
            }
        }

        private void RenderInVisibleControl(HtmlTextWriter writer)
        {
            if (Manager.Instance.IsAjaxCallback)
                RenderInVisibleControlInCallback(writer);
            else
                writer.Write(GetControlInvisibleHTML());
        }

        private void RenderInVisibleControlInCallback(HtmlTextWriter writer)
        {
            if (!this.IsReRendered() && Rendered)
                Manager.Instance.InternalJavaScriptWriter.Write(
                    "MUX.Control.$('{0}').destroy('{1}');",
                    ClientID,
                    GetControlInvisibleHTML());
            else
                writer.Write(GetControlInvisibleHTML());
        }

		internal Dictionary<string, string> GetJsonValues(string key)
		{
			if (!_ajaxUpdates.ContainsKey(key))
			{
				_ajaxUpdates[key] = new Dictionary<string, string>();
			}
			return (Dictionary<string, string>)_ajaxUpdates[key];
		}

		protected void SetJsonValue(string key, object value)
		{
            if (IsTrackingViewState)
            {
                _ajaxUpdates[key] = value;
            }
		}

		protected void SetJsonGeneric(string key, string value)
		{
			if (IsTrackingViewState)
			{
                // The 'Generic' JSON value is a special type of collection which
                // will be mapped towards additional attributes on the client-side...
				Dictionary<string, string> generic = GetJsonValues("Generic");
				generic[key] = value;
			}
		}

		public override void Focus()
		{
            if (Manager.Instance.IsAjaxCallback)
                SetJsonValue("Focus", "");
            else
                _hasFocus = true;
		}

		protected string SerializeJson()
		{
			if (_ajaxUpdates.Count == 0)
				return null;
			StringBuilder values = new StringBuilder();
            foreach (string idx in _ajaxUpdates.Keys)
            {
                object val = _ajaxUpdates[idx];
                SerializeJSONValue(idx, val, values);
            }
			if (values.Length > 0)
				return "{" + values.ToString() + "}";
			return string.Empty;
		}

		protected virtual void SerializeJSONValue(
            string key, 
            object value, 
            StringBuilder builder)
		{
            if (value == null)
            {
                if (builder.Length > 0)
                    builder.Append(",");
                builder.AppendFormat("\"{0}\":\"\"", key);
                return;
            }
            switch(value.GetType().ToString())
            {
                case "System.String":
                    if (builder.Length > 0)
                        builder.Append(",");
                    builder.AppendFormat("\"{0}\":\"{1}\"",
                        key,
                        // TODO: Create some sort of "EscapeMethod" in StringHelper or something...
                        value.ToString().Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r"));
                    break;
                case "System.Drawing.Color":
                    if (builder.Length > 0)
                        builder.Append(",");
                    Color color = (Color)value;
                    string tmp = ColorTranslator.ToHtml(color);
                    builder.AppendFormat(
                        "\"{0}\":\"{1}\"",
                        key,
                        tmp);
                    break;
                case "System.Boolean":
                    if (builder.Length > 0)
                        builder.Append(",");
                    builder.AppendFormat("\"{0}\":{1}",
                        key,
                        value.ToString().ToLower());
                    break;
                case "System.Int32":
                    if (builder.Length > 0)
                        builder.Append(",");
                    builder.AppendFormat("\"{0}\":{1}",
                        key,
                        value);
                    break;
                case "System.Decimal":
                    if (builder.Length > 0)
                        builder.Append(",");
                    builder.AppendFormat("\"{0}\":{1}",
                        key,
                        value);
                    break;
                case "System.Drawing.Rectangle":
                    if (builder.Length > 0)
                        builder.Append(",");
                    Rectangle rect = (Rectangle)value;
                    builder.AppendFormat(
                        "{0}:{{left:{1},top:{2},width:{3},height:{4}}}",
                        key,
                        rect.Left, rect.Top, rect.Width, rect.Height);
                    break;
                case "System.Drawing.Point":
                    if (builder.Length > 0)
                        builder.Append(",");
                    Point pt = (Point)value;
                    builder.AppendFormat(
                        "{0}:{{x:{1},y:{2}}}",
                        key,
                        pt.X, pt.Y);
                    break;
                case "System.DateTime":
                    if (builder.Length > 0)
                        builder.Append(",");
                    DateTime dt = (DateTime)value;
                    builder.AppendFormat(
                        "{0}:{1}",
                        key,
                        dt.ToString("yyyy:MM:dd HH:mm", CultureInfo.InvariantCulture));
                    break;
                default:
                    Dictionary<string, string> dictValues = value as Dictionary<string, string>;
                    if (dictValues != null)
                        SerializeGenericValues(key, dictValues, builder);
                    else
                        throw new ArgumentException("unknown type serialized as json in BaseControl");
                    break;
            }
		}

        private static void SerializeGenericValues(
            string key, 
            Dictionary<string, string> values, 
            StringBuilder builder)
        {
            if (values.Count > 0)
            {
                if (builder.Length > 0)
                    builder.Append(",");
                builder.AppendFormat("\"{0}\":[", key);
                bool first = true;
                foreach (string idxKey in values.Keys)
                {
                    if (first)
                        first = false;
                    else
                        builder.Append(",");

                    string val = values[idxKey];
                    string key2 = idxKey;

                    builder.AppendFormat("[\"{0}\",\"{1}\"]",
                        key2,
                        val.Replace("\\", "\\\\").Replace("\"", "\\\""));
                }
                builder.Append("]");
            }
        }

		protected override void OnInit(EventArgs e)
		{
            Manager.Instance.InitializePage(this);
            Manager.Instance.IncludeCoreJavaScript();
            Manager.Instance.IncludeCoreControlScripts();
            Page.RegisterRequiresControlState(this);
			base.OnInit(e);
		}

		private string GetChildrenClientSideScript(ControlCollection controls)
		{
            StringBuilder builder = new StringBuilder();
			foreach (Control idx in controls)
			{
				if (idx.Visible)
				{
					if (idx is BaseControl)
					{
						builder.Append((idx as BaseControl).GetClientSideScript());
					}
					builder.Append(GetChildrenClientSideScript(idx.Controls));
				}
			}
			return builder.ToString();
		}

        protected virtual string GetClientSideScript()
		{
            string options = GetClientSideScriptOptions();

            string evts = GetEventsRegisterScript();
            if (!string.IsNullOrEmpty(evts))
                evts = "evts:[" + evts + "]";

            string optionsString = options;
            optionsString = StringHelper.ConditionalAdd(evts, "", ",", optionsString);

            if (!string.IsNullOrEmpty(optionsString))
				optionsString = ",{" + optionsString + "}";

            return string.Format("{2}('{0}'{1});", 
				ClientID, 
				optionsString,
				GetClientSideScriptType());
		}

		protected virtual string GetClientSideScriptType()
		{
			return "MUX.C";
		}

		protected virtual string GetEventsRegisterScript()
		{
			return string.Empty;
		}

		protected virtual string GetClientSideScriptOptions()
		{
			if (_hasFocus)
				return "focus:true";
			return "";
		}

        protected virtual void RenderMuxControl(HtmlBuilder builder)
        { }

        protected virtual void AddAttributes(Element el)
        {
            el.AddAttribute("id", ClientID);
        }

		protected virtual string GetControlInvisibleHTML()
		{
            return string.Format("<span id=\"{0}\" style=\"display:none;\"></span>", ClientID);
        }
	}
}
