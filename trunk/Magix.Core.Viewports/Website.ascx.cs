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

namespace Magix.Core.Viewports
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

		public void Page_Load (object sender, EventArgs e)
		{
			messageLabel.Text = "";
			if (!IsPostBack)
			{
				Node node = new Node();
				node["IsPostBack"].Value = false;
                ActiveEvents.Instance.RaiseActiveEvent(
                    this,
                    "Magix.Core.PageLoad",
					node);
			}
		}

        /**
         */
        [ActiveEvent(Name = "Magix.Core.ShowMessage")]
		protected void Magix_Core_ShowMessage (object sender, ActiveEventArgs e)
		{
			if (e.Params.Count == 0 || !e.Params.Contains ("Message"))
			{
				e.Params["Message"].Value = "Message to show to End User";
			}
			else
			{
				messageLabel.Text += "<p>" + e.Params["Message"].Get<string>() + "</p>";
				new EffectFadeIn(messageWrapper, 500)
					.ChainThese (
						new EffectTimeout(3000),
						new EffectFadeOut(messageWrapper, 500))
					.Render ();
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
         */
        [ActiveEvent(Name = "Magix.Core.ClearControls")]
		protected void Magix_Core_ClearControls (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Container") || 
			    string.IsNullOrEmpty (e.Params ["Container"].Get<string> ()))
			{
				e.Params["Container"].Value = "content1|content2";
			}
			else
			{
				DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
	                this, 
	                e.Params ["Container"].Get<string> ());

				ClearControls (dyn);
			}
		}

		/**
         */
        [ActiveEvent(Name = "Magix.Core.LoadModule")]
		protected void Magix_Core_LoadModule (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Container") || 
			    string.IsNullOrEmpty (e.Params ["Container"].Get<string> ()))
			{
				e.Params["Name"].Value = "Name of Class that Implements Module";
				e.Params["Container"].Value = "content1|content2|content3";
			}
			else
			{
				string moduleName = e.Params ["Name"].Get<string> ();

				DynamicPanel dyn = Selector.FindControl<DynamicPanel> (
                this, 
                e.Params ["Container"].Get<string> ());

				ClearControls (dyn);
				dyn.LoadControl (moduleName, e.Params);
			}
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
