/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - QueenOfSpades20122@gmail.com
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

namespace Magix.viewports
{
    /**
     */
    public class Website : ActiveModule
    {
        protected DynamicPanel content1;
        protected DynamicPanel content2;
        protected DynamicPanel content3;
		protected Panel messageWrapper;
		protected Label messageLabel;
		protected Label msgBoxHeader;

		public void Page_Load (object sender, EventArgs e)
		{
			messageLabel.Text = "";
			if (!IsPostBack)
			{
				Node node = new Node();
				node["initial-load"].Value = null;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "magix.viewport.page-load",
					node);
			}
		}

        /**
         */
        [ActiveEvent(Name = "magix.viewport.show-message")]
		protected void magix_viewport_show_message (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["message"].Value = "Message to show to End User";
				e.Params["header"].Value = "Message from system";
				e.Params["time"].Value = "1000";
				e.Params["inspect"].Value = @"Shows a message box to the 
end user for some seconds.";
				return;
			}
			messageLabel.Text += "<p>" + e.Params["message"].Get<string>() + "</p>";
			if (e.Params.Contains ("header"))
				msgBoxHeader.Text = e.Params["header"].Get<string>();
			int time = 3000;
			if (e.Params.Contains ("time"))
				time = int.Parse (e.Params["time"].Get<string>());
			new EffectRollDown(messageWrapper, 500)
				.JoinThese (
					new EffectFadeIn())
				.ChainThese (
					new EffectTimeout(time),
					new EffectRollUp(messageWrapper, 500)
						.JoinThese (
							new EffectFadeOut()))
				.Render ();
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
			Node node = new Node();
			RaiseEvent ("magix.execute._event-override-removed", node);
		}

		/**
         */
        [ActiveEvent(Name = "magix.viewport.load-module")]
		protected void magix_viewport_load_module (object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["Data"]["Database"].Value = "localhost";
				e.Params["name"].Value = "FullNameSpace.AndAlso.FullName_OfClassThatImplementsModule";
				e.Params["container"].Value = "content2";
				e.Params["context"].Value = "[Data]";
				e.Params["inspect"].Value = @"Loads an active module into the 
given ""container"" viewport container. The module name must be defined in 
the ""name"" node. If ""context"" is given, this will be passed into
the loading of the module, and used for initialization of the module.
Else the incoming parameters will be used.";
				return;
			}
			string moduleName = e.Params ["name"].Get<string> ();

			DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
            	this, 
            	e.Params ["container"].Get<string> ());

			Node context = e.Params;

			if (e.Params.Contains ("context"))
				context = Expressions.GetExpressionValue (
					e.Params["context"].Get<string>(), e.Params, e.Params) as Node;

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
