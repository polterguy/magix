/*
 * Magix-BRIX - A Web Application Framework for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix-BRIX is licensed as GPLv3.
 */

using System;
using System.IO;
using System.Text;
using System.Web.UI;
using Magix.Core;
using Magix.UX;

namespace Magix.app
{
    public partial class Magix_PageStatePersister : PageStatePersister
	{
		private Guid _session;

		public Magix_PageStatePersister(Page page)
			: base(page)
		{
			if (page.IsPostBack)
            {
                _session = new Guid(page.Request["__VIEWSTATE_KEY"]);
            }
            else
            {
                _session = Guid.NewGuid();
            }
            if (!Manager.Instance.IsAjaxCallback)
            {
                LiteralControl lit = new LiteralControl();
                lit.Text = string.Format(@"
<input type=""hidden"" value=""{0}"" name=""__VIEWSTATE_KEY"" />", _session);
                page.Form.Controls.Add(lit);
            }
		}

		public override void Load()
		{
			string id = _session.ToString() + "|" + Page.Request.Url.ToString();
            LosFormatter formatter = new LosFormatter();

			if (Page.Session[id] == null)
				throw new ArgumentException("session timeout ...");

			string obj = Page.Session[id] as string;
            Pair pair = formatter.Deserialize(obj) as Pair;
            ViewState = pair.First;
            ControlState = pair.Second;
		}

		public override void Save()
		{
            LosFormatter formatter = new LosFormatter();
            StringBuilder builder = new StringBuilder();

            using (StringWriter writer = new StringWriter(builder))
            {
                formatter.Serialize(writer, new Pair(ViewState, ControlState));
            }

			string id = _session.ToString() + "|" + Page.Request.Url.ToString();

			Page.Session[id] = builder.ToString();
		}
	}
}
