/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
	/**
     * Widget encapsulating an HTML5 drag and drop File Uploader control,
     * with progree bar and support for multiple files
     */
	public class Uploader : BaseWebControl
	{
		private string _fileCache = null;

		/**
         * Fired when File is uploaded. Access the file in raw [BASE64 encoded] through
         * the GetFileRawBASE64
         */
		public event EventHandler Uploaded;

		public Uploader()
		{
			CssClass = "mux-file-uploader";
		}

		/**
         * Returns the raw BASE64 encoded information about the file. Use Convert.FromBase64...
         * to convert file to raw byte[] content ...
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

		/**
         * Returns the file's client side filename
         */
		public string GetFileName()
		{
			if (string.IsNullOrEmpty(Page.Request["__FILE"]))
				throw new ArgumentException("No files in Uploader");

			return Page.Request["__FILENAME"];
		}

		/**
         * Since the user can drag and drop multiple files into the browser at
         * the same time, we need to track which is our 'current file' to be able to notify
         * our code that a 'batch is finished'. This one returns the size [number of files]
         * in the 'current processed batch'. Meaning, as long as SizeOfBatch is higher than
         * CurrentNo, then you've got more files coming in after this one ...
         */
		public int SizeOfBatch
		{
			get { return int.Parse(Page.Request.Params["__MUX_TOTAL"]); }
		}

		/**
         * Since the user can drag and drop multiple files into the browser at
         * the same time, we need to track which is our 'current file' to be able to notify
         * our code that a 'batch is finished'. This one returns the current index
         * in the 'current processed batch'. Meaning, as long as SizeOfBatch is higher than
         * CurrentNo, then you've got more files coming in after this one ...
         */
		public int CurrentNo
		{
			get { return int.Parse(Page.Request.Params["__MUX_CURRENT"]); }
		}

		protected override void OnPreRender(EventArgs e)
		{
			AjaxManager.Instance.IncludeScriptFromResource(
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
