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
				e.Params["event:magix.forms.create-form"].Value = "content1";
				e.Params["container"].Value = "content1";
				e.Params["css"].Value = "css class(es) of your form";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["events"]["magix.forms.widget-selected"]["magix.forms.show-message"]["message"].Value = "jo, something was selected";
				e.Params["controls"]["button"].Value = "btn";
				e.Params["controls"]["button"]["text"].Value = "Hello World!";
				e.Params["inspect"].Value = @"creates a dynamic form
and loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;[controls]
contains the controls themselves, [css] becomes the css classes of your form
.&nbsp;&nbsp;not thread safe";
				return;
			}

			if (!e.Params.Contains("container"))
				throw new ArgumentException("you need a [container] for your create-form");

			if (!e.Params.Contains("form-id"))
				throw new ArgumentException("you need a [form-id] for your create-form");

			LoadModule(
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
so on.&nbsp;&nbsp;you can embed forms using this syntax
{{form:name_of_form}}.&nbsp;&nbsp;not thread safe";
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
		
		/**
		 * Will create, and instantiate, a newly created dynamic web page
		 */
		[ActiveEvent(Name = "magix.forms.controls.lambda")]
		public void magix_forms_controls_lambda(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.forms.create-form"].Value = null;
				e.Params["inspect"].Value = @"creates a lambda type of web control.&nbsp;&nbsp;
[oncreatecontrols] is the hyper lisp code executing when the 
control is lately bound.&nbsp;&nbsp;the [oncreatecontrols] event is expected 
to return control(s), or nothing, in the [$] return node";
				e.Params["container"].Value = "content5";
				e.Params["form-id"].Value = "sample-form";
				e.Params["controls"]["lambda"]["oncreatecontrols"]["set"].Value = "[$][link-button][text].Value";
				e.Params["controls"]["lambda"]["oncreatecontrols"]["set"]["value"].Value = "howdy world :)";
				return;
			}

			Node code = e.Params["_code"].Value as Node;

			if (!code.Contains("oncreatecontrols"))
				return; // nothing to render unless event handler is defined ...

			if (!code.Contains("id"))
				throw new ArgumentException("a [lambda] control must have a unique [id]");

			Node getC = new Node();
			getC["id"].Value = "magix.forms.controls.lambda_" + code["id"].Get<string>();

			RaiseActiveEvent(
				"magix.viewport.get-viewstate",
				getC);

			Node exe = null;

			if (getC.Contains("value"))
			{
				exe = new Node();
				exe["$"].AddRange(getC["value"].Clone());
			}
			else
			{
				exe = code["oncreatecontrols"].Clone();
				
				RaiseActiveEvent(
					"magix.execute",
					exe);

				Node cache = new Node();

				cache["id"].Value = "magix.forms.controls.lambda_" + code["id"].Get<string>();
				cache["value"].AddRange(exe["$"]);

				RaiseActiveEvent(
					"magix.viewport.set-viewstate",
					cache);
			}

			if (!exe.Contains("_stop") && exe.Contains("$"))
			{
				e.Params["_ctrl"].Value = null;
				foreach (Node idxCtrl in exe["$"])
				{
					string evtName = "magix.forms.controls." + idxCtrl.Name;

					Node node = new Node();

					node["_code"].Value = idxCtrl;
					idxCtrl["_first"].Value = idxCtrl["_first"].Value;

					RaiseActiveEvent(
						evtName,
						node);

					idxCtrl["_first"].UnTie();

					if (node.Contains("_ctrl"))
					{
						if (node["_ctrl"].Value != null)
							e.Params["_ctrl"].Add(new Node("_x", node["_ctrl"].Value));
						else
						{
							// multiple controls returned ...
							foreach (Node idxCtrl2 in node["_ctrl"])
							{
								e.Params["_ctrl"].Add(new Node("_x", idxCtrl2.Value));
							}
						}
					}

					e.Params["_buffer"].Value = true;
				}
			}
			else
				e.Params["_ctrl"].Value = null;
		}
	}
}

























