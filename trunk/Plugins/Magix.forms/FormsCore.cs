/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
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

            if (!Ip(e.Params).Contains("container"))
				throw new ArgumentException("you need a [container] for your create-web-part");

			LoadModule(
				"Magix.forms.WebPart",
                Ip(e.Params)["container"].Get<string>(),
                Ip(e.Params));
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
				e.Params["file"].Value = "sample-scripts/email-harvester.mml";
				e.Params["inspect"].Value = @"creates a dynamic magix markup language web page from a file,
loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;
you can intermix webcontrols into your mml by creating a control 
collection by typing them inside of brackets such as 
{{...controls, using hyper lisp syntax and code in
event handlers goes here...}}.&nbsp;&nbsp;internally it 
calls LoadModule, hence all the parameters that goes
into your magix.viewport.load-module active event, 
can also be passed into this, such as [css] and 
so on.&nbsp;&nbsp;you can embed forms using this syntax
{{form=>name_of_form}}.&nbsp;&nbsp;not thread safe";
				return;
			}

            Node tmp = Ip(e.Params).Clone();

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

            if (!Ip(e.Params).Contains("container"))
				throw new ArgumentException("create-web-part needs a [container] parameter");

            if (!Ip(e.Params).Contains("mml"))
				throw new ArgumentException("create-web-part needs an [mml] parameter");

			LoadModule(
				"Magix.forms.WebPart",
                Ip(e.Params)["container"].Get<string>(),
                Ip(e.Params));
		}
	}
}

