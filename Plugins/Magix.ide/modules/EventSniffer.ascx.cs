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

namespace Magix.ide.modules
{
    /*
     * tracer tool
     */
    public class EventSniffer : ActiveModule
    {
		protected Label lbl;

		/*
		 * reloads the tracer when page is posting back
		 */
        [ActiveEvent(Name = "magix.viewport.page-init")]
        public static void magix_viewport_page_init(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.viewport.page-init-dox].value");
                return;
            }
            if (!ip["is-postback"].Get<bool>())
            {
                // reloading tracer if we should
                if (HttpContext.Current.Session["magix.tracer.toggle-tracer"] != null)
                {
                    Node tracerNode = new Node();
                    tracerNode["container"].Value = "trace";
                    tracerNode["name"].Value = "Magix.ide.modules.EventSniffer";

                    RaiseActiveEvent(
                        "magix.viewport.load-module",
                        tracerNode);
                }
            }
        }

		/*
         * toggles tracer
		 */
        [ActiveEvent(Name = "magix.tracer.toggle-tracer")]
        public static void magix_tracer_toggle_tracer(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.tracer.toggle-tracer-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.tracer.toggle-tracer-sample]");
                return;
            }

            if (HttpContext.Current.Session["magix.tracer.toggle-tracer"] == null)
            {
                Node tracerNode = new Node();
                tracerNode["container"].Value = "trace";
                tracerNode["name"].Value = "Magix.ide.modules.EventSniffer";

                RaiseActiveEvent(
                    "magix.viewport.load-module",
                    tracerNode);

                HttpContext.Current.Session["magix.tracer.toggle-tracer"] = true;
            }
            else
            {
                Node clearNode = new Node();
                clearNode["container"].Value = "trace";

                RaiseActiveEvent(
                    "magix.viewport.clear-controls",
                    clearNode);

                HttpContext.Current.Session.Remove("magix.tracer.toggle-tracer");
            }
        }

		/*
         * handles magix.execute
		 */
		[ActiveEvent(Name = "magix.execute")]
		public void magix_execute(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.execute-dox].value");
                return;
            }
            Node toCodeNode = new Node();
            toCodeNode["node"].Value = ip;

			RaiseActiveEvent(
				"magix.execute.node-2-code",
				toCodeNode);

            if (toCodeNode.ContainsValue("code"))
            {
                string code = toCodeNode["code"].Get<string>();
                code = code.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");
                string origin = "origin: " + e.Params.Name;
                code = "<label class=\"span-22 last\">" + origin + "</label><pre class=\"span-22 last bottom-1\">" + code + "</pre>";
                lbl.Value += code;
            }
		}
	}
}
