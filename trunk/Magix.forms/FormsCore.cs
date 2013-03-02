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
				e.Params["inspect"].Value = @"creates a dynamic form
and loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;[controls]
contains the controls themselves";
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
				e.Params["form-id"].Value = "unique-id-of-form";
				e.Params["html"].Value = "some html";
				e.Params["inspect"].Value = @"creates a dynamic html web page,
loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;[html]
contains the html.&nbsp;&nbsp;you can intermix controls into 
your html by creating a control collection by typing them 
inside of brackets such as {{...controls here...}}";
				return;
			}

			LoadModule(
				"Magix.forms.HtmlViewer", 
				e.Params["container"].Get<string>(), 
				e.Params);
		}
	}
}

