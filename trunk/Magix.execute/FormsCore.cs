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
		[ActiveEvent(Name = "magix.forms.load-form")]
		public void magix_forms_load_form (object sender, ActiveEventArgs e)
		{
			if (!e.Params.Contains ("Container"))
			{
				e.Params["Container"].Value = "content1|content2|content3";
				e.Params["Button"].Value = "btn";
				e.Params["Button"]["Text"].Value = "Hello World!";
				return;
			}
			LoadModule (
				"Magix.SampleModules.DynamicForm", 
				e.Params["Container"].Get<string>(), 
				e.Params);
		}
	}
}

