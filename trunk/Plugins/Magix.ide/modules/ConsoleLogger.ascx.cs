/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Web.UI;
using System.Globalization;
using System.Collections.Generic;
using Magix.Core;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.ide.modules
{
    /*
     * console logger
     */
    public class ConsoleLogger : ActiveModule
    {
		protected Label lbl;

        /*
         * no rows of log items to show in console, if -1, show all
         */
        private int Rows
        {
            get
            {
                if (ViewState["Rows"] == null)
                    return -1;
                return (int)ViewState["Rows"];
            }
            set { ViewState["Rows"] = value; }
        }

        /*
         * items that have been logged
         */
        private List<string> Content
        {
            get
            {
                if (ViewState["Content"] == null)
                    ViewState["Content"] = new List<string>();
                return (List<string>)ViewState["Content"];
            }
        }

        /*
         * initializes module
         */
        public override void InitialLoading(Node node)
        {
            base.InitialLoading(node);

            Node ip = Ip(node);
            int rows = ip.GetValue("rows", -1);

            Load +=
                delegate
                {
                    Rows = rows;
                };
        }

        /*
         * loads the console logger
         */
        [ActiveEvent(Name = "magix.console.load")]
        public static void magix_console_load(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.console.load-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.console.load-sample]");
                return;
            }

            Node exeNode = new Node("magix.viewport.load-module");
            exeNode["name"].Value = "Magix.ide.modules.ConsoleLogger";
            exeNode.AddRange(ip.Clone());

            Node newPars = e.Params.Clone();
            newPars["_root-only-execution"].Value = true;
            newPars["_ip"].Value = exeNode;
            newPars["_dp"].Value = exeNode;

            RaiseActiveEvent(
                "magix.execute",
                newPars);
        }

        /*
         * logs a piece of text to the console logger
         */
        [ActiveEvent(Name = "magix.console.log")]
        public void magix_console_log(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.console.log-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.console.log-sample]");
                return;
            }

            string message = Expressions.GetFormattedExpression("value", e.Params, null);

            if (string.IsNullOrEmpty(message))
                throw new Exception("no [value] given to [magix.console.log]");
            bool error = ip.GetValue("error", false);

            message = DateTime.Now.ToString("hh:MM:ss", CultureInfo.InvariantCulture) + ": " + message;

            if (error)
                message = "<p class=\"error\">" + message + "</p>";
            else
                message = "<p>" + message + "</p>";

            Content.Add(message);
            if (Rows != -1 && Content.Count > Rows)
                Content.RemoveAt(0);
        }

        protected override void OnPreRender(EventArgs e)
        {
            lbl.Value = "";
            foreach (string idx in Content)
            {
                lbl.Value += idx;
            }
            base.OnPreRender(e);
        }
	}
}
