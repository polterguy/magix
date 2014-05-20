/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/*
	 * contains the button control
	 */
	public class LambdaController : ActiveController
	{
		/*
		 * will create, and instantiate a new lambda controls
		 */
		[ActiveEvent(Name = "magix.forms.controls.lambda")]
		public void magix_forms_controls_lambda(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				AppendInspect(ip["inspect"], @"creates a lambda type of web control

the lambda control is a control which have late creation of its child controls.  this 
means that when the lambda is declared, it is not obvious which controls it will 
contain, but rather due to hyperlisp logic within your [oncreatecontrols] hyperlisp 
active event, the lambda control will dynamically create its child control collection, 
only once rendered.  the lambda might appear similar to the dynamic control, but has 
some unique features which makes it useful in places where the dynamic web control 
doesn't quite do the trick

[oncreatecontrols] is the hyperlisp code executing when the control is lately bound.  
the oncreatecontrols event is expected to return control(s), or nothing, in the [$] 
return node.  return any web controls or controls within the [$] collection to create 
a dynamically created control collection.  the lambda control doesn't render any outher 
html, but only serves as a container of other controls, without any markup of its own");
                ip["magix.forms.create-web-part"]["container"].Value = "content5";
                ip["magix.forms.create-web-part"]["form-id"].Value = "sample-form";
                ip["magix.forms.create-web-part"]["controls"]["lambda"].Value = "idOfLambda";
                ip["magix.forms.create-web-part"]["controls"]["lambda"]["oncreatecontrols"]["set"].Value = "[$][link-button][value].Value";
                ip["magix.forms.create-web-part"]["controls"]["lambda"]["oncreatecontrols"]["set"]["value"].Value = "howdy world :)";
				return;
			}

            Node code = ip["_code"].Get<Node>();
			if (!code.Contains("oncreatecontrols"))
			{
				Label lbl = new Label();
				lbl.Value = "{{lambda control}}";
				lbl.ID = code.Value as string;
                ip["_ctrl"].Value = lbl;

				if (code.Contains("onclick"))
				{
					// design select support
					Node codeNode = code["onclick"].Clone();

					lbl.Click += delegate(object sender2, EventArgs e2)
					{
						RaiseActiveEvent(
							"magix.execute",
							codeNode);
					};
				}
				return; // nothing to render unless event handler is defined ...
			}

			string id = code.Get<string>();
			if (id == null && !code.Contains("id"))
				throw new ArgumentException("a [lambda] control must have a unique [id]");

			if (id == null)
				id = code["id"].Get<string>();
			if (id == null)
				throw new ArgumentException("a [lambda] control must have a unique [id]");

			Node exe = new Node();
			if (code.Contains("_buffer"))
			{
				exe["$"].UnTie();
				exe["$"].AddRange(code["_buffer"].Clone());
			}
			else
			{
				exe = code["oncreatecontrols"].Clone();

				RaiseActiveEvent(
					"magix.execute",
					exe);
				code["_buffer"].UnTie();
				code["_buffer"].AddRange(exe["$"]); 
			}

			if (exe.Contains("$"))
			{
                ip["_ctrl"].Value = null;
				foreach (Node idxCtrl in exe["$"])
				{
					string evtName = "magix.forms.controls." + idxCtrl.Name;

					Node node = new Node();

					node["_code"].Value = idxCtrl;
                    node["_first"].Value = e.Params["_first"].Value;

					RaiseActiveEvent(
						evtName,
						node);

					idxCtrl["_first"].UnTie();

					if (node.Contains("_ctrl"))
					{
						if (node["_ctrl"].Value != null)
                            ip["_ctrl"].Add(new Node("_x", node["_ctrl"].Value));
						else
						{
							// multiple controls returned ...
							foreach (Node idxCtrl2 in node["_ctrl"])
							{
                                ip["_ctrl"].Add(new Node("_x", idxCtrl2.Value));
							}
						}
					}

					code["_buffer"].Value = true;
				}
			}
			else
                ip["_ctrl"].Value = null;
		}
	}
}