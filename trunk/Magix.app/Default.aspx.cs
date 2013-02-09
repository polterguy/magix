/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.Configuration;
using System.Web.UI;
using System.Text;
using System.IO;
using Magix.Core;
using Magix.UX;
using System.Web;
using System.Threading;

namespace Magix.app
{
    /**
     * Level4: Your 'Application Pool', meaning the 'world' where all your 'components' lives.
     * Nothing really to see here, this should just 'work'. But are here for reference reasons
     */
    public partial class MainWebPage : Page
    {
        protected void Page_Init(object sender, EventArgs e)
        {
            InitializeViewport();
        }

        private void InitializeViewport()
        {
            string defaultControl = ConfigurationManager.AppSettings["Magix.Core.Viewport"];
            Control ctrl = ModuleControllerLoader.Instance.LoadActiveModule(defaultControl);
            Form.Controls.Add(ctrl);
        }
    }
}