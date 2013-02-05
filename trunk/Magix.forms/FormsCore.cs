/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2012 - MareMara13@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * Level3: Controller that encapsulates execution engine
	 */
	[ActiveController]
	public class FormsCore : ActiveController
	{
		/**
		 */
		[ActiveEvent(Name = "magix.forms.create-form")]
		public void magix_forms_create_form (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("container") || !e.Params.Contains ("form-id"))
			{
				e.Params["container"].Value = "content2";
				e.Params["form-id"].Value = "unique-identification-of-your-form";
				e.Params["controls"]["Button"].Value = "btn";
				e.Params["controls"]["Button"]["Text"].Value = "Hello World!";
				e.Params["context"].Value = "[OR][Path][To][Form]";
				return;
			}
			Node formContent = e.Params;
			if (e.Params.Contains ("context"))
				formContent = Expressions.GetExpressionValue (
					e.Params["context"].Get<string>(), 
					e.Params, 
					e.Params) as Node;
			LoadModule (
				"Magix.forms.DynamicForm", 
				e.Params["container"].Get<string>(), 
				formContent.Clone ());
		}
	}
}

