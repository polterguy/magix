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
	 * uploader control
	 */
    internal sealed class UploaderController : BaseWebControlController
	{
		/*
		 * creates uploader control
		 */
		[ActiveEvent(Name = "magix.forms.controls.uploader")]
		private void magix_forms_controls_button(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Uploader ret = new Uploader();
            Node node = ip["_code"].Get<Node>();
            
            if (node.Contains("class") && !string.IsNullOrEmpty(node["class"].Get<string>()))
				ret.Class = node["class"].Get<string>();

			string folder = "";

			if (node.Contains("folder") && !string.IsNullOrEmpty(node["folder"].Get<string>()))
				folder = node["folder"].Get<string>();

			if (ShouldHandleEvent("onuploaded", node))
			{
				Node codeNode = node["onuploaded"].Clone();
				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
                    FillOutEventInputParameters(codeNode, sender2);
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
            ip["_ctrl"].Value = ret;
		}

		/*
		 * has more data
		 */
		[ActiveEvent(Name = "magix.forms.has-more-data")]
		private void magix_forms_has_more_data(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                ip["inspect"].Value = @"returns true in [value] if there is more data in the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
                ip["magix.forms.has-more-data"]["id"].Value = "control";
                ip["magix.forms.has-more-data"]["form-id"].Value = "webpages";
                ip["magix.forms.has-more-data"]["value"].Value = true;
				return;
			}

            Uploader ctrl = FindControl<Uploader>(ip);

            ip["value"].Value = ctrl.SizeOfBatch > ctrl.CurrentNo + 1;
		}

		protected override void Inspect(Node node)
		{
            AppendInspect(node["inspect"], @"creates an uploader type of web control

uploaders are useful for allowing the end user to drag and drop files onto the browser 
surface, such that he can upload files to the server");
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["uploader"]);
            node["magix.forms.create-web-part"]["controls"]["uploader"]["folder"].Value = "system42";
            node["magix.forms.create-web-part"]["controls"]["uploader"]["class"].Value = "mux-file-uploader";
            node["magix.forms.create-web-part"]["controls"]["uploader"]["visible"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["info"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onuploaded"].Value = "hyperlisp code";
            AppendInspect(node["inspect"], @"[folder] is the folder where the files 
will be uploaded.  the filename of the file from the client-side, will be the same 
when the file is uploaded to the server

[class] is the css class asssociated with your uploader, and what determines the 
looks of the 'wait for files to upload' parts of your uploader

[onuploaded] is the active event which will be fired once the uploader is finished 
uploading a file", true);
		}
	}
}

