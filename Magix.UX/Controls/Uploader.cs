/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using Magix.UX;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;


namespace Magix.UX.Widgets
{
	/*
     * uploader control
     */
	public class Uploader : BaseWebControl
	{
		private string _fileCache = null;

		/*
         * event firec when file is uploaded
         */
		public event EventHandler Uploaded;

		public Uploader()
		{
			Class = "mux-file-uploader";
		}

        /*
         * folder for saving files in
         */
        public string Folder
        {
            get { return ViewState["Folder"] as string; }
            set { ViewState["Folder"] = value; }
        }

		/*
         * returns file as base64 encoded
         */
		public string GetFileRawBASE64()
		{
			if (string.IsNullOrEmpty(Page.Request["__FILE"]))
				throw new ArgumentException("No files in Uploader");

			if (_fileCache == null)
			{
				_fileCache = Page.Request["__FILE"];
				_fileCache = _fileCache.Substring(_fileCache.IndexOf(",") + 1);
			}

			return _fileCache;
		}

		/*
         * returns client side filename
         */
		public string GetFileName()
		{
			if (string.IsNullOrEmpty(Page.Request["__FILE"]))
				throw new ArgumentException("No files in Uploader");

			return Page.Request["__FILENAME"];
		}

		/*
         * number of files currently being uploaded
         */
		public int SizeOfBatch
		{
			get { return int.Parse(Page.Request.Params["__MUX_TOTAL"]); }
		}

		/*
         * current file being uploaded of total batch
         */
		public int CurrentNo
		{
			get { return int.Parse(Page.Request.Params["__MUX_CURRENT"]); }
		}

		protected override void OnPreRender(EventArgs e)
		{
			Manager.Instance.IncludeResourceScript(
				typeof(Timer),
				"Magix.UX.Js.Uploader.js");
			base.OnPreRender(e);
		}

		public override void RaiseEvent(string name)
		{
			switch (name)
			{
				case "uploaded":
				if (Uploaded != null)
					Uploaded(this, new EventArgs());
				break;
			}
		}

		protected override string GetClientSideScriptType()
		{
			return "new MUX.Uploader";
		}

		protected override void AddAttributes(Element el)
		{
			base.AddAttributes(el);
		}

		protected override void RenderMuxControl(HtmlBuilder builder)
		{
			using (Element el = builder.CreateElement("div"))
			{
				AddAttributes(el);

				RenderChildren(builder.Writer);

				using (Element ul = builder.CreateElement("ul"))
				{
					ul.AddAttribute("id", this.ClientID + "_ul");
				}
			}
		}
	}
}
