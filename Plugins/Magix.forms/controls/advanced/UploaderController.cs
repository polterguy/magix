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

			Uploader ctrl = new Uploader();

            Node node = ip["_code"].Get<Node>();

            if (node.ContainsValue("class"))
				ctrl.Class = node["class"].Get<string>();

			string folder = node["directory"].Get<string>("tmp");
            ctrl.Folder = folder;

            string idPrefix = "";
            if (ip.ContainsValue("id-prefix"))
                idPrefix = ip["id-prefix"].Get<string>();

            if (node.ContainsValue("id"))
                ctrl.ID = idPrefix + node["id"].Get<string>();
            else if (node.Value != null)
                ctrl.ID = idPrefix + node.Get<string>();

            if (ShouldHandleEvent("onuploaded", node))
			{
				Node codeNode = node["onuploaded"].Clone();
				ctrl.Uploaded += delegate(object sender2, EventArgs e2)
				{
                    string directory = (sender2 as Uploader).Folder;
                    SaveFile(directory, sender2);
                    FillOutEventInputParameters(codeNode, sender2);
                    RaiseActiveEvent(
						"magix.execute",
						codeNode);
				};
			}
			else
			{
				ctrl.Uploaded += delegate(object sender2, EventArgs e2)
				{
                    SaveFile(folder, sender2);
                };
			}
            ip["_ctrl"].Value = ctrl;
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

        /*
         * sets the directory of where to upload files to
         */
        [ActiveEvent(Name = "magix.forms.uploader-set-directory")]
        private void magix_forms_uploader_set_directory(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.uploader-set-directory-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.uploader-set-directory-sample]");
                return;
            }

            if (!ip.ContainsValue("value"))
                throw new ArgumentException("no [value] given to [magix.forms.uploader-set-directory]");

            Uploader ctrl = FindControl<Uploader>(ip);
            ctrl.Folder = ip["value"].Get<string>();
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

