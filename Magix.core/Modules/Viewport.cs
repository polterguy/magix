/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
    /**
     * Inherit your Viewports from this class to get most of the functionality 
     * you need in your viewports for free
     */
    public abstract class Viewport : ActiveModule
    {
		protected override void OnInit (EventArgs e)
		{
			Load +=
				delegate
				{
					Page_Load_Initializing();
		            if (!AjaxManager.Instance.IsCallback && IsPostBack)
		            {
		                IncludeAllCssFiles();
		                IncludeAllJsFiles();
		            }
				};
			base.OnInit (e);
		}

        protected abstract string GetDefaultContainer();

		private void Page_Load_Initializing ()
		{
			if (!IsPostBack)
			{
				// Checking to see if this is a remotely activated Active Event
				// And if so, raising the "magix.viewport.remote-event" active event
	            if (!AjaxManager.Instance.IsCallback && 
	                Request.HttpMethod == "POST" &&
	                !string.IsNullOrEmpty(Page.Request["event"]))
	            {
					// We only raise events which are allowed to be remotely invoked
					if (ActiveEvents.Instance.IsAllowedRemotely (Page.Request["event"]))
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
					Node node = new Node();
					node["initial-load"].Value = null;

					ActiveEvents.Instance.RaiseActiveEvent(
	                    this,
	                    "magix.viewport.page-load",
						node);
				}
			}
		}

        private void IncludeAllCssFiles()
        {
            foreach (string idx in CssFiles)
            {
                IncludeCssFile(idx);
            }
        }

        private void IncludeAllJsFiles()
        {
            foreach (string idx in JsFiles)
            {
                IncludeJsFile(idx);
            }
        }

		private void ClearControls(DynamicPanel dynamic)
        {
            foreach (Control idx in dynamic.Controls)
            {
                ActiveEvents.Instance.RemoveListener(idx);
            }
            dynamic.ClearControls();
        }

		/**
		 * Will clear the controls of the given "container" Viewport container
         */
        [ActiveEvent(Name = "magix.viewport.clear-controls")]
		protected virtual void magix_viewport_clear_controls(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["container"].Value = "content1";
				e.Params["inspect"].Value = @"will empty the given [container]
viewport container for all of its controls.&nbsp;&nbsp;
unloads a container for controls";
				return;
			}

			DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
                this,
                Ip(e.Params)["container"].Get<string>());

			if (dyn == null)
				return;

			ClearControls(dyn);
		}

		/**
		 * executes the given javascript
		 */
		[ActiveEvent(Name = "magix.viewport.execute-javascript")]
		protected void magix_viewport_execute_javascript(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["execute:magix.viewport.execute-javascript"]["script"].Value = "alert('{0}');";
				e.Params["execute:magix.viewport.execute-javascript"]["script"]["x"].Value = "thomas";
				e.Params["inspect"].Value = @"executes the javascript given in [script] using the values
of its children as string.format values.&nbsp;&nbsp;
not thread safe";
				return;
			}

            if (!Ip(e.Params).Contains("script"))
				throw new ArgumentException("you need a [script] value to execute javascript");

            string script = Ip(e.Params)["script"].Get<string>();

            if (Ip(e.Params)["script"].Count > 0)
			{
                for (int idx = 0; idx < Ip(e.Params)["script"].Count; idx++)
				{
                    script = script.Replace("{" + idx + "}", Ip(e.Params)["script"][idx].Get<string>());
				}
			}

			AjaxManager.Instance.WriterAtBack.Write(script);
		}

        private List<string> CssFiles
        {
            get
            {
                if (ViewState["CssFiles"] == null)
                    ViewState["CssFiles"] = new List<string>();
                return ViewState["CssFiles"] as List<string>;
            }
        }

        private List<string> JsFiles
        {
            get
            {
                if (ViewState["CssFiles"] == null)
                    ViewState["CssFiles"] = new List<string>();
                return ViewState["CssFiles"] as List<string>;
            }
        }

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
				if (AjaxManager.Instance.IsCallback)
				{
					AjaxManager.Instance.WriterAtBack.Write (
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

        private void IncludeJsFile(string jsFile)
        {
            jsFile = jsFile.Replace("~/", GetApplicationBaseUrl());
            AjaxManager.Instance.IncludeScriptFromFile(jsFile);
        }

		/**
		 * Will include a file on the client side of the given "type", which can
		 * be "JavaScript" or "CSS"
         */
        [ActiveEvent(Name = "magix.viewport.include-client-file")]
		protected void magix_viewport_include_client_file(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"includes either a css file or a 
javascript file on the client side.&nbsp;&nbsp;not thread safe";
				e.Params["type"].Value = "css";
				e.Params["file"].Value = "media/main-debug.css";
				return;
			}
            if (!Ip(e.Params).Contains("type"))
				throw new ArgumentException("You need to submit a type of file to load, legal values are 'css' and 'javascript'");
            if (Ip(e.Params)["type"].Get<string>() == "css")
			{
                string cssFile = Ip(e.Params)["file"].Get<String>();
                if (!CssFiles.Contains(cssFile))
                {
                    CssFiles.Add(cssFile);
                    IncludeCssFile(cssFile);
                }
			}
            else if (Ip(e.Params)["type"].Get<string>() == "javascript")
			{
                string js = Ip(e.Params)["file"].Get<String>();
                if (!JsFiles.Contains(js))
                {
                    JsFiles.Add(js);
                    IncludeJsFile(js);
                }
			}
			else
                throw new ArgumentException("Only type of javascript and css are legal inclusion files, you tried to include a file of type; " + Ip(e.Params)["type"].Get<string>());
		}

		/**
		 * sets a viewstate object
		 */
		[ActiveEvent(Name = "magix.viewport.set-viewstate")]
		protected virtual void magix_viewport_set_viewstate(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"stores [value] node hierarchy into viewstate
with value as key for later retrieval.
&nbsp;&nbsp;not thread safe";
				e.Params["magix.viewport.set-viewstate"].Value = "some-id";
				e.Params["magix.viewport.set-viewstate"]["value"]["some-node"]["hierarchy"].Value = "some node hierarchy to store into viewstate";
				return;
			}

            string id = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("need an id as a value to [magix.viewport.set-viewstate]");

            if (!Ip(e.Params).Contains("value"))
			{
				if (ViewState[id] != null)
					ViewState.Remove(id);
			}
			else
			{
                Node value = Ip(e.Params)["value"].Clone();
				ViewState[id] = value;
			}
		}

		/**
		 * retrieves a viewstate object
		 */
		[ActiveEvent(Name = "magix.viewport.get-viewstate")]
		protected virtual void magix_viewport_get_viewstate(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"retrieves [id] viewstate value, and puts
it into [value] node.&nbsp;&nbsp;not thread safe";
				e.Params["id"].Value = "some-id";
				return;
			}

            string id = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(id))
				throw new ArgumentException("need an id as a value to [magix.viewport.get-viewstate]");

			if (ViewState[id] != null && ViewState[id] is Node)
			{
                Ip(e.Params)["value"].Value = (ViewState[id] as Node).Value;
                Ip(e.Params)["value"].ReplaceChildren((ViewState[id] as Node).Clone());
			}
		}

		/**
		 * Will load an Active Module and put it into the "container" viewport container
         */
        [ActiveEvent(Name = "magix.viewport.load-module")]
		protected virtual void magix_viewport_load_module(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["name"].Value = "namespace.module_name";
				e.Params["container"].Value = "content1";
				e.Params["css"].Value = "span-24";
				e.Params["inspect"].Value = @"loads an active module into the 
given [container] viewport container.&nbsp;&nbsp;the module name must be defined in 
the [name] node.&nbsp;&nbsp;the incoming parameters will be used.&nbsp;&nbsp;not thread safe";
				return;
			}

            string moduleName = Ip(e.Params)["name"].Get<string>();

            string container = GetDefaultContainer();

            if (Ip(e.Params).Contains("container"))
                container = Ip(e.Params)["container"].Get<string>();

			DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
            	this,
                container);

			if (dyn == null)
				return;

			dyn.Style[Styles.display] = "";

            Node context = Ip(e.Params);

			ClearControls(dyn);

            if (Ip(e.Params).Contains("css"))
                dyn.CssClass = Ip(e.Params)["css"].Get<string>();
            else
                dyn.CssClass = "";

			dyn.LoadControl(moduleName, context);
        }

		/*
		 * Event handler for reloading controls back into Dynamic Panel
		 */
        protected void dynamic_LoadControls(object sender, DynamicPanel.ReloadEventArgs e)
        {
            DynamicPanel dynamic = sender as DynamicPanel;
            Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(e.Key);
            if (e.FirstReload)
            {
				// Since this is the Initial Loading of our module
				// We'll need to make sure our Initial Loading procedure is being
				// called, if Module is of type ActiveModule
                Node nn = e.Extra as Node;
                ctrl.Init +=
                    delegate
                    {
                        ActiveModule module = ctrl as ActiveModule;
                        if (module != null)
                        {
                            module.InitialLoading(nn);
                        }
                    };
            }
            dynamic.Controls.Add(ctrl);
        }
    }
}
