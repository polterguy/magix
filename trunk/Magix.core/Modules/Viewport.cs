/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
    public class Viewport : ActiveModule
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

						node["remote"].Value = true;

						RaiseEvent (
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
		protected void magix_viewport_clear_controls (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["container"].Value = "content2";
				e.Params["inspect"].Value = @"Will empty the given ""container""
viewport container for all of its controls. Unloads a container for controls.";
				return;
			}
			DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
                this, 
                e.Params ["container"].Get<string> ());

			ClearControls (dyn);

			// We must raise this event to signal to other controls
			// that Active Events MIGHT have been removed from the list
			// of Active Active Events
			Node node = new Node();
			RaiseEvent ("magix.execute._event-override-removed", node);
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
				if (cssFile.Contains ("~"))
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
            jsFile = jsFile.Replace("~/", GetApplicationBaseUrl()).ToLowerInvariant();
            AjaxManager.Instance.IncludeScriptFromFile(jsFile);
        }

		/**
		 * Will include a file on the client side of the given "type", which can
		 * be "JavaScript" or "CSS"
         */
        [ActiveEvent(Name = "magix.viewport.include-client-file")]
		protected void magix_viewport_include_client_file (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = "Includes either a CSS file or a JavaScript file on the client side";
				e.Params["type"].Value = "CSS";
				e.Params["file"].Value = "media/main-debug.css";
				return;
			}
			if (!e.Params.Contains ("type"))
				throw new ArgumentException("You need to submit a type of file to load, legal values are 'CSS' and 'JavaScript'");
			if (e.Params["type"].Get<string>() == "CSS")
			{
                string cssFile = e.Params["file"].Get<String>();
                if (!CssFiles.Contains(cssFile))
                {
                    CssFiles.Add(cssFile);
                    IncludeCssFile(cssFile);
                }
			}
			else if (e.Params["type"].Get<string>() == "JavaScript")
			{
                string js = e.Params["file"].Get<String>();
                if (!JsFiles.Contains(js))
                {
                    JsFiles.Add(js);
                    IncludeJsFile(js);
                }
			}
			else
				throw new ArgumentException("Only type of JavaScript and CSS are legal inclusion files, you tried to include a file of type; " + e.Params["type"].Get<string>());
		}

		/**
		 * Will load an Active Module and put it into the "container" viewport container
         */
        [ActiveEvent(Name = "magix.viewport.load-module")]
		protected void magix_viewport_load_module (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["name"].Value = "FullNameSpace.AndAlso.FullName_OfClassThatImplementsModule";
				e.Params["container"].Value = "content2";
				e.Params["inspect"].Value = @"Loads an active module into the 
given ""container"" viewport container. The module name must be defined in 
the ""name"" node. The incoming parameters will be used.";
				return;
			}
			string moduleName = e.Params ["name"].Get<string> ();

			DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
            	this, 
            	e.Params ["container"].Get<string> ());

			Node context = e.Params;

			ClearControls (dyn);
			dyn.LoadControl (moduleName, context);
			Node node = new Node();
			RaiseEvent ("magix.execute._event-overridden", node);
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
				// caled, if Module is of type ActiveModule
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
