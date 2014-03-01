/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;


namespace Magix.UX.Widgets
{
    /**
     * Timer widget that will periodically call the server every n'th millisecond. You
     * can set its period through the 
     */
    public class Timer : BaseControl
    {
        /**
         * Raised periodically every Duration milliseconds
         */
        public event EventHandler Tick;

        /**
         * if true control is enabled and will raise the Tick events every Duration milliseconds, otherwise
         * will not raise tick events before enabled again
         */
        [DefaultValue(true)]
        public bool Enabled
        {
            get { return ViewState["Enabled"] == null ? true : (bool)ViewState["Enabled"]; }
            set
            {
                if (value != Enabled)
                    SetJsonValue("Enabled", value);
                ViewState["Enabled"] = value;
            }
        }

        /**
         * Milliseconds bewteen Tick events are raised
         */
        [DefaultValue(1000)]
        public int Interval
        {
            get { return ViewState["Interval"] == null ? 1000 : (int)ViewState["Interval"]; }
            set
            {
                if (value != Interval)
                    SetJsonValue("Interval", value);
                ViewState["Interval"] = value;
            }
        }

        protected override void OnPreRender(EventArgs e)
        {
            AjaxManager.Instance.IncludeScriptFromResource(
                typeof(Timer),
                "Magix.UX.Js.Timer.js");
            base.OnPreRender(e);
        }

        public override void RaiseEvent(string name)
        {
            switch (name)
            {
                case "tick":
                    if (Tick != null)
                        Tick(this, new EventArgs());
                    break;
            }
        }

		protected override string GetClientSideScriptOptions()
		{
			string retVal = base.GetClientSideScriptOptions();
            if (Enabled && Tick != null)
            {
                if (!string.IsNullOrEmpty(retVal))
                    retVal += ",";
                retVal += "enabled:true";
            }
			if (Interval != 1000)
			{
                if (!string.IsNullOrEmpty(retVal))
					retVal += ",";
				retVal += string.Format("interval:{0}", Interval);
			}
			return retVal;
		}

		protected override string GetClientSideScriptType()
		{
			return "new MUX.Timer";
		}

        protected override void RenderMuxControl(HtmlBuilder builder)
        {
            using (Element el = builder.CreateElement("span"))
            {
                AddAttributes(el);
                el.Write("&nbsp;");
            }
        }

        protected override void AddAttributes(Element el)
        {
            el.AddAttribute("style", "display:none;");
            base.AddAttributes(el);
        }

        public void ReStart()
        {
            SetJsonValue("Restart", true);
        }
    }
}
