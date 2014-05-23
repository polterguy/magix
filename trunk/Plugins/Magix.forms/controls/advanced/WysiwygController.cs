/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/*
	 * wysiwyg control
	 */
    internal sealed class WysiwygController : BaseWebControlFormElementInputTextController
	{
		/*
		 * creates wysiwyg control
		 */
		[ActiveEvent(Name = "magix.forms.controls.wysiwyg")]
		private void magix_forms_controls_wysiwyg(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

            Wysiwyg ret = new Wysiwyg();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("value"))
				ret.Value = node["value"].Get<string>();

            if (node.ContainsValue("has-bold"))
				ret.HasBold = node["has-bold"].Get<bool>();

            if (node.ContainsValue("has-italic"))
				ret.HasItalic = node["has-italic"].Get<bool>();

            if (node.ContainsValue("has-unorderedlist"))
				ret.HasUnorderedList = node["has-unorderedlist"].Get<bool>();

            if (node.ContainsValue("has-orderedlist"))
				ret.HasOrderedList = node["has-orderedlist"].Get<bool>();

            if (node.ContainsValue("has-createlink"))
				ret.HasCreateLink = node["has-createlink"].Get<bool>();

            if (node.ContainsValue("has-insertimage"))
				ret.HasInsertImage = node["has-insertimage"].Get<bool>();

            if (node.ContainsValue("has-h1"))
				ret.HasH1 = node["has-h1"].Get<bool>();

            if (node.ContainsValue("has-h2"))
				ret.HasH2 = node["has-h2"].Get<bool>();

            if (node.ContainsValue("has-h3"))
				ret.HasH3 = node["has-h3"].Get<bool>();

            if (node.ContainsValue("has-h4"))
				ret.HasH4 = node["has-h4"].Get<bool>();

            if (node.ContainsValue("has-h5"))
				ret.HasH5 = node["has-h5"].Get<bool>();

            if (node.ContainsValue("has-h6"))
				ret.HasH6 = node["has-h6"].Get<bool>();

            if (node.ContainsValue("has-forecolor"))
				ret.HasForeColor = node["has-forecolor"].Get<bool>();

            if (node.ContainsValue("has-insertspeech"))
				ret.HasInsertSpeech = node["has-insertspeech"].Get<bool>();

            if (node.ContainsValue("has-showhtml"))
				ret.HasChangeView = node["has-showhtml"].Get<bool>();

            if (node.ContainsValue("placeholder"))
				ret.PlaceHolder = node["placeholder"].Get<string>();

            if (node.ContainsValue("editor-css-file"))
				ret.EditorCssFile = node["editor-css-file"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.wysiwyg-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.wysiwyg-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["wysiwyg"]);
            node["magix.forms.create-web-part"]["controls"]["wysiwyg"]["style"].Value = "display:block;";
            node["magix.forms.create-web-part"]["controls"]["wysiwyg"]["class"].Value = "wysiwyg span-22 last";
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.wysiwyg-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["wysiwyg"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.wysiwyg-sample-end]");
		}
	}
}

