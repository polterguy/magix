/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.admin
{
    /**
     * event sniffer
     */
    public class EventSniffer : ActiveModule
    {
		protected Label lbl;

		/**
		 * reloads the tracer when page is postback
		 */
        [ActiveEvent(Name = "magix.viewport.page-init")]
        public static void magix_viewport_page_init(object sender, ActiveEventArgs e)
        {
            if (!e.Params["is-postback"].Get<bool>())
            {
                // reloading tracer if we should
                if (HttpContext.Current.Session["magix.admin.toggle-tracer"] != null)
                {
                    Node tracerNode = new Node();
                    tracerNode["container"].Value = "trace";
                    tracerNode["name"].Value = "Magix.admin.EventSniffer";

                    ActiveEvents.Instance.RaiseActiveEvent(
                        typeof(EventSniffer),
                        "magix.viewport.load-module",
                        tracerNode);
                }
            }
        }

        /**
         * loads desktop shortcuts
         */
        [ActiveEvent(Name = "magix.admin.desktop-shortcuts.get-tracer")]
        public static void magix_admin_desktop_shortcuts_get_tracer(object sender, ActiveEventArgs e)
        {
            if (e.Params.Contains("inspect"))
            {
                e.Params["inspect"].Value = @"<p>loads the desktop shortcuts for 
toggling the tracer tool</p><p>thread safe</p>";
                return;
            }

            Node ip = Ip(e.Params);

            ip["load-tracer"]["text"].Value = "toggle tracer";
            ip["load-tracer"]["category"].Value = "tools";
            ip["load-tracer"]["code"]["magix.admin.toggle-tracer"].Value = null;
            ip["load-tracer"]["code"]["magix.help.open-file"]["file"].Value = "system42/admin/help/backend/tracer.mml";
        }

		/**
         * toggles tracer
		 */
        [ActiveEvent(Name = "magix.admin.toggle-tracer")]
        public static void magix_admin_toggle_tracer(object sender, ActiveEventArgs e)
        {
            if (e.Params.Contains("inspect"))
            {
                e.Params["inspect"].Value = @"<p>toggles the tracer tool, on and off</p>
<p>the tracer is a tool that allows you to see all hyperlisp code that is executed within 
the system in a tracer output on a per-session basis.&nbsp;&nbsp;takes no parameters</p>
<p>not thread safe</p>";
                return;
            }

            if (HttpContext.Current.Session["magix.admin.toggle-tracer"] == null)
            {
                Node tracerNode = new Node();
                tracerNode["container"].Value = "trace";
                tracerNode["name"].Value = "Magix.admin.EventSniffer";

                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(EventSniffer),
                    "magix.viewport.load-module",
                    tracerNode);

                HttpContext.Current.Session["magix.admin.toggle-tracer"] = true;
            }
            else
            {
                Node clearNode = new Node();
                clearNode["container"].Value = "trace";

                ActiveEvents.Instance.RaiseActiveEvent(
                    typeof(EventSniffer),
                    "magix.viewport.clear-controls",
                    clearNode);

                HttpContext.Current.Session.Remove("magix.admin.toggle-tracer");
            }
        }

		/**
         * handles magix.execute
		 */
		[ActiveEvent(Name = "magix.execute")]
		public void magix_execute(object sender, ActiveEventArgs e)
		{
			if (e.Params != null)
			{
                Node tmp = new Node();
                tmp["node"].Value = Ip(e.Params);

				RaiseActiveEvent(
					"magix.execute.node-2-code",
					tmp);

                if (tmp.Contains("code") && !string.IsNullOrEmpty(tmp["code"].Get<string>()))
                {
                    string code = tmp["code"].Get<string>();
                    code = code.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                    string origin = "origin: " + e.Params.Name;
                    code = "<label class=\"span-22 last\">" + origin + "</label><pre class=\"span-22 last bottom-1\">" + code + "</pre>";
                    lbl.Value += code;
                }
            }
		}
	}
}
