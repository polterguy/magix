/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.Core;
using System.Web;
using System.Configuration;
using System.Collections.Generic;
using System.Diagnostics;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
    /**
     * create a web part consisting of magix markup language
     */
    public class WebPart : ActiveModule
    {
		private bool isFirst;

		private Node DataSource
		{
			get
			{
				if (ViewState["DataSource"] == null)
					ViewState["DataSource"] = new Node();

				return ViewState["DataSource"] as Node;
			}
			set { ViewState["DataSource"] = value; }
		}

		protected string FormID
		{
			get { return ViewState["FormID"] as string; }
			set { ViewState["FormID"] = value; }
		}

		protected Dictionary<string, Node> Methods
		{
			get
			{
				if (ViewState["Methods"] == null )
					ViewState["Methods"] = new Dictionary<string, Node>();
				return ViewState["Methods"] as Dictionary<string, Node>; }
		}

		public override void InitialLoading(Node node)
		{
			isFirst = true;
			Load +=
			delegate
			{
				if (node.Contains("form-id"))
					FormID = node["form-id"].Get<string>();

				if (node.Contains("mml"))
				{
					// mml form
					TokenizeMarkup(node);
				}
				else
				{
					// web controls form
					DataSource["controls"].Value = node["controls"].Clone();

					if (node.Contains("events"))
					{
						foreach (Node idxEvent in node["events"])
						{
							Methods[idxEvent.Name] = idxEvent.Clone();
						}
					}
				}
			};
			base.InitialLoading(node);
		}

		/**
		 * raises form events, if match
		 */
		[ActiveEvent(Name = "")]
		protected void magix_null_event_handler(object sender, ActiveEventArgs e)
		{
			if (Methods.ContainsKey(e.Name))
			{
				if (e.Params.Contains("inspect"))
				{
					e.Params["inspect"].Value = "dynamically created active event attached to form object " + FormID;
					return;
				}

				Node tmp = Methods[e.Name].Clone();

				// cloning in the incoming parameters
				if (e.Params.Count > 0)
					tmp["$"].AddRange(e.Params.Clone());

				RaiseEvent(
					"magix.execute",
					tmp);

				if (tmp.Contains("$"))
				{
					e.Params.ReplaceChildren(tmp["$"]);
				}
			}
		}

		/**
		 * raises form events, if match
		 */
		[ActiveEvent(Name = "magix.forms._get-control")]
		protected void magix_forms__get_control(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"returns the given control as [_ctrl] to caller 
if [form-id] matches and control with given [id] is found";
				return;
			}

			if (!e.Params.Contains("id"))
				throw new ArgumentException("need [id] to know which control to find");

			if (!e.Params.Contains("form-id") || FormID == e.Params["form-id"].Get<string>())
			{
				Control ctrl = Selector.FindControl<Control>(this.Parent /* to include viewport */, e.Params["id"].Get<string>());
				e.Params["_ctrl"].Value = ctrl;
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			EnsureChildControls();
		}

		protected override void CreateChildControls()
		{
			base.CreateChildControls();
			BuildControls();
		}

		private void BuildControls()
		{
			foreach (Node idx in DataSource)
			{
				if (idx.Name == "html")
				{
					LiteralControl ctrl = new LiteralControl();
					ctrl.Text = idx.Get<string>();
					Controls.Add(ctrl);
				}
				else
				{
					foreach (Node idxInner in idx.Get<Node>())
					{
						BuildControl(idxInner, this);
					}
				}
			}
		}

		protected void BuildControl(Node ctrlNode, Control parent)
		{
			string typeName = ctrlNode.Name;

			bool isControlName = true;
			foreach (char idxC in typeName)
			{
				if ("abcdefghijklmnopqrstuvwxyz-".IndexOf(idxC) == -1)
				{
					isControlName = false;
					break;
				}
			}

			if (!isControlName)
			{
				// assuming it's an event override method
				if (isFirst)
					Methods[typeName] = ctrlNode;
			}
			else
			{
				// this is a control
				string evtName = "magix.forms.controls." + typeName;

				Node node = new Node();

				node["_code"].Value = ctrlNode;
				ctrlNode["_first"].Value = isFirst;

				RaiseEvent(
					evtName,
					node);

				if (node.Contains("_ctrl"))
				{
					if (node["_ctrl"].Value != null)
						parent.Controls.Add(node["_ctrl"].Value as Control);
					else
					{
						// multiple controls returned ...
						foreach (Node idxCtrl in node["_ctrl"])
						{
							parent.Controls.Add(idxCtrl.Value as Control);
						}
					}
				}
				else
				{
					throw new ArgumentException("unknown control type in your template control '" + typeName + "'");
				}
			}
		}

		private void TokenizeMarkup(Node node)
		{
			string mml = node["mml"].Get<string>();

			bool isOutside = true;
			bool lastWasBracket = false;

			string buffer = "";

			for (int idx = 0; idx < mml.Length; idx++)
			{
				if (isOutside)
				{
					if (mml[idx] == '{')
					{
						if (lastWasBracket)
						{
							lastWasBracket = false;
							isOutside = false;
							buffer = buffer.Substring(0, buffer.Length - 1);

							// adding up to datasource as html
							Node tmp = new Node("html", buffer);
							DataSource.Add(tmp);

							// resetting buffer
							buffer = "";
						}
						else
						{
							lastWasBracket = true;
							buffer += mml[idx];
						}
					}
					else
					{
						lastWasBracket = false;
						buffer += mml[idx];
					}
				}
				else
				{
					if (mml[idx] == '}')
					{
						if (lastWasBracket)
						{
							lastWasBracket = false;
							isOutside = true;
							buffer = buffer.Substring(0, buffer.Length - 1);

							// adding up to datasource as web control tree
							Node tmp = new Node();

							tmp["code"].Value = buffer.Replace("&gt;", ">"); // TODO: refactor such that this is done during saving of form

							RaiseEvent(
								"magix.code.code-2-node",
								tmp);

							DataSource.Add(new Node("controls", tmp["json"].Get<Node>()));

							// resetting buffer
							buffer = "";
						}
						else
						{
							lastWasBracket = true;
							buffer += mml[idx];
						}
					}
					else
					{
						lastWasBracket = false;
						buffer += mml[idx];
					}
				}
			}
			if (!string.IsNullOrEmpty(buffer))
			{
				// adding up the last parts to datasource as html
				Node tmp = new Node("html", buffer);
				DataSource.Add(tmp);
			}
		}
	}
}

