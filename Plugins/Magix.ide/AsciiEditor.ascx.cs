﻿/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
	/**
	 * edits ascii files
	 */
    public class AsciiEditor : ActiveModule
    {
		protected TextArea surface;
        protected Button save;
        protected Button preview;
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
                if (path.Text.LastIndexOf(".mml") == path.Text.Length - 4)
                    preview.Visible = true;
			};
		}

        protected void save_Click(object sender, EventArgs e)
        {
            SaveFileContent();
        }

        protected void preview_Click(object sender, EventArgs e)
        {
            SaveFileContent();

            Node tmp = new Node();
            tmp["file"].Value = path.Text;
            tmp["container"].Value = "content3";
            tmp["css"].Value = "span-24 last top-1";

            RaiseActiveEvent(
                "magix.forms.load-mml-web-part",
                tmp);
        }

        private void SaveFileContent()
        {
            Node tmp = new Node();
            tmp["value"].Value = surface.Text;
            tmp.Value = path.Text;

            RaiseActiveEvent(
                "magix.file.save",
                tmp);

            tmp = new Node();
            tmp["message"].Value = "file saved";
            tmp["time"].Value = 500;

            RaiseActiveEvent(
                "magix.viewport.show-message",
                tmp);

            tmp = new Node();
            tmp["path"].Value = path.Text;

            RaiseActiveEvent(
                "magix.ide.file-saved",
                tmp);
        }

        protected void delete_Click(object sender, EventArgs e)
        {
            Node c = new Node();
            c["message"].Value = "are you sure you wish to delete the file " + path.Text + "?";
            c["code"]["magix.file.delete"].Value = path.Text;
            c["code"]["magix.ide.file-deleted"]["path"].Value = path.Text;

            RaiseActiveEvent(
                "magix.viewport.confirm",
                c);
        }

        protected void close_Click(object sender, EventArgs e)
        {
            Node c = new Node();
            c["container"].Value = this.Parent.ID;

            RaiseActiveEvent(
                "magix.viewport.clear-controls",
                c);
        }
    }
}

