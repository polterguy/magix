/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/**
	 * contains the form control
	 */
	public class FormElementCore : BaseWebControlCore
	{
		/**
		 * set-enabled
		 */
		[ActiveEvent(Name = "magix.forms.set-enabled")]
		protected void magix_forms_set_enabled(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.set-enabled"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"sets the enabled property of the given 
[id] web control, in the [form-id] form, from [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(e.Params);

			if (ctrl != null)
			{
				bool enabled = false;
				if (e.Params.Contains("value"))
					enabled = e.Params["value"].Get<bool>();

				ctrl.Enabled = enabled;
			}
		}

		/**
		 * get-enabled
		 */
		[ActiveEvent(Name = "magix.forms.get-enabled")]
		protected void magix_forms_get_enabled(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect") && e.Params["inspect"].Value == null)
			{
				e.Params["event:magix.forms.get-enabled"].Value = null;
				e.Params["id"].Value = "control";
				e.Params["form-id"].Value = "webpages";
				e.Params["value"].Value = true;
				e.Params["inspect"].Value = @"retrieves the enabled property of the given 
[id] web control, in the [form-id] form, into [value].&nbsp;&nbsp;not thread safe";
				return;
			}

			BaseWebControlFormElement ctrl = FindControl<BaseWebControlFormElement>(e.Params);

			if (ctrl != null)
			{
				e.Params["value"].Value = ctrl.Enabled;
			}
		}
	}
}

