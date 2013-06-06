/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX.Builder;
using Magix.UX.Widgets.Core;

namespace Magix.UX.Widgets
{
    /**
     * wysiwyg editor type of control
     */
	public class Wysiwyg : BaseWebControlFormElementInputText
    {
		/**
		 * text property of wysiwyg editor
		 */
		public override string Text
		{
			get
			{
				return ViewState["Text"] == null ? "" : (string)ViewState["Text"];
			}
			set
			{
				if (value != Text && HasRendered)
				{
					AjaxManager.Instance.WriterAtBack.Write(
						"window.actualEditor.setValue(\"" + 
						(value.Replace("\n", "\\n").Replace("\r\n", "\\n").Replace("\"", "\\\"")) + "\");");
				}
				ViewState["Text"] = value;
			}
		}

		protected override void SetValue()
		{
			string valueOfTextBox = Page.Request.Params[ClientID + "_text"];
			if (valueOfTextBox != Text)
			{
				ViewState["Text"] = valueOfTextBox;
			}
		}

		/**
         * editor css file
         */
		public string EditorCssFile
		{
			get { return ViewState["EditorCssFile"] == null ? "media/grid/main.css" : (string)ViewState["EditorCssFile"]; }
			set { ViewState["EditorCssFile"] = value; }
		}

		/**
         * place holder
         */
		public string PlaceHolder
		{
			get { return ViewState["PlaceHolder"] == null ? null : (string)ViewState["PlaceHolder"]; }
			set { ViewState["PlaceHolder"] = value; }
		}

		/**
         * has bold
         */
		public bool HasBold
		{
			get { return ViewState["HasBold"] == null ? true : (bool)ViewState["HasBold"]; }
			set { ViewState["HasBold"] = value; }
		}

		/**
         * has italic
         */
		public bool HasItalic
		{
			get { return ViewState["HasItalic"] == null ? true : (bool)ViewState["HasItalic"]; }
			set { ViewState["HasItalic"] = value; }
		}

		/**
         * has unordered list
         */
		public bool HasUnorderedList
		{
			get { return ViewState["HasUnorderedList"] == null ? true : (bool)ViewState["HasUnorderedList"]; }
			set { ViewState["HasUnorderedList"] = value; }
		}

		/**
         * has ordered list
         */
		public bool HasOrderedList
		{
			get { return ViewState["HasOrderedList"] == null ? true : (bool)ViewState["HasOrderedList"]; }
			set { ViewState["HasOrderedList"] = value; }
		}

		/**
         * has create link
         */
		public bool HasCreateLink
		{
			get { return ViewState["HasCreateLink"] == null ? true : (bool)ViewState["HasCreateLink"]; }
			set { ViewState["HasCreateLink"] = value; }
		}

		/**
         * has insert image
         */
		public bool HasInsertImage
		{
			get { return ViewState["HasInsertImage"] == null ? true : (bool)ViewState["HasInsertImage"]; }
			set { ViewState["HasInsertImage"] = value; }
		}

		/**
         * has h1
         */
		public bool HasH1
		{
			get { return ViewState["HasH1"] == null ? true : (bool)ViewState["HasH1"]; }
			set { ViewState["HasH1"] = value; }
		}

		/**
         * has h2
         */
		public bool HasH2
		{
			get { return ViewState["HasH2"] == null ? true : (bool)ViewState["HasH2"]; }
			set { ViewState["HasH2"] = value; }
		}

		/**
         * has h3
         */
		public bool HasH3
		{
			get { return ViewState["HasH3"] == null ? false : (bool)ViewState["HasH3"]; }
			set { ViewState["HasH3"] = value; }
		}

		/**
         * has h4
         */
		public bool HasH4
		{
			get { return ViewState["HasH4"] == null ? false : (bool)ViewState["HasH4"]; }
			set { ViewState["HasH4"] = value; }
		}

		/**
         * has h5
         */
		public bool HasH5
		{
			get { return ViewState["HasH5"] == null ? false : (bool)ViewState["HasH5"]; }
			set { ViewState["HasH5"] = value; }
		}

		/**
         * has h6
         */
		public bool HasH6
		{
			get { return ViewState["HasH6"] == null ? false : (bool)ViewState["HasH6"]; }
			set { ViewState["HasH6"] = value; }
		}
		
		/**
         * has fore color
         */
		public bool HasForeColor
		{
			get { return ViewState["HasForeColor"] == null ? true : (bool)ViewState["HasForeColor"]; }
			set { ViewState["HasForeColor"] = value; }
		}

		/**
         * has insert speech
         */
		public bool HasInsertSpeech
		{
			get { return ViewState["HasInsertSpeech"] == null ? true : (bool)ViewState["HasInsertSpeech"]; }
			set { ViewState["HasInsertSpeech"] = value; }
		}

		/**
         * has change view
         */
		public bool HasChangeView
		{
			get { return ViewState["HasChangeView"] == null ? true : (bool)ViewState["HasChangeView"]; }
			set { ViewState["HasChangeView"] = value; }
		}

		protected override void OnPreRender(EventArgs e)
		{
			AjaxManager.Instance.IncludeScriptFromResource(
				typeof(Wysiwyg),
				"Magix.UX.Js.advanced.js");
			AjaxManager.Instance.IncludeScriptFromResource(
				typeof(Wysiwyg),
				"Magix.UX.Js.wysihtml5-0.4.0pre.min.js");
			base.OnPreRender(e);
		}

		protected override string GetClientSideScript()
		{
			return base.GetClientSideScript() + 
				"window.actualEditor = new wysihtml5.Editor('" + 
				ClientID + "_text" + 
				@"', {
toolbar:'" + ClientID + @"_editortoolbar',
stylesheets: ['" + EditorCssFile + @"'],
useLineBreaks: false,
parserRules: wysihtml5ParserRules});";
		}

		protected override void RenderMuxControl(Magix.UX.Builder.HtmlBuilder builder)
		{
			using (Element el = builder.CreateElement("div"))
			{
				AddAttributes(el);

				using (Element bar = builder.CreateElement("div"))
				{
					bar.AddAttribute("id", ClientID + "_editortoolbar");
					bar.AddAttribute("class", "wysiwyg-editor-toolbar");
					bar.AddAttribute("style", "display:none;");

					using (Element header = builder.CreateElement("header"))
					{
						using (Element ul = builder.CreateElement("ul"))
						{
							ul.AddAttribute("class", "commands");
							if (HasBold)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "bold");
									li.AddAttribute("title", "make text bold (ctrl + b)");
									li.AddAttribute("class", "command");
								}
							}
							if (HasItalic)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "italic");
									li.AddAttribute("title", "make text italic (ctrl + i)");
									li.AddAttribute("class", "command");
								}
							}
							if (HasUnorderedList)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "insertUnorderedList");
									li.AddAttribute("title", "insert an unordered list");
									li.AddAttribute("class", "command");
								}
							}
							if (HasOrderedList)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "insertOrderedList");
									li.AddAttribute("title", "insert an ordered list");
									li.AddAttribute("class", "command");
								}
							}
							if (HasCreateLink)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "createLink");
									li.AddAttribute("title", "insert a link");
									li.AddAttribute("class", "command");
								}
							}
							if (HasInsertImage)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "insertImage");
									li.AddAttribute("title", "insert an image");
									li.AddAttribute("class", "command");
								}
							}
							if (HasH1)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "formatBlock");
									li.AddAttribute("data-wysihtml5-command-value", "h1");
									li.AddAttribute("title", "insert headline 1");
									li.AddAttribute("class", "command");
								}
							}
							if (HasH2)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "formatBlock");
									li.AddAttribute("data-wysihtml5-command-value", "h2");
									li.AddAttribute("title", "insert headline 2");
									li.AddAttribute("class", "command");
								}
							}
							if (HasH3)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "formatBlock");
									li.AddAttribute("data-wysihtml5-command-value", "h3");
									li.AddAttribute("title", "insert headline 3");
									li.AddAttribute("class", "command");
								}
							}
							if (HasH4)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "formatBlock");
									li.AddAttribute("data-wysihtml5-command-value", "h4");
									li.AddAttribute("title", "insert headline 4");
									li.AddAttribute("class", "command");
								}
							}
							if (HasH5)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "formatBlock");
									li.AddAttribute("data-wysihtml5-command-value", "h5");
									li.AddAttribute("title", "insert headline 5");
									li.AddAttribute("class", "command");
								}
							}
							if (HasH6)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "formatBlock");
									li.AddAttribute("data-wysihtml5-command-value", "h6");
									li.AddAttribute("title", "insert headline 6");
									li.AddAttribute("class", "command");
								}
							}
							if (HasForeColor)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command-group", "foreColor");
									li.AddAttribute("title", "color the selected text");
									li.AddAttribute("class", "fore-color");
									using (Element ul2 = builder.CreateElement("ul"))
									{
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "silver");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "gray");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "maroon");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "red");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "purple");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "green");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "olive");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "navy");
										}
										using (Element li2 = builder.CreateElement("li"))
										{
											li2.AddAttribute("data-wysihtml5-command", "foreColor");
											li2.AddAttribute("data-wysihtml5-command-value", "blue");
										}
									}
								}
							}
							if (HasInsertSpeech)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-command", "insertSpeech");
									li.AddAttribute("title", "insert speech");
									li.AddAttribute("class", "command");
								}
							}
							if (HasChangeView)
							{
								using (Element li = builder.CreateElement("li"))
								{
									li.AddAttribute("data-wysihtml5-action", "change_view");
									li.AddAttribute("title", "show html");
									li.AddAttribute("class", "action");
								}
							}
						}
					}
					if (HasCreateLink)
					{
						using (Element cr = builder.CreateElement("div"))
						{
							cr.AddAttribute("data-wysihtml5-dialog", "createLink");
							cr.AddAttribute("style", "display:none;");
							using (Element lbl = builder.CreateElement("label"))
							{
								lbl.Write("link:");
								using (Element inp = builder.CreateElement("input"))
								{
									inp.AddAttribute("data-wysihtml5-dialog-field", "href");
									inp.AddAttribute("value", "http://");
								}
							}
							using (Element anc = builder.CreateElement("a"))
							{
								anc.AddAttribute("data-wysihtml5-dialog-action", "save");
								anc.Write("ok");
							}
							cr.Write("&nbsp;");
							using (Element anc = builder.CreateElement("a"))
							{
								anc.AddAttribute("data-wysihtml5-dialog-action", "cancel");
								anc.Write("cancel");
							}
						}
					}
					if (HasInsertImage)
					{
						using (Element cr = builder.CreateElement("div"))
						{
							cr.AddAttribute("data-wysihtml5-dialog", "insertImage");
							cr.AddAttribute("style", "display:none;");
							using (Element lbl = builder.CreateElement("label"))
							{
								lbl.Write("image:");
								using (Element inp = builder.CreateElement("input"))
								{
									inp.AddAttribute("data-wysihtml5-dialog-field", "src");
									inp.AddAttribute("value", "http://");
								}
							}
							using (Element anc = builder.CreateElement("a"))
							{
								anc.AddAttribute("data-wysihtml5-dialog-action", "save");
								anc.Write("ok");
							}
							cr.Write("&nbsp;");
							using (Element anc = builder.CreateElement("a"))
							{
								anc.AddAttribute("data-wysihtml5-dialog-action", "cancel");
								anc.Write("cancel");
							}
						}
					}
				}

				using (Element txt = builder.CreateElement("textarea"))
				{
					txt.AddAttribute("id", ClientID + "_text");
					txt.AddAttribute("name", ClientID + "_text");
					txt.AddAttribute("class", "wysiwyg-editor-textarea");
					if (PlaceHolder != null)
						txt.AddAttribute("placeholder", PlaceHolder);
					txt.Write(Text);
				}
				RenderChildren(builder.Writer as System.Web.UI.HtmlTextWriter);
			}
		}
    }
}
