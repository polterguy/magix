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

            if (node.ContainsValue("class"))
				ret.Class = node["class"].Get<string>();

			string folder = node["folder"].Get<string>("tmp");

			if (ShouldHandleEvent("onuploaded", node))
			{
				Node codeNode = node["onuploaded"].Clone();
				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
                    SaveFile(folder, sender2);
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
			else
			{
				ret.Uploaded += delegate(object sender2, EventArgs e2)
				{
                    SaveFile(folder, sender2);
                };
			}
            ip["_ctrl"].Value = ret;
		}

        /*
         * helper for above
         */
        private void SaveFile(string folder, object sender2)
        {
            Uploader that = sender2 as Uploader;

            string fileName = Page.Server.MapPath(folder.Trim('/') + "/" + that.GetFileName());
            if (File.Exists(fileName))
                File.Delete(fileName);

            byte[] content = Convert.FromBase64String(that.GetFileRawBASE64());
            using (FileStream stream = File.Create(fileName))
            {
                stream.Write(content, 0, content.Length);
            }
        }

		/*
		 * has more data
		 */
		[ActiveEvent(Name = "magix.forms.uploader-has-more-data")]
		private void magix_forms_uploader_has_more_data(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.uploader-has-more-data-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.uploader-has-more-data-sample]");
				return;
			}

            Uploader ctrl = FindControl<Uploader>(ip);
            ip["value"].Value = ctrl.SizeOfBatch > ctrl.CurrentNo + 1;
		}

		protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.uploader-dox-start].Value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.uploader-sample-start]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["uploader"]);
            node["magix.forms.create-web-part"]["controls"]["uploader"]["class"].Value = "mux-file-uploader";
            node["magix.forms.create-web-part"]["controls"]["uploader"]["visible"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["info"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["dir"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["tabindex"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["title"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["style"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onclick"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["ondblclick"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onmousedown"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onmouseup"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onmouseover"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onmouseout"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onkeypress"].UnTie(); // makes no sense
            node["magix.forms.create-web-part"]["controls"]["uploader"]["onesc"].UnTie(); // makes no sense
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.uploader-dox-end].Value",
                true);
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["uploader"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.uploader-sample-end]");
		}
	}
}

