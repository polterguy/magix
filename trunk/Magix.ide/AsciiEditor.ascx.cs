/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
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

namespace Magix.ide
{
    public class AsciiEditor : ActiveModule
    {
		protected TextArea surface;
		protected Button save;

		private string Path
		{
			get { return ViewState["Path"] as string; }
			set { ViewState["Path"] = value; }
		}

		public override void InitialLoading(Node node)
		{
			base.InitialLoading(node);

			Load +=
				delegate
			{
				surface.Text = node["content"].Get<string>();
				Path = node["file"].Get<string>();
			};
		}

		protected void save_Click(object sender, EventArgs e)
		{
			Node tmp = new Node();
			tmp["file"].Value = surface.Text;
			tmp["path"].Value = Path;

			RaiseEvent(
				"magix.file.save",
				tmp);

			tmp = new Node();
			tmp["message"].Value = "file saved";
			tmp["time"].Value = 500;

			RaiseEvent(
				"magix.viewport.show-message",
				tmp);
		}
	}
}























