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
                e.Params["inspect"].Value = @"<p>creates a dynamic form and loads 
it into the [container] viewport container</p><p>[form-id] must be a unique id.
&nbsp;&nbsp;[controls] contains the controls themselves, and is a list of the 
controls the form contains.&nbsp;&nbsp;[class] is the css class(es) you wish to 
use for your form</p><p>you can also associate temporary active events with your 
form, which will only exist as long as your form exists.&nbsp;&nbsp;you do this 
by adding active events directly beneath the [events] node, where the name of the 
active event becomes the first node, and the code to execute is directly beneath 
the active event declaration</p><p>all parameters can be either constants or 
expressions.&nbsp;&nbsp;if you use expressions for the [events] and [controls] 
nodes, then you add the expression as the value of the [event] and/or the 
[controls] node, and whatever node these expressions returns, will become what 
the form uses to load its controls/events</p><p>not thread safe</p>";
                e.Params["magix.forms.create-web-part"]["container"].Value = "content3";
                e.Params["magix.forms.create-web-part"]["class"].Value = "css class(es) of your form";
                e.Params["magix.forms.create-web-part"]["form-id"].Value = "unique-id";
                e.Params["magix.forms.create-web-part"]["events"]["magix.forms.control-clicked"]["magix.viewport.show-message"]["message"].Value = "yo";
                e.Params["magix.forms.create-web-part"]["controls"]["button"].Value = "btn";
                e.Params["magix.forms.create-web-part"]["controls"]["button"]["value"].Value = "click me";
                e.Params["magix.forms.create-web-part"]["controls"]["button"]["onclick"]["magix.forms.control-clicked"].Value = null;
                return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

            string container = null;
            if (ip.Contains("container"))
                container = Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string;

			LoadActiveModule(
				"Magix.forms.WebPart",
                container,
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
				e.Params["file"].Value = "sample-scripts/email-harvester.mml";
				e.Params["inspect"].Value = @"creates a dynamic magix markup language web page from a file,
loading it into the [container] viewport container.&nbsp;&nbsp;[form-id]
must be a uniquely identifiable id for later use.&nbsp;&nbsp;
you can intermix webcontrols into your mml by creating a control 
collection by typing them inside of brackets such as 
{{...controls, using hyper lisp syntax and code in
event handlers goes here...}}.&nbsp;&nbsp;internally it 
calls LoadActiveModule, hence all the parameters that goes
into your magix.viewport.load-module active event, 
can also be passed into this, such as [class] and 
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

			LoadActiveModule(
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
				e.Params["class"].Value = "css-classes-of-container";
				e.Params["mml"].Value = @"<p>magix markup language goes here
{{
link-button=>btn
  value=>howdy
}}</p>";
				return;
			}

            if (!Ip(e.Params).Contains("container"))
				throw new ArgumentException("create-web-part needs a [container] parameter");

            if (!Ip(e.Params).Contains("mml"))
				throw new ArgumentException("create-web-part needs an [mml] parameter");

			LoadActiveModule(
				"Magix.forms.WebPart",
                Ip(e.Params)["container"].Get<string>(),
                Ip(e.Params));
		}
	}
}

