/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
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

			string folder = "";

			if (node.Contains("folder") && !string.IsNullOrEmpty(node["folder"].Get<string>()))
				folder = node["folder"].Get<string>();

			if (node.Contains("onuploaded"))
			{
				// TODO: is this right? do we need to clone?
				Node codeNode = node["onuploaded"].Clone();

				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
					Uploader that = sender2 as Uploader;

					string fileName = Page.Server.MapPath(folder.Trim('/') + "/" + that.GetFileName());

					if (File.Exists(fileName))
						File.Delete(fileName);

					byte[] content = Convert.FromBase64String(that.GetFileRawBASE64());
					using(FileStream stream = File.Create(fileName))
					{
						stream.Write(content, 0, content.Length);

					}

					RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
			else
			{
				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
					Uploader that = sender2 as Uploader;

					string fileName = Page.Server.MapPath(folder.Trim('/') + "/" + that.GetFileName());

					if (File.Exists(fileName))
						File.Delete(fileName);

					byte[] content = Convert.FromBase64String(that.GetFileRawBASE64());
					using(FileStream stream = File.Create(fileName))
					{
						stream.Write(content, 0, content.Length);
					}
				};
			}
			e.Params["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
			node["event:magix.forms.create-form"].Value = null;
			node["inspect"].Value = @"creates a uploader input type of web control.&nbsp;&nbsp;
[onuploaded] is the event handler";
			node["container"].Value = "content5";
			node["form-id"].Value = "sample-form";
			node["controls"]["uploader"]["folder"].Value = "system42";
			base.Inspect(node["controls"]["uploader"]);
			node["controls"]["uploader"]["onuploaded"].Value = "hyper lisp code";
		}
	}
}

