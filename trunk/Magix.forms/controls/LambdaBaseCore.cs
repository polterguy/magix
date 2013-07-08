/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;

namespace Magix.forms
{
	/**
	 * contains the button control
	 */
	public class LambdaBaseCore : ActiveController
	{
		/**
		 * will create, and instantiate a new pambda controls
		 */
		[ActiveEvent(Name = "magix.forms.controls.lambda")]
		public void magix_forms_controls_lambda(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.forms.create-web-part"].Value = null;
				e.Params["inspect"].Value = @"creates a lambda type of web control.&nbsp;&nbsp;
[oncreatecontrols] is the hyper lisp code executing when the 
control is lately bound.&nbsp;&nbsp;the [oncreatecontrols] event is expected 
to return control(s), or nothing, in the [$] return node";
				e.Params["container"].Value = "content5";
				e.Params["form-id"].Value = "sample-form";
				e.Params["controls"]["lambda"].Value = "idOfLambda";
				e.Params["controls"]["lambda"]["oncreatecontrols"]["set"].Value = "[$][link-button][text].Value";
				e.Params["controls"]["lambda"]["oncreatecontrols"]["set"]["value"].Value = "howdy world :)";
				return;
			}

			Node code = (e.Params["_code"].Value as Node);

			if (!code.Contains("oncreatecontrols"))
			{
				Label lbl = new Label();
				lbl.Text = "{{lambda control}}";
				lbl.ID = code.Value as string;
				e.Params["_ctrl"].Value = lbl;

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

			string id = code.Value as string;

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

					code["_buffer"].Value = true;
				}
			}
			else
				e.Params["_ctrl"].Value = null;
		}
	}
}