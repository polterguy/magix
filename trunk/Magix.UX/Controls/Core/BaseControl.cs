/*
 * Magix - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
    /**
     * Abstract base class for mostly all widgets in Magix UX. This is where the Ajax
     * 'core engine' to a large extent exists within Magix UX. Contains several 
     * interesting features, such as for instance the Info property, which can take 
     * any string information and serialize back and forth between server-requests. 
     * If you need to create your own Ajx Control, you should either directly, or most
     * probably indirectly, inherit from this MuxBaseControl.
     * If you inherit, directly or indirectly, form this class you need to override
     * several methods from this class. The most notable is RenderMuxControl.
     * If you have a 'visual widget', you will mostly inherit from MuxBaseWebControl
     * instead of this class. If your control is 'non-visual', such as the MUX Timer,
     * you will mostly inherit from this class.
     */
    [ViewStateModeById]
    public abstract class BaseControl : Control
	{
        private bool _hasBeenRendered;
        private bool _forceReRender;
        private Dictionary<string, object> _json = new Dictionary<string, object>();
        protected bool _gainedFocusThisRequest;

        /**
         * Event raised when the Widget is rendered for the firt time, meaning it will
         * be triggered upon re-renderings and such, but not during normal postbacks. But
         * obviously, if a parent Widget is being re-rendered, then also this Event will be
         * raised again, even though the Widget hasn't changed any of its properties.
         */
        public event EventHandler InitiallyLoaded;

        /**
         * Additional information you wish to attach to your widget somehow. Useful
         * for things such as Repeaters where you have the same event handler for
         * several buttons on your page, but need to know specifically which button
         * was actually pressed. 
         * PS!
         * Just like the HiddenField the value in this property should not be trusted
         * to be securely serialized since internally it uses the ViewState to serialize 
         * this additional information, and ViewState is (by default) not encrypted in
         * any ways.
         */
        public string Info
        {
            get { return ViewState["Info"] == null ? "" : (string)ViewState["Info"]; }
            set { ViewState["Info"] = value; }
        }

        protected override void LoadControlState(object state)
		{
            if (state != null)
			{
                // If there is any ControlState in our control, then unless some
                // control developer, further up in our inheritance hierarchy have messed
                // it completely up, this value should be a boolean, indicating whether
                // or not the control has been rendered, and hence we set the has rendered 
                // field to the value of this boolean.
				_hasBeenRendered = (bool)state;
			}
		}

		protected override object SaveControlState()
		{
            // The Visible part of our control must be tracked since it's the only
            // way we know how to serialize towards the control on the client-side.
            // Meaning, if the control has been rendered before, the rendering route
            // is completely different than the default rendering mechanism.
			return Visible;
		}

        /**
         * Will return true if this widget has been rendered before. Kind of
         * like an 'IsPostback logic' on a control level. Useful for determining
         * if this is the first time this control is being rendered, or not.
         */
        public bool HasRendered
        {
            get { return _hasBeenRendered; }
        }

        /**
         * Returns true if this control is either not rendered from before, or
         * has been signaled to re-render as HTML (and JS object registration)
         * Notice though that the control still might be rendering HTML due to
         * an ancestor control, any way upwards in this control's ancestor hierarchy
         * has been signaled to re-render. Meaning, this is not by any means any 
         * 'absolute definition' of whether or not this control will render HTML.
         * If you need absolute proof for some reasons of whether or not this
         * control will render as HTML you need to check this property, but also the
         * AreAncestorsRenderingHTML method to find out whether or not any of its 
         * ancestor controls are re-rendering.
         */
        public bool RenderingHtml
        {
            get { return _forceReRender || !HasRendered; }
        }

        /**
         * Forces the control to re-render as HTML. This is basically a revert
         * to UpdatePanel logic for those extreme cases where such is needed.
         * In general terms you should really only use this method if you either
         * are wrapping non-MUX controls inside of MUX Ajax controls, or you
         * have changed the control collection by either adding or removing new
         * controls from this control's Control collection. There exists several
         * tutorials on this matter at the MUX website.
         */
        public void ReRender()
		{
            if (HasRendered && AjaxManager.Instance.IsCallback)
            {
                // We only set this flag if the control has *not* been rendered
                // from before, and the current request is a MUX Request.
                _forceReRender = true;
            }
		}

        /**
         * Override this method to handle events that are being raised on 
         * the client-side. Theoretically you can raise events in code yourself
         * to mimick whatever event you wish to mimick, but 99% of the times you
         * think you would want to do such a thing, you're really doing something
         * very bad. DON'T...!
         * At least unless you know very well that this is exactly what you want 
         * to do.
         */
        public virtual void RaiseEvent(string name)
        {
            throw new ArgumentException(
                "DispatchEvent call bubbled all the way to the top and nothing would handle '" + 
                name + 
                "' for the control with the ClientID of '" + 
                this.ClientID + 
                "'.");
        }

        public void RaiseInitiallyLoaded()
        {
            if (InitiallyLoaded != null)
            {
                InitiallyLoaded(this, new EventArgs());
            }
        }

		public override void RenderControl(HtmlTextWriter writer)
		{
            if (DesignMode)
            {
                ControlVisibleForFirstTime(writer);
            }
            else
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
		}

        private void RenderVisibleControl(HtmlTextWriter writer)
        {
            if (AjaxManager.Instance.IsCallback)
            {
                // This is a MUX Ajax Callback...
                RenderVisibleControlInCallback(writer);
            }
            else
            {
                // This is *NOT* a MUX Ajax Callback, and hence we
                // render the whole thing as if it was never rendered before...
                // It may be a PostBack, but that is really irellevant...
                // It may also be an initial page rendering, but that too is 
                // irellevant...
                HtmlBuilder builder = new HtmlBuilder(writer);
                RenderMuxControl(builder);
                AjaxManager.Instance.Writer.WriteLine(GetClientSideScript());
            }
        }

        private void RenderVisibleControlInCallback(HtmlTextWriter writer)
        {
            if (AreAncestorsRenderingHTML())
            {
                // Some ancestor, some place upwards in this widget's hierarchy
                // is for some reasons being re-rendered...
                AncestorReRendering(writer);
            }
            else if (_forceReRender)
            {
                // This specific widget is being re-rendered...
                ReRenderWidget(writer);
            }
            else if (HasRendered)
            {
                // Here we're only rendering the JSON properties to the control...
                RenderJson(writer);
            }
            else
            {
                // Control was being made visible for the first time...
                ControlVisibleForFirstTime(writer);
            }
        }

        /**
         * Will return true, if for some reasons, any ancestor control are being
         * rendered as pure HTML. This might happen due to an ancestor control are being
         * re-rendered, or it may happen due to an ancestor control being set to visible
         * for the first time. This is in general terms an *EXPENSIVE* method call since
         * it needs to traverse the entire ancestor hierarchy until it hits the Page. Hence
         * be *CAREFUL* with using it...!!
         */
        public bool AreAncestorsRenderingHTML()
        {
            Control idx = Parent;
            while (idx != null)
            {
                BaseControl tmp = idx as BaseControl;
                if (tmp != null)
                {
                    if (tmp.RenderingHtml)
                        return true;
                }
                idx = idx.Parent;
            }
            return false;
        }

        /**
         * Only override this one if you truly know what you're doing, and you really
         * need some modified logic.
         */
        protected virtual void RenderJson(HtmlTextWriter writer)
        {
            string json = SerializeJson();
            if (!string.IsNullOrEmpty(json))
            {
                AjaxManager.Instance.Writer.WriteLine(
                    "MUX.Control.$('{1}').JSON({0});",
                    json,
                    ClientID);
            }
            RenderChildren(writer);
        }

        private void ReRenderWidget(HtmlTextWriter writer)
        {
            // Notice that we just 'discards' the given writer here, and
            // instead writes to our own Ajax stream here...
            // That's basically because all JavaScript goes into our
            // 'own' stream, and not in the main 'HTML stream'...
            // This is to make sure we can get it at the bottom of our response.
            // Since we need it to be the last thing rendered.
            using (HtmlBuilder builder = new HtmlBuilder())
            {
                RenderMuxControl(builder);
                AjaxManager.Instance.Writer.Write(
                    "\r\nMUX.Control.$('{0}').reRender('{1}');\r\n{2}",
                    ClientID,
                    builder.ToStringJson(),
                    GetChildrenClientSideScript(Controls));
            }
        }

        private void AncestorReRendering(HtmlTextWriter writer)
        {
            // Here we render into the given stream.
            // This is only the HTML portions of our control.
            // The JavaScript initialization string will be rendered
            // another place.
            // In fact, the JavaScript registration will be rendered at
            // the *END* of the Parent widget that was re-rendering its content
            // to make sure all HTML is present before any JS initialization
            // code is ran.
            HtmlBuilder builder = new HtmlBuilder(writer);
            RenderMuxControl(builder);
        }

        private void ControlVisibleForFirstTime(HtmlTextWriter writer)
        {
            using (HtmlBuilder builder = new HtmlBuilder())
            {
                RenderMuxControl(builder);
                AjaxManager.Instance.Writer.Write(
                    "MUX.$('{0}').repl('{1}');{2}{3}",
                    ClientID,
                    builder.ToStringJson(),
                    GetClientSideScript(),
                    GetChildrenClientSideScript(Controls));
            }
        }

        // Called when the Control is in-visible
        private void RenderInVisibleControl(HtmlTextWriter writer)
        {
            if (AjaxManager.Instance.IsCallback)
            {
                RenderInVisibleControlInCallback(writer);
            }
            else
            {
                writer.Write(GetControlInvisibleHTML());
            }
        }

        private void RenderInVisibleControlInCallback(HtmlTextWriter writer)
        {
            if (!this.AreAncestorsRenderingHTML() && HasRendered)
            {
                // Control has been rendered, visible, before, and we need
                // to 'destroy' it and replace it with its in-visible HTML...
                AjaxManager.Instance.Writer.Write(
                    "\r\nMUX.Control.$('{0}').destroy('{1}');",
                    ClientID,
                    GetControlInvisibleHTML());
            }
            else
            {
                writer.Write(GetControlInvisibleHTML());
            }
        }

        /**
         * Retrieves a specific JSON collection values. Used by the style classes and to
         * add up generic attributes to the DOM elements.
         */
		public Dictionary<string, string> GetJsonValues(string key)
		{
			if (!_json.ContainsKey(key))
			{
				_json[key] = new Dictionary<string, string>();
			}
			return (Dictionary<string, string>)_json[key];
		}

        /**
         * This one should be used in properties and such for control developers.
         * Please notice that there is no way we can track values being set before
         * the ViewState is finished loading, and hence all properties and such
         * being set before the ViewState is loaded will be ignored...
         */
		protected void SetJsonValue(string key, object value)
		{
            if (IsTrackingViewState)
            {
                _json[key] = value;
            }
		}

        /**
         * This method will set a 'generic' property for you control. Useful
         * for JS client-side mappings that doesn't really exists since these
         * key/values pairs will be rendered as additional attributes back to the
         * client.
         */
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

        /**
         * If called, this will set the focus to this widget on the client-side.
         * Please notice that there's no 'multiple calls guard' in this logic, which
         * means if you call this method for several controls, only the last method call
         * will actually 'succeed'...
         */
		public override void Focus()
		{
            if (AjaxManager.Instance.IsCallback)
                SetJsonValue("Focus", "");
            else
                _gainedFocusThisRequest = true;
		}

        // This method will be called if the control has already been rendered
        // and it is not currently being re-rendered. For those cases, all we
        // need to do is to send changes back to the client. This is being done
        // by passing over new/changed values as JSON, which again is being hendled
        // on the client-side.
		protected string SerializeJson()
		{
			if (_json.Count == 0)
				return null;
			StringBuilder values = new StringBuilder();
            foreach (string idx in _json.Keys)
            {
                object val = _json[idx];
                SerializeJSONValue(idx, val, values);
            }
			if (values.Length > 0)
				return "{" + values.ToString() + "}";
			return string.Empty;
		}

        /**
         * Helper method to serialize *ONE* JSON value. Can by default handle
         * a range of different types, such as Rectangle, Point, string, decimal,
         * int, bool etc. If you need to be able to serialize other types, then
         * this can easily be accomplished through overriding this method in
         * your own widget and handle that specific type/property yourself.
         */
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
                        throw new ArgumentException("Type cannot be serialized because we don't have a mapper for it. Something is seriously wrong in one of your custom widgets...!");
                    break;
            }
		}

        // This is where we serialize the 'generic value', which basically are
        // wrappers around attributes which do not have 'custom handlers' on
        // the client-side of MUX.
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

                    switch (idxKey)
                    {
                        case "backgroundImage":
                        {
                            if (val.Contains("linear-gradient"))
                            {
                                val = val.Replace(
                                    "linear-gradient",
                                    StyleCollection.GetBrowserPrefix() + "linear-gradient");
                            }
                        } break;
                    }

                    builder.AppendFormat("[\"{0}\",\"{1}\"]",
                        key2,
                        val.Replace("\\", "\\\\").Replace("\"", "\\\""));
                }
                builder.Append("]");
            }
        }

        // We override this one since we first of all need to make sure we have
        // all JS files included, secondly because we need to register the control
        // to ASP.NET as a ControlState control.
		protected override void OnInit(EventArgs e)
		{
            if (!DesignMode)
            {
                AjaxManager.Instance.InitializeControl(this);
                AjaxManager.Instance.IncludeMainRaScript();
                AjaxManager.Instance.IncludeMainControlScripts();
                Page.RegisterRequiresControlState(this);
            }
			base.OnInit(e);
		}

        // When a control, with children, are being re-rendered for some reason
        // its child controls must not render their JS initialization code
        // before the control being re-rendered is completely finished re-rendering.
        // Hence this method being called from the core rendering logic.
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

        /**
         * Method for registering the control on the client-side. Most control
         * will only need this base functionality, but most controls that have 
         * specific JavaScript needs, will need to override this method.
         * One example of a control that needs to override this method is
         * the MUX Timer.
         */
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

            return string.Format("\r\n{2}('{0}'{1});", 
				ClientID, 
				optionsString,
				GetClientSideScriptType());
		}

        /**
         * If the only thing you need to override when you're creating your own controls
         * are the 'JavaScript type', then you should override this method only to
         * change the JS type being sent back as the registering script to the client.
         * The default value is 'MUX.C', which is a shorthand for 'MUX.Control', which
         * is the common Control JS class for most controls in MUX.
         */
		protected virtual string GetClientSideScriptType()
		{
			return "MUX.C";
		}

        /**
         * This is the method you need to override, when creating your own controls, and
         * you have server-side events you need to handle. The return value of this method
         * is expected to be something such as "['click'],['mouseover']" etc. It will create
         * handlers for those DOM events on the client-side and automatically create an Ajax
         * Request going towards the server when those DOM events are raised.
         */
		protected virtual string GetEventsRegisterScript()
		{
			return string.Empty;
		}

        /**
         * This method makes it possible for you to return custom options according to
         * what properties/options your custom widget have. The method is expected to 
         * return something similar to "handle:'xyz',color:'Red'" which will automatically 
         * map towards the JS client-side options of your custom widget class.
         */
		protected virtual string GetClientSideScriptOptions()
		{
			if (_gainedFocusThisRequest)
				return "focus:true";
			return "";
		}

        /**
         * For Ajax Control creators, this is your most important method.
         * This is the method that expects you to build up the HTML markup
         * for your control. Most controls will only build one HTML DOM
         * element, but you can build any amount of complexity up with
         * the help of the HtmlBuilder class and its related classes.
         */
        protected virtual void RenderMuxControl(HtmlBuilder builder)
        { }

        /**
         * Override this method to add up attributes and their values 
         * to your custom widget. Only override this method if you're an Ajax
         * Custom Control builder. Within this method you're expected to only
         * put logic such as 'el.AddAttribute("attributeName", "attributeValue");'
         */
        protected virtual void AddAttributes(Element el)
        {
            el.AddAttribute("id", ClientID);
        }

        /**
         * Some, but very few, controls need specific in-visible HTML. The default
         * value of this method will be rendering a 'span' with the style value of 
         * display:none. Some custom widgets needs to be able to override this markup,
         * for instance because they they needs to render list-elements (HTML li element)
         * to be semantically correct. Override this method to make sure your HTML 
         * becomes semantically correct for such cases.
         */
		protected virtual string GetControlInvisibleHTML()
		{
            return string.Format("<span id=\"{0}\" style=\"display:none;\"></span>", ClientID);
        }
	}
}
