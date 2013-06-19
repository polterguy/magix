/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 */
	public class UploaderCore : BaseWebControlCore
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.controls.uploader")]
		public void magix_forms_controls_button(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				Inspect(e.Params);
				return;
			}

			Node node = e.Params["_code"].Value as Node;

			Uploader ret = new Uploader();

			FillOutParameters(node, ret);

			if (node.Contains("onuploaded"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onuploaded"].Clone();

				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a uploader input type of web control.&nbsp;&nbsp;
[onuploaded] is the event handler";
			node["container"].Value = "modal";
			node["form-id"].Value = "sample-form";
			node["controls"]["uploader"]["onuploaded"].Value = "hyper lisp code";
			base.Inspect(node["controls"]["uploader"]);
		}
	}
}

