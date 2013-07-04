/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using Magix.Core;

namespace Magix.admin
{
	/*
	 * ide controller
	 */
	public class IdeCore : ActiveController
	{
		/*
		 * edit ascii files
		 */
		[ActiveEvent(Name = "magix.admin.edit-file")]
		public void magix_admin_edit_file(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.admin.edit-file"].Value = null;
				e.Params["inspect"].Value = @"attempts to open the given [file]
in whatever editor makes sense according to its extension, or download directly in browser 
if none.&nbsp;&nbsp;if file doesn't exist, an empty file 
will be edited, but not created before saved.&nbsp;&nbsp;not thread safe";
				e.Params["file"].Value = "media/grid/main.css";
				e.Params["css"].Value = "css classes of editor";
				e.Params["container"].Value = "content5";
				return;
			}

			if (!e.Params.Contains("file"))
				throw new ArgumentException("need [file] parameter");

			string file = e.Params["file"].Get<string>();

			string extension = file.Substring(file.LastIndexOf('.') + 1);

			switch (extension)
			{
			case "txt":
			case "cs":
			case "hl":
			case "mml":
			case "csproj":
			case "config":
			case "html":
			case "htm":
			case "css":
			case "aspx":
			case "ascx":
			case "asax":
			case "js":
			{
				if (!e.Params.Contains("container"))
					throw new ArgumentException("edit-file needs [container]");

				Node tmp = new Node();
				tmp["file"].Value = file;

				if (File.Exists(Page.Server.MapPath(file)))
				{
					using (TextReader reader = File.OpenText(file))
					{
						tmp["content"].Value = reader.ReadToEnd();
					}
				}
				else
					tmp["content"].Value = "";

				if (e.Params.Contains("css"))
					tmp["css"].Value = e.Params["css"].Value;

				LoadModule(
					"Magix.ide.AsciiEditor", 
					e.Params["container"].Get<string>(), 
					tmp);
			} break;
			default:
			{
				Node tmp = new Node();

				tmp["script"].Value = "window.open('" + file + "', '_blank').focus();";

				RaiseActiveEvent(
					"magix.viewport.execute-javascript",
					tmp);
			} break;
			}
		}
	}
}
