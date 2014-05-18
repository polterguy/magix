/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.Web;
using System.Web.UI;
using System.Configuration;
using Magix.Core;

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
            if (!IsPostBack)
            {
                Page.Title = ConfigurationManager.AppSettings["magix.core.portal-name"];
            }
            InitializeViewport();
        }

        private void InitializeViewport()
        {
            // must de-reference this class before raising active events, to make sure active events are wired
            ModuleControllerLoader tmp = ModuleControllerLoader.Instance;

            Node viewport = new Node();
            Magix.Core.ActiveEvents.Instance.RaiseActiveEvent(
                this,
                "magix.viewport.load-viewport",
                viewport);
            Form.Controls.Add(viewport["viewport"].Value as Control);
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