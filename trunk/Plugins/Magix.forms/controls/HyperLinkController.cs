/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/*
	 * hyper link
	 */
    public class HyperLinkController : BaseWebControlFormElementController
	{
		/*
		 * creates a hyper link
		 */
		[ActiveEvent(Name = "magix.forms.controls.hyperlink")]
		public void magix_forms_controls_hyperlink(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			HyperLink ret = new HyperLink();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();
            if (node.Contains("value") && node["value"].Value != null)
				ret.Value = node["value"].Get<string>();

			if (node.Contains("href") && node["href"].Value != null)
			{
				string url = node["href"].Get<string>();

				if (url.StartsWith("~"))
					url = url.Replace("~", GetApplicationBaseUrl());

				ret.Href = url;
			}

			if (node.Contains("target") && node["target"].Value != null)
				ret.Target = node["target"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			AppendInspect(node["inspect"], @"creates a hyperlink input type of web control

hyperlinks points to other urls, either locally, or externally.  you can choose if the 
hyperlink should open up in the same window, or another browser window");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["hyperlink"]);
            node["magix.forms.create-web-part"]["controls"]["hyperlink"]["value"].Value = "anchor text of hyperlink";
            node["magix.forms.create-web-part"]["controls"]["hyperlink"]["href"].Value = "http://google.com";
            node["magix.forms.create-web-part"]["controls"]["hyperlink"]["target"].Value = "_blank";
            node["magix.forms.create-web-part"]["controls"]["hyperlink"]["key"].Value = "C";
            AppendInspect(node["inspect"], @"[value] is the readable text, displayed to 
the end user of the web control.  this is the property which is changed or retrieved when 
you invoke the [magix.forms.set-value] and the [magix.forms.get-value] for your control

[href] sets the url of the document to link to.  this can be any url, locally or externally

[target] can _blank, _new, or any id you wish to use, and informs the browser of what browser 
window to open up the url within once clicked", true);
		}
	}
}

