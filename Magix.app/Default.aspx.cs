/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.Configuration;
using System.Web.UI;
using Magix.Core;
using System.Web;

namespace Magix.app
{
    /**
     * Your 'Application Pool', meaning the 'world' where all your 'components' lives.
     * Nothing really to see here, this should just 'work'. But are here for reference reasons.
     * If you wish to change the design on your application, then don't do this here. Rather
     * use the Viewport for such changes. These files are intentionally intended to be 'empty'.
     */
    public partial class MainWebPage : Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            InitializeViewport();
        }

        private void InitializeViewport()
        {
			string defaultControl = ConfigurationManager.AppSettings["magix.core.viewport"];
            Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(defaultControl);
            Form.Controls.Add(ctrl);
        }

		private PageStatePersister _pageStatePersister;
		protected override System.Web.UI.PageStatePersister PageStatePersister
		{
			get
			{
				if (_pageStatePersister == null)
					_pageStatePersister = new Magix_PageStatePersister(this);
				return _pageStatePersister;
			}
		}
    }
}