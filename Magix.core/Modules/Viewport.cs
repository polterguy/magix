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

namespace Magix.Core
{
    /*
     * base class for viewports
     */
    public abstract class Viewport : ActiveModule
    {
        /*
         * override to return default container
         */
        protected abstract string GetDefaultContainer();

        /*
         * override to return all containers, except the system containers
         */
        protected abstract string[] GetAllDefaultContainers();

        /*
         * contains all css files
         */
        private List<string> CssFiles
        {
            get
            {
                if (ViewState["CssFiles"] == null)
                    ViewState["CssFiles"] = new List<string>();
                return ViewState["CssFiles"] as List<string>;
            }
        }

        /*
         * contains all javascript files
         */
        private List<string> JavaScriptFiles
        {
            get
            {
                if (ViewState["JavaScriptFiles"] == null)
                    ViewState["JavaScriptFiles"] = new List<string>();
                return ViewState["JavaScriptFiles"] as List<string>;
            }
        }

        protected override void OnInit(EventArgs e)
		{
			Load +=
				delegate
				{
					Page_Load_Initializing();
		            if (!Manager.Instance.IsAjaxCallback && IsPostBack)
		            {
		                IncludeAllCssFiles();
		                IncludeAllJsFiles();
		            }
				};
			base.OnInit(e);
            Page_Init_Initializing();
		}

        private void Page_Init_Initializing()
        {
            Node node = new Node();
            node["is-postback"].Value = IsPostBack;

            ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "magix.viewport.page-init",
                node);
        }

		private void Page_Load_Initializing()
		{
			if (!IsPostBack)
			{
				// Checking to see if this is a remotely activated Active Event
				// And if so, raising the "magix.viewport.remote-event" active event
	            if (!Manager.Instance.IsAjaxCallback && 
	                Request.HttpMethod == "POST" &&
	                !string.IsNullOrEmpty(Page.Request["event"]))
	            {
					// We only raise events which are allowed to be remotely invoked
					if (ActiveEvents.Instance.IsAllowedRemotely(Page.Request["event"]))
					{
						Node node = new Node();

						if (!string.IsNullOrEmpty (Page.Request["params"]))
							node = Node.FromJSONString (Page.Request["params"]);

                        if (node.Contains("inspect"))
                            throw new ArgumentException("no events can be remotely inspected, through the [inspect] feature");

						RaiseActiveEvent(
							Page.Request["event"],
							node);

						Page.Response.Clear ();
						Page.Response.Write ("return:" + node.ToJSONString ());
						try
						{
							Page.Response.End ();
						}
						catch
						{
							; // Intentionally do nothing ...
						}
					}
				}
				else
				{
					RaiseActiveEvent("magix.viewport.page-load");
				}
			}
		}

        /*
         * helper, re-includes all css files
         */
        private void IncludeAllCssFiles()
        {
            foreach (string idx in CssFiles)
            {
                IncludeCssFile(idx);
            }
        }

        /*
         * helper, re-includes all javascript files
         */
        private void IncludeAllJsFiles()
        {
            foreach (string idx in JavaScriptFiles)
            {
                IncludeJavaScriptFile(idx);
            }
        }

        /*
         * helper for clearing controls from container, such that it gets un-registered
         */
		private void ClearControls(DynamicPanel dynamic)
        {
            foreach (Control idx in dynamic.Controls)
            {
                ActiveEvents.Instance.RemoveListener(idx);
            }
            dynamic.ClearControls();
        }

		/*
		 * clears the given container
         */
        [ActiveEvent(Name = "magix.viewport.clear-controls")]
		protected virtual void magix_viewport_clear_controls(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.clear-controls-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.clear-controls-sample]");
                return;
            }

            Node dp = Dp(e.Params);
            string container = Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string;

            if (ip.ContainsValue("all") && ip["all"].Get<bool>())
            {
                foreach (string idx in GetAllDefaultContainers())
                {
                    DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
                        this,
                        idx);
                    ClearControls(dyn);
                }
            }
            else
            {
                DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
                    this,
                    container);

                if (dyn == null)
                    return;
                ClearControls(dyn);
            }
		}

		/*
		 * executes the given javascript
		 */
		[ActiveEvent(Name = "magix.viewport.execute-javascript")]
		protected void magix_viewport_execute_javascript(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.execute-javascript-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.execute-javascript-sample]");
                return;
			}

            if (!ip.ContainsValue("script"))
				throw new ArgumentException("no [script] given to [magix.viewport.execute-script]");

            Node dp = Dp(e.Params);

            string script = Expressions.GetExpressionValue(ip["script"].Get<string>(), dp, ip, false) as string;
            if (ip["script"].Count > 0)
                script = Expressions.FormatString(dp, ip, ip["script"], script);

			Manager.Instance.JavaScriptWriter.Write(script);
		}

        /*
         * helper for re-including css files
         */
        private void IncludeCssFile (string cssFile)
		{
			if (!string.IsNullOrEmpty (cssFile))
			{
				if (cssFile.Contains("~"))
				{
					string appPath = HttpContext.Current.Request.Url.ToString ();
					appPath = appPath.Substring (0, appPath.LastIndexOf ('/'));
					cssFile = cssFile.Replace ("~", appPath);
				}
				if (Manager.Instance.IsAjaxCallback)
				{
					Manager.Instance.JavaScriptWriter.Write (
                        @"MUX.Element.prototype.includeCSS('<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />');", cssFile);
				}
				else
				{
					LiteralControl lit = new LiteralControl ();
					lit.Text = string.Format (@"
<link href=""{0}"" rel=""stylesheet"" type=""text/css"" />
",
                        cssFile);
					Page.Header.Controls.Add (lit);
				}
			}
		}

        /*
         * helper for including javascript file
         */
        private void IncludeJavaScriptFile(string javaScriptFile)
        {
            javaScriptFile = javaScriptFile.Replace("~/", GetApplicationBaseUrl());
            Manager.Instance.IncludeFileScript(javaScriptFile);
        }

        /*
         * includes a file on the client
         */
        [ActiveEvent(Name = "magix.viewport.include-client-file")]
		protected void magix_viewport_include_client_file(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.include-client-file-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.include-client-file-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("type"))
                throw new ArgumentException("no [type] given to [magix.viewport.include-client-file]");
            string type = Expressions.GetExpressionValue(ip["type"].Get<string>(), dp, ip, false) as string;

            if (!ip.ContainsValue("file"))
                throw new ArgumentException("no [file] given to [magix.viewport.include-client-file]");
            string file = Expressions.GetExpressionValue(ip["file"].Get<string>(), dp, ip, false) as string;

            if (type == "css")
			{
                if (!CssFiles.Contains(file))
                {
                    CssFiles.Add(file);
                    IncludeCssFile(file);
                }
			}
            else if (type == "javascript")
			{
                if (!JavaScriptFiles.Contains(file))
                {
                    JavaScriptFiles.Add(file);
                    IncludeJavaScriptFile(file);
                }
			}
			else
                throw new ArgumentException("only 'css' and 'javascript' are legal types in [magix.viewport.include-client-file]");
		}

		/*
		 * sets a viewstate object
		 */
		[ActiveEvent(Name = "magix.viewport.set-viewstate")]
		protected virtual void magix_viewport_set_viewstate(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.set-viewstate-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.set-viewstate-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.viewport.set-viewstate]");
            string name = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
            if (ip["id"].Count > 0)
                name = Expressions.FormatString(dp, ip, ip["id"], name);

            if (!ip.Contains("value"))
			{
				if (ViewState[name] != null)
					ViewState.Remove(name);
			}
			else
			{
                Node value = null;
                if (ip.ContainsValue("value") && ip["value"].Get<string>().StartsWith("["))
                    value = (Expressions.GetExpressionValue(ip["value"].Get<string>(), dp, ip, false) as Node).Clone();
                else
                    value = ip["value"].Clone();
				ViewState[name] = value;
			}
		}

		/*
		 * retrieves a viewstate object
		 */
		[ActiveEvent(Name = "magix.viewport.get-viewstate")]
		protected virtual void magix_viewport_get_viewstate(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.get-viewstate-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.get-viewstate-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
                throw new ArgumentException("no [id] given to [magix.viewport.get-viewstate]");
            string name = Expressions.GetExpressionValue(ip["id"].Get<string>(), dp, ip, false) as string;
            if (ip["id"].Count > 0)
                name = Expressions.FormatString(dp, ip, ip["id"], name);

			if (ViewState[name] != null && ViewState[name] is Node)
                ip.Add((ViewState[name] as Node).Clone());
		}

		/*
         * loads an active module
         */
        [ActiveEvent(Name = "magix.viewport.load-module")]
		protected virtual void magix_viewport_load_module(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.load-module-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.Core",
                    "Magix.Core.hyperlisp.inspect.hl",
                    "[magix.viewport.load-module-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            string container = GetDefaultContainer();
            if (ip.ContainsValue("container"))
                container = Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string;

			DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
            	this,
                container);

            if (dyn == null && ip.ContainsValue("container"))
				return;

            string moduleName = Expressions.GetExpressionValue(e.Params["name"].Get<string>(), dp, ip, false) as string;

            ClearControls(dyn);

            if (ip.ContainsValue("class"))
                dyn.Class = Expressions.GetExpressionValue(ip["class"].Get<string>(), dp, ip, false) as string;

            if (ip.ContainsValue("style"))
            {
                // clearing old styles
                foreach (string idx in dyn.Style.Keys)
                {
                    dyn.Style[idx] = "";
                }

                // setting new styles
                string[] styles =
                    ip["style"].Get<string>().Replace("\n", "").Replace("\r", "").Split(
                        new char[] { ';' },
                        StringSplitOptions.RemoveEmptyEntries);
                foreach (string idxStyle in styles)
                {
                    dyn.Style[idxStyle.Split(':')[0]] = idxStyle.Split(':')[1];
                }
            }

            dyn.LoadControl(moduleName, e.Params);
        }

		/*
         * reloading controls upon postbacks
		 */
        protected void dynamic_LoadControls(object sender, DynamicPanel.ReloadEventArgs e)
        {
            DynamicPanel dynamic = sender as DynamicPanel;
            Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(e.Key);
            if (e.FirstReload)
            {
                ActiveModule module = ctrl as ActiveModule;
                if (module != null)
                {
                    Node nn = e.Extra as Node;
                    ctrl.Init +=
                        delegate
                        {
                            module.InitialLoading(nn);
                        };
                }
            }
            dynamic.Controls.Add(ctrl);
        }
    }
}
