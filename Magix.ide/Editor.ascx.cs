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
    public class Editor : ActiveModule
    {
		protected TextArea surface;

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (FirstLoad)
			{
				AjaxManager.Instance.IncludeScriptFromFile("media/xing-wysihtml5/parser_rules/advanced.js");
				AjaxManager.Instance.IncludeScriptFromFile("media/xing-wysihtml5/dist/wysihtml5-0.4.0pre.min.js");
				AjaxManager.Instance.WriterAtBack.InnerWriter.Write(
					"new wysihtml5.Editor('" + 
					surface.ClientID + 
					@"', {
toolbar:'wysihtml5-editor-toolbar',
stylesheets: ['css/editor.css'],
useLineBreaks: false,
parserRules: wysihtml5ParserRules});");
			}
		}
	}
}
