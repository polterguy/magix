/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.forms
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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.forms.create-form"].Value = "content";
				e.Params["container"].Value = "content";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["controls"]["Button"].Value = "btn";
				e.Params["controls"]["Button"]["Text"].Value = "Hello World!";
				e.Params["inspect"].Value = @"creates a dynamic form
and loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;[controls]
contains the controls themselves.&nbsp;&nbsp;not thread safe";
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
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.forms.create-web-page"].Value = null;
				e.Params["container"].Value = "header";
				e.Params["form-id"].Value = "harvester";
				e.Params["html"].Value = "<p>some html, or file.&nbsp;&nbsp;mutually exclusive parameters</p>";
				e.Params["file"].Value = "sample-scripts/email-harvester.mml";
				e.Params["inspect"].Value = @"creates a dynamic html web page,
loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;[html]
contains the html, alternatively you can use [file].&nbsp;&nbsp;
you can intermix webcontrols into your html by creating a control 
collection by typing them inside of brackets such as 
{{...controls, using hyper lisp syntax and code in
event handlers goes here...}}.&nbsp;&nbsp;internally it 
calls LoadModule, hence all the parameters that goes
into your magix.viewport.load-module active event, 
can also be passed into this, such as [css] and 
so on.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (e.Params.Contains("file"))
			{
				if (e.Params.Contains("html"))
					throw new ArgumentException(
						@"marvin confused, don't know to use [file] or [html], choose one.&nbsp;&nbsp;
only one girl-friend at the time.&nbsp;&nbsp;[html] and [file] are two mutually exclusive 
parameters");
				using (TextReader reader = File.OpenText(Page.Server.MapPath(e.Params["file"].Get<string>())))
				{
					e.Params["html"].Value = reader.ReadToEnd();
					e.Params["file"].UnTie(); // removing file object, to not confuse HtmlViewer ...
				}
			}

			LoadModule(
				"Magix.forms.HtmlViewer", 
				e.Params["container"].Get<string>(), 
				e.Params);
		}
	}
}

