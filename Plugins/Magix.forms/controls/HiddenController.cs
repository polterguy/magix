/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
	/*
	 * hidden field
	 */
    public class HiddenController : BaseControlController
	{
		/*
		 * creates hidden field
		 */
		[ActiveEvent(Name = "magix.forms.controls.hidden")]
		public void magix_forms_controls_hidden(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

			Hidden ret = new Hidden();
            FillOutParameters(e.Params, ret);

            Node node = ip["_code"].Get<Node>();
            if (node.Contains("value") && node["value"].Value != null)
				ret.Value = node["value"].Get<string>();

            ip["_ctrl"].Value = ret;
		}

		protected override void Inspect (Node node)
		{
            node["inspect"].Value = @"
<p>creates a hidden field input type of control.&nbsp;&nbsp;
hidden fields are invisible to the user, but can store any 
data as text strings within their [value] node</p>";
            node["magix.forms.create-web-part"]["container"].Value = "content5";
            node["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["hidden"]);
            node["magix.forms.create-web-part"]["controls"]["hidden"]["value"].Value = "whatever you wish to put in as value";
            node["inspect"].Value = node["inspect"].Value + @"
<p><strong>properties for hidden</strong><p>[value] is the 
actual value of the hidden field.&nbsp;&nbsp;this is the 
property which is changed or retrieved when you invoke the 
[magix.forms.set-value] and the [magix.forms.get-value] for 
your control</p>";
		}
		
		/*
		 * helper for events such that value can be passed into event handlers
		 */
		protected override object GetValue(BaseControl that)
		{
			return ((Hidden)that).Value;
		}
	}
}
