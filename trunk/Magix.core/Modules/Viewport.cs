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

						RaiseEvent(
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
                e.Params["container"].Get<string> ());

			if (dyn == null)
				return;

			ClearControls(dyn);

			// We must raise this event to signal to other controls
			// that Active Events MIGHT have been removed from the list
			// of Active Active Events
			Node node = new Node();

			RaiseEvent(
				"magix.execute._event-override-removed", 
				node);
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

			if (!e.Params.Contains("script"))
				throw new ArgumentException("you need a [script] value to execute javascript");

			string script = e.Params["script"].Get<string>();

			if (e.Params["script"].Count > 0)
			{
				for (int idx = 0; idx < e.Params["script"].Count; idx++)
				{
					script = script.Replace("{" + idx + "}", e.Params["script"][idx].Get<string>());
				}
			}

			AjaxManager.Instance.WriterAtBack.Write(script);
		}

		/**
		 * creates a new effect
         */
		[ActiveEvent(Name = "magix.forms.effect")]
		protected void magix_forms_effect(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["inspect"].Value = @"creates an effect on the 
[id] element in the viewport lasting for [time] milliseconds [joined] with and chaining
executing [chained] afterwards.&nbsp;&nbsp;
different effects have different properties.&nbsp;&nbsp;not thread safe";
				e.Params["type"].Value = "fade-in|fade-out|focus-and-select|highlight|move|roll-up|roll-down|size|slide|timeout";
				e.Params["id"].Value = "idOfMyWidget";
				e.Params["time"].Value = 500M;
				e.Params["joined"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["type"].Value = "fade-in";
				e.Params["chained"]["e0"]["time"].Value = 500M;
				return;
			}
			Effect tmp = CreateEffect(e.Params);

			tmp.Render();
		}

		private Effect CreateEffect(Node pars)
		{
			string id = null;
			Control ctrl = null;
			if (pars.Contains("id"))
			{
				id = pars["id"].Get<string>();
				ctrl = Selector.FindControl<Control>(this, id);
			}

			Effect tmp = null;
			switch (pars["type"].Get<string>())
			{
				case "fade-in":
			{
				decimal frm = 0.0M;
				decimal to = 1.0M;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("from"))
					frm = pars["from"].Get<decimal>();
				if (pars.Contains("to"))
					to = pars["to"].Get<decimal>();
				tmp = new EffectFadeIn(ctrl, milliseconds, frm, to);
			} break;
				case "fade-out":
			{
				decimal frm = 1.0M;
				decimal to = 0.0M;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("from"))
					frm = pars["from"].Get<decimal>();
				if (pars.Contains("to"))
					to = pars["to"].Get<decimal>();
				tmp = new EffectFadeOut(ctrl, milliseconds, frm, to);
			} break;
				case "focus-and-select":
			{
				if (string.IsNullOrEmpty(id))
					throw new ArgumentException("focus-and-select effect needs an [id] for widget to select");
				tmp = new EffectFocusAndSelect(ctrl);
			} break;
				case "highlight":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectHighlight(ctrl, milliseconds);
			} break;
				case "move":
			{
				int left = -1;
				int top = -1;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("left"))
					left = pars["left"].Get<int>();
				if (pars.Contains("top"))
					top = pars["top"].Get<int>();
				tmp = new EffectMove(ctrl, milliseconds, left, top);
			} break;
				case "roll-up":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectRollUp(ctrl, milliseconds);
			} break;
				case "roll-down":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectRollDown(ctrl, milliseconds);
			} break;
				case "size":
			{
				int width = -1;
				int height = -1;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("width"))
					width = pars["width"].Get<int>();
				if (pars.Contains("height"))
					height = pars["height"].Get<int>();
				tmp = new EffectSize(ctrl, milliseconds, width, height);
			} break;
				case "slide":
			{
				int offset = -1;
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				if (pars.Contains("offset"))
					offset = pars["offset"].Get<int>();
				tmp = new EffectSlide(ctrl, milliseconds, offset);
			} break;
				case "timeout":
			{
				int milliseconds = -1;
				if (pars.Contains("time"))
					milliseconds = pars["time"].Get<int>();
				tmp = new EffectTimeout(milliseconds);
			} break;
			}

			if (pars.Contains("joined"))
			{
				foreach (Node idx in pars["joined"])
				{
					Effect tp = CreateEffect(idx);
					tmp.Joined.Add(tp);
				}
			}

			if (pars.Contains("chained"))
			{
				foreach (Node idx in pars["chained"])
				{
					Effect tp = CreateEffect(idx);
					tmp.Chained.Add(tp);
				}
			}

			return tmp;
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
			if (!e.Params.Contains("type"))
				throw new ArgumentException("You need to submit a type of file to load, legal values are 'css' and 'javascript'");
			if (e.Params["type"].Get<string>() == "css")
			{
                string cssFile = e.Params["file"].Get<String>();
                if (!CssFiles.Contains(cssFile))
                {
                    CssFiles.Add(cssFile);
                    IncludeCssFile(cssFile);
                }
			}
			else if (e.Params["type"].Get<string>() == "javascript")
			{
                string js = e.Params["file"].Get<String>();
                if (!JsFiles.Contains(js))
                {
                    JsFiles.Add(js);
                    IncludeJsFile(js);
                }
			}
			else
				throw new ArgumentException("Only type of javascript and css are legal inclusion files, you tried to include a file of type; " + e.Params["type"].Get<string>());
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
				e.Params["css"].Value = "span12";
				e.Params["inspect"].Value = @"loads an active module into the 
given [container] viewport container.&nbsp;&nbsp;the module name must be defined in 
the [name] node.&nbsp;&nbsp;the incoming parameters will be used.&nbsp;&nbsp;not thread safe";
				return;
			}

			string moduleName = e.Params["name"].Get<string>();

			DynamicPanel dyn = Selector.FindControl<DynamicPanel>(
            	this, 
            	e.Params["container"].Get<string>());

			if (dyn == null)
				return;

			Node context = e.Params;

			ClearControls(dyn);

			if (e.Params.Contains("css"))
				dyn.CssClass = e.Params["css"].Get<string>();

			dyn.LoadControl(moduleName, context);

			Node node = new Node();

			RaiseEvent(
				"magix.execute._event-overridden", 
				node);
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
