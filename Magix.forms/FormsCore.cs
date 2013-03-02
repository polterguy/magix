/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Controller that makes it possible to create new 
	 * dynamically created forms
	 */
	public class FormsCore : ActiveController
	{
		/**
		 * Will create, and instantiate, a newly created dynamic form
		 */
		[ActiveEvent(Name = "magix.forms.create-form")]
		public void magix_forms_create_form(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.forms.create-form"].Value = "content";
				e.Params["container"].Value = "content";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["controls"]["Button"].Value = "btn";
				e.Params["controls"]["Button"]["Text"].Value = "Hello World!";
				e.Params["inspect"].Value = @"Creates a dynamic form
and loading it into the ""container"" viewport container. ""form-id""
must be a uniquely identifiable id for later use. ""controls""
contains the controls themselves, such as ""Button"" nodes, etc.";
				return;
			}

			LoadModule (
				"Magix.forms.DynamicForm", 
				e.Params["container"].Get<string>(), 
				e.Params);
		}

		/**
		 * Will create, and instantiate, a newly created dynamic web page
		 */
		[ActiveEvent(Name = "magix.forms.create-web-page")]
		public void magix_forms_create_web_page(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["event:magix.forms.create-web-page"].Value = null;
				e.Params["container"].Value = "content";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["html"].Value = "Some HTML";
				e.Params["inspect"].Value = @"Creates a dynamic HTML web page,
loading it into the ""container"" viewport container. ""form-id""
must be a uniquely identifiable id for later use. ""html""
contains the html themselves. You can intermix controls into 
your HTML by creatning a control collection by typing them 
inside of {{...controls here...}}";
				return;
			}

			LoadModule(
				"Magix.forms.HtmlViewer", 
				e.Params["container"].Get<string>(), 
				e.Params);
		}
	}
}

