/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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
	/**
	 * edits ascii files
	 */
    public class AsciiEditor : ActiveModule
    {
		protected TextArea surface;
		protected Button save;
		protected TextBox path;

		public override void InitialLoading(Node node)
		{
			base.InitialLoading(node);

			Load +=
				delegate
			{
				surface.Text = node["content"].Get<string>();
				path.Text = node["file"].Get<string>("empty.txt");
				path.Select();
				path.Focus();
			};
		}
		
		protected void save_Click(object sender, EventArgs e)
		{
			Node tmp = new Node();
			tmp["value"].Value = surface.Text;
			tmp.Value = path.Text;

			RaiseEvent(
				"magix.file.save",
				tmp);

			tmp = new Node();
			tmp["message"].Value = "file saved";
			tmp["time"].Value = 500;

			RaiseEvent(
				"magix.viewport.show-message",
				tmp);
			
			tmp = new Node();
			tmp["path"].Value = path.Text;

			RaiseEvent(
				"magix.ide.file-saved",
				tmp);
		}
		
		protected void delete_Click(object sender, EventArgs e)
		{
			Node c = new Node();
			c["message"].Value = "are you sure you wish to delete the file " + path.Text + "?";
			c["code"]["magix.file.save"].Value = path.Text;
			c["code"]["magix.ide.file-deleted"]["path"].Value = path.Text;

			RaiseEvent(
				"magix.viewport.confirm",
				c);
		}
	}
}

