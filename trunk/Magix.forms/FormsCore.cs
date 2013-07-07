/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.forms
{
	/**
	 * controller that makes it possible to create new dynamically created forms
	 */
	public class FormsCore : ActiveController
	{
		/**
		 * will create, and instantiate, a newly created dynamic form
		 */
		[ActiveEvent(Name = "magix.forms.create-web-part")]
		public void magix_forms_create_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.forms.create-web-part"].Value = "content1";
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
				throw new ArgumentException("you need a [container] for your create-web-part");

			LoadModule(
				"Magix.forms.WebPart", 
				e.Params["container"].Get<string>(), 
				e.Params);
		}
		
		/**
		 * will create, and instantiate, a newly created dynamic web page
		 */
		[ActiveEvent(Name = "magix.forms.load-mml-web-part")]
		public void magix_forms_load_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.forms.load-mml-web-part"].Value = null;
				e.Params["container"].Value = "header";
				e.Params["form-id"].Value = "harvester";
				e.Params["html"].Value = "<p>some html, or file.&nbsp;&nbsp;[html] and [file] are mutually exclusive parameters</p>";
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

			Node tmp = e.Params.Clone();

			if (!tmp.Contains("container"))
				throw new ArgumentException("load-mml-web-part needs a [container] parameter");

			if (!tmp.Contains("file"))
				throw new ArgumentException("load-mml-web-part needs a [file] parameter");

			using (TextReader reader = File.OpenText(Page.Server.MapPath(tmp["file"].Get<string>())))
			{
				tmp["mml"].Value = reader.ReadToEnd();
				tmp["file"].UnTie(); // removing file object, to not confuse HtmlViewer ...
			}

			LoadModule(
				"Magix.forms.WebPart", 
				tmp["container"].Get<string>(), 
				tmp);
		}
		
		/**
		 * will create, and instantiate, a newly created dynamic web part
		 */
		[ActiveEvent(Name = "magix.forms.create-mml-web-part")]
		public void magix_forms_create_mml_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params.Clear();
				e.Params["event:magix.forms.create-mml-web-part"].Value = null;
				e.Params["inspect"].Value = @"creates a dynamic magix markup language web part.
&nbsp;&nbsp;not thread safe";
				e.Params["container"].Value = "header";
				e.Params["form-id"].Value = "my-form";
				e.Params["css"].Value = "css-classes-of-container";
				e.Params["mml"].Value = @"<p>magix markup language goes here
{{
link-button=>btn
  text=>howdy
}}</p>";
				return;
			}

			if (!e.Params.Contains("container"))
				throw new ArgumentException("create-web-part needs a [container] parameter");

			if (!e.Params.Contains("mml"))
				throw new ArgumentException("create-web-part needs an [mml] parameter");

			LoadModule(
				"Magix.forms.WebPart", 
				e.Params["container"].Get<string>(), 
				e.Params);
		}

		/**
		 * lists all widget types that exists in system
		 */
		[ActiveEvent(Name="magix.forms.list-widget-types")]
		protected static void magix_forms_list_widget_types(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"lists all widget types as [types] available in system.&nbsp;&nbsp;
not thread safe";
				e.Params["magix.forms.list-widget-types"].Value = null;
				return;
			}

			Node tmp = new Node();
			tmp["begins-with"].Value = "magix.forms.controls.";

			RaiseActiveEvent(
				"magix.admin.get-active-events",
				tmp);

			Node ip = e.Params;

			foreach (Node idx in tmp["events"])
			{
				Node tp = new Node("widget");

				tp["type"].Value = idx.Get<string>();
				tp["properties"]["id"].Value = "id";

				Node tp2 = new Node();
				tp2["inspect"].Value = null;

				RaiseActiveEvent(
					idx.Get<string>(),
					tp2);

				if (tp2.Contains("_no-embed"))
					tp["_no-embed"].Value = true;

				foreach (Node idx2 in tp2["controls"][0])
				{
					tp["properties"][idx2.Name].Value = idx2.Value;
					tp["properties"][idx2.Name].AddRange(idx2);
				}

				ip["types"].Add(tp);
			}
		}
	}
}

