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
    internal sealed class LambdaController : ActiveController
	{
		/*
		 * will create, and instantiate a new lambda controls
		 */
		[ActiveEvent(Name = "magix.forms.controls.lambda")]
		private void magix_forms_controls_lambda(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.lambda-dox-start].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.lambda-sample-start]");
				return;
			}

            Node code = ip["_code"].Get<Node>();

			string id = code.Get<string>() ?? code["id"].Get<string>(null);
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