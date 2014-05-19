/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Collections.Generic;
using Magix.Core;

namespace Magix.help
{
	/**
	 * implementation of help logic
	 */
	public class ControllerHelp : ActiveController
	{
		private string Next
		{
			get
			{
				return Page.Session["ControllerHelper.Next"] as string;
			}
			set
			{
				Page.Session["ControllerHelper.Next"] = value;
			}
		}

		private int CurrentIndex
		{
			get
			{
				if (Page.Session["ControllerHelper.CurrentIndex"] == null)
					Page.Session["ControllerHelper.CurrentIndex"] = -1;
				return (int)Page.Session["ControllerHelper.CurrentIndex"];
			}
			set { Page.Session["ControllerHelper.CurrentIndex"] = value; }
		}

		/**
		 * starts help
		 */
		[ActiveEvent(Name = "magix.help.start-help")]
		public void magix_help_start_help(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["inspect"].Value = @"<p>opens the help files in the help 
container</p><p>not thread safe</p>";
				return;
			}

            // Including JavaScript files ...
            Node tmp = new Node();

            tmp["type"].Value = "javascript";
            tmp["file"].Value = "media/bootstrap/js/jQuery.js";

            RaiseActiveEvent(
                "magix.viewport.include-client-file",
                tmp);

            tmp = new Node();
            tmp["type"].Value = "javascript";
            tmp["file"].Value = "media/bootstrap/js/bootstrap.min.js";

            RaiseActiveEvent(
                "magix.viewport.include-client-file",
                tmp);
            
            tmp = new Node();

			tmp["form-id"].Value = "help-navigation";
			tmp["container"].Value = "help";
			tmp["class"].Value = "span-22 last";
			tmp["mml"].Value = string.Format(@"
<div class=""span-23 last bottom-1 btn-group top-2"">
{{{{
button=>back
  value=><<
  class=>btn-large span-2
  onclick
    magix.help.move-backwards
    magix.viewport.scroll=>help
button=>index
  value=>index
  class=>btn-large span-2
  onclick
    magix.help.open-file
      file=>system42/admin/help/index.mml
    magix.viewport.scroll=>help
button=>close
  value=>close
  class=>btn-large span-2
  onclick
    magix.viewport.clear-controls
      container=>help
text-box=>search
  placeholder=>search ...
  class=>span-13 input-large
  autocomplete=>false
  @data-provide=>typeahead
  @data-items=>12
  @data-source=>{0}
  onenterpressed
    set=>[magix.help.search-for-file][file-header].Value
      value=>[$][value].Value
    magix.help.search-for-file
    magix.forms.set-focus
      id=>next-page
    magix.forms.set-value
      id=>search
      value=>
button=>next-page
  value=>next page
  class=>btn-large span-3
  onclick
    magix.help.move-next
      force-page=>true
    magix.viewport.scroll=>help
button=>next
  value=>>>
  class=>btn-large span-2 last
  onclick
    magix.help.move-next
    magix.viewport.scroll=>help
}}}}
</div>
{{{{
dynamic=>help-content
  class=>span-22 last
  onfirstload
    magix.help.open-help-files
}}}}
<div class=""span-6 last right btn-group"">
{{{{
button=>back-2
  value=>back
  class=>btn-large span-3
  onclick
    magix.help.move-backwards
    magix.viewport.scroll=>help
button=>next-page-2
  value=>next page
  class=>btn-large span-3 last
  onclick
    magix.help.move-next
      force-page=>true
    magix.viewport.scroll=>help
}}}}
</div>
", GetAllHelpHeaders());
			RaiseActiveEvent(
				"magix.forms.create-mml-web-part",
				tmp);
        }

        [ActiveEvent(Name = "magix.help.open-help-files")]
        protected void magix_help_open_help_files(object sender, ActiveEventArgs e)
        {
			Node tmp = new Node();

			tmp["form-id"].Value = "help";
            tmp["container"].Value = "help-content";
			tmp["class"].Value = "span-22 last help-system";

			using (TextReader reader = File.OpenText(Page.Server.MapPath("system42/admin/help/index.mml")))
			{
				tmp["mml"].Value = reader.ReadToEnd();
			}

			RaiseActiveEvent(
				"magix.forms.create-mml-web-part",
				tmp);

			if (Pages.Count > 0)
			{
				// We have help history here, open last seen page instead
				tmp = new Node();

				tmp["freeze-help-stack"].Value = true;
				tmp["file"].Value = Pages[CurrentIndex];

				RaiseActiveEvent(
					"magix.help.open-file",
					tmp);
			}
			else
			{
				tmp = new Node();
				tmp["reset"].Value = true;
				tmp["page"].Value = "system42/admin/help/index.mml";

				RaiseActiveEvent(
					"magix.help.add-page",
					tmp);
			}
		}

        private static string _helpHeaders = "";
        private static Dictionary<string, string> _helpFiles = new Dictionary<string, string>();
        private string GetAllHelpHeaders()
        {
            if (string.IsNullOrEmpty(_helpHeaders))
            {
                lock (_helpHeaders)
                {
                    if (string.IsNullOrEmpty(_helpHeaders))
                    {
                        _helpHeaders += "[";
                        GetHelpFilesForDirectory(Page.MapPath("~/system42/admin/help"));
                        _helpHeaders = _helpHeaders.Trim(',');
                        _helpHeaders += "]";
                    }
                }
            }
            return _helpHeaders;
        }

        private void GetHelpFilesForDirectory(string folder)
        {
            foreach (string idx in Directory.GetFiles(folder, "*.mml"))
            {
                using (TextReader reader = File.OpenText(idx))
                {
                    string fileContent = reader.ReadToEnd();
                    if (!fileContent.Contains("<h2>"))
                        continue;
                    string[] h2 = fileContent.Split(new string[] {"</h2>"}, StringSplitOptions.RemoveEmptyEntries);
                    string helpFileHeader = ("x" + h2[0]).Split(new string[] { "<h2>" }, StringSplitOptions.RemoveEmptyEntries)[1];
                    _helpFiles[helpFileHeader] = idx.Substring(idx.IndexOf("system42"));
                    _helpHeaders += "\"" + helpFileHeader.Replace("\"", "\\\"") + "\",";
                }
            }
            foreach (string idxFolder in Directory.GetDirectories(folder))
            {
                GetHelpFilesForDirectory(idxFolder);
            }
        }

		/**
		 * sets next help page
		 */
        [ActiveEvent(Name = "magix.help.search-for-file")]
        public void magix_help_search_for_file(object sender, ActiveEventArgs e)
        {
            if (e.Params.Contains("inspect"))
            {
                e.Params["event:magix.help.search-for-file"].Value = null;
                e.Params["inspect"].Value = @"search for a help file with the given header";
                e.Params["file-header"].Value = "active events";
                return;
            }

            string fileHeader = e.Params["file-header"].Get<string>();

            if (!_helpFiles.ContainsKey(fileHeader))
            {
                Node tmp = new Node();
                tmp["message"].Value = "file not found!";
                RaiseActiveEvent(
                    "magix.viewport.show-message",
                    tmp);
            }
            else
            {
                string fileName = _helpFiles[fileHeader];

                Node tmp = new Node();
                tmp["file"].Value = fileName;
                RaiseActiveEvent(
                    "magix.help.open-file",
                    tmp);
            }
        }

		/**
		 * sets next help page
		 */
		[ActiveEvent(Name = "magix.help.set-next")]
		public void magix_help_set_next(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.help.set-next"].Value = null;
				e.Params["inspect"].Value = @"set the next page for the help system.
&nbsp;&nbsp;not thread safe";
				e.Params["next"].Value = "system42/admin/help/index.mml";
				return;
			}

            if (!Ip(e.Params).Contains("next") || Ip(e.Params)["next"].Get<string>("") == "")
				throw new ArgumentException("cannot set-next without a next value");

            Next = Ip(e.Params)["next"].Get<string>();
		}

		/**
		 * moves to next help page
		 */
		[ActiveEvent(Name = "magix.help.move-next")]
		public void magix_help_move_next(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains ("inspect"))
			{
				e.Params["event:magix.help.move-next"].Value = null;
				e.Params["inspect"].Value = @"moves to the next page for the help system.
&nbsp;&nbsp;not thread safe";
				return;
			}

			// Defaulting to "next"
			string next = Next;

			Node node = new Node();

            if (e.Params.Contains("force-page") && e.Params["force-page"].Get<bool>())
            {
                next = Next;
            }
			else if (CurrentIndex < Pages.Count - 1)
			{
				// Mowing "forward" since we've got "forward history"
				CurrentIndex += 1;
				next = Pages[CurrentIndex];
				node["freeze-help-stack"].Value = true;
			}

			if (string.IsNullOrEmpty(next))
				throw new ArgumentException("no next page in help system");

			Next = null;

			node["file"].Value = next;

			RaiseActiveEvent(
				"magix.help.open-file",
				node);
		}

		/**
		 * opens a specific page on disc, as if it was mml
		 */
		[ActiveEvent(Name = "magix.help.open-file")]
		public void magix_help_open_file(object sender, ActiveEventArgs e)
        {
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.open-file"].Value = null;
				e.Params["inspect"].Value = @"opens the given [file] file as html.
&nbsp;&nbsp;thread safe";
				e.Params["file"].Value = "system42/admin/help/index.mml";
				return;
			}

			Node tmp = new Node();
			tmp["form-id"].Value = "help";

            using (TextReader reader = File.OpenText(Page.Server.MapPath(Ip(e.Params)["file"].Get<string>())))
			{
				tmp["mml"].Value = reader.ReadToEnd();
			}

			RaiseActiveEvent(
				"magix.forms.change-mml",
				tmp);

            if (!(Ip(e.Params).Contains("freeze-help-stack") &&
                Ip(e.Params)["freeze-help-stack"].Get<bool>()))
			{
				tmp = new Node();
                tmp["page"].Value = Ip(e.Params)["file"].Get<string>();

				RaiseActiveEvent(
					"magix.help.add-page",
					tmp);
			}
		}

		private List<string> Pages
		{
			get
			{
				if (Page.Session["ControllerHelper.Pages"] == null)
					Page.Session["ControllerHelper.Pages"] = new List<string>();
				return Page.Session["ControllerHelper.Pages"] as List<string>;
			}
		}

		/**
		 * adds a page to the stack of forward/backward stored pages
		 */
		[ActiveEvent(Name = "magix.help.add-page")]
		public void magix_help_add_page(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.add-page"].Value = null;
				e.Params["page"].Value = "help-system/path-to-page.mml";
				e.Params["reset"].Value = false;
				e.Params["inspect"].Value = @"adds the given [page] to the history 
of the help system.&nbsp;&nbsp;set [reset] to true to reset entire history stack
of help system.&nbsp;&nbsp;not thread safe";
				return;
			}

            if (Ip(e.Params).Contains("reset") && Ip(e.Params)["reset"].Get<bool>())
			{
				CurrentIndex = -1;
				Pages.Clear();
			}

            if (!Ip(e.Params).Contains("page"))
				throw new ArgumentException("need to know which page to add");

			while (Pages.Count - 1 > CurrentIndex)
				Pages.RemoveAt(Pages.Count - 1);

			CurrentIndex += 1;
            Pages.Add(Ip(e.Params)["page"].Get<string>());
		}

		/**
		 * moves backkwards to previous help page
		 */
		[ActiveEvent(Name = "magix.help.move-backwards")]
		public void magix_help_move_backwards(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.move-backwards"].Value = null;
				e.Params["inspect"].Value = @"opens the previously opened help file.
&nbsp;&nbsp;not thread safe";
				return;
			}

			if (CurrentIndex <= 0)
			{
				Node ms = new Node();
				ms["message"].Value = "No more previous screens";

				RaiseActiveEvent(
					"magix.viewport.show-message",
					ms);

				return;
			}

			CurrentIndex -= 1;

			Node tmp = new Node();

			tmp["freeze-help-stack"].Value = true;
			tmp["file"].Value = Pages[CurrentIndex];

			RaiseActiveEvent(
				"magix.help.open-file",
				tmp);

            Ip(e.Params)["success"].Value = true;
		}

		/**
		 * moves forwards to next help page
		 */
		[ActiveEvent(Name = "magix.help.move-forwards")]
		public void magix_help_move_forwards(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.help.move-backwards"].Value = null;
				e.Params["inspect"].Value = @"moves forward in the history of 
opened help pages.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (CurrentIndex >= Pages.Count - 1)
			{
				Node ms = new Node();
				ms["message"].Value = "No more forward screens";

				RaiseActiveEvent(
					"magix.viewport.show-message",
					ms);
				return;
			}

			CurrentIndex += 1;

			Node tmp = new Node();

			tmp["freeze-help-stack"].Value = true;
			tmp["file"].Value = Pages[CurrentIndex];

			RaiseActiveEvent(
				"magix.help.open-file",
				tmp);
		}
	}
}

