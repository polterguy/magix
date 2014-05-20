/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
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
    /*
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
                Node ip = Ip(node);
                Node dp = Dp(node);

				if (ip.Contains("form-id"))
					FormID = Expressions.GetExpressionValue(ip["form-id"].Get<string>(), dp, ip, false) as string;

				if (ip.Contains("mml"))
				{
					// mml form
                    string mml = Expressions.GetExpressionValue(ip["mml"].Get<string>(), dp, ip, false) as string;
					TokenizeMarkup(mml);
				}
				else
				{
                    if (!ip.Contains("controls"))
                        throw new ArgumentException("you must supply a [controls] segment for your web part");
                    
                    if (!string.IsNullOrEmpty(ip["controls"].Get<string>()))
                    {
                        DataSource["controls"].Value = 
                            (Expressions.GetExpressionValue(ip["controls"].Get<string>(), dp, ip, false) as Node).Clone();
                    }
                    else
                    {
                        DataSource["controls"].Value = ip["controls"].Clone();
                    }

					if (ip.Contains("events"))
					{
                        Node evts = ip["events"];
                        if (!string.IsNullOrEmpty(ip["events"].Get<string>()))
                            evts = Expressions.GetExpressionValue(ip["events"].Get<string>(), dp, ip, false) as Node;
						foreach (Node idxEvent in evts)
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
					e.Params.AddRange(Methods[e.Name].Clone());
					return;
				}

				Node tmp = Methods[e.Name].Clone();

				// cloning in the incoming parameters
                if (Ip(e.Params).Count > 0)
                    tmp["$"].AddRange(Ip(e.Params).Clone());

				RaiseActiveEvent(
					"magix.execute",
					tmp);

				if (tmp.Contains("$"))
				{
                    Ip(e.Params).Clear();
                    Ip(e.Params).AddRange(tmp["$"]);
				}
			}
		}
		
		/**
		 * raises form events, if match
		 */
		[ActiveEvent(Name = "magix.forms.change-mml")]
		protected void magix_forms_change_mml(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"changes the mml of the given [form-id] 
to the value in [mml]";
				return;
			}

            if (FormID == Ip(e.Params)["form-id"].Get<string>())
			{
                Node ip = Ip(e.Params);
                Node dp = Dp(e.Params);

				DataSource = new Node();
				Methods.Clear();
                TokenizeMarkup(Expressions.GetExpressionValue(ip["mml"].Get<string>(), dp, ip, false) as string);
				this.Controls.Clear();
				isFirst = true;
				BuildControls();
				((DynamicPanel)this.Parent).ReRender();
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
if [form-id] matches and control with given [id] is found.&nbsp;&nbsp;
[form-id] is conditional, and if not given, will return first control that matches [id], 
which might be dangerous if you have multiple forms on page.&nbsp;&nbsp;
no sample code, since not invokable through hyperlisp in a sane way.&nbsp;&nbsp;
not thread safe";
				return;
			}

            if (!Ip(e.Params).Contains("id"))
				throw new ArgumentException("need [id] to know which control to find");

            if (!Ip(e.Params).Contains("form-id") || FormID == Ip(e.Params)["form-id"].Get<string>())
			{
                Control ctrl = Selector.FindControl<Control>(this.Parent /* to include viewport */, Ip(e.Params)["id"].Get<string>());
                if (ctrl != null)
                    Ip(e.Params)["_ctrl"].Value = ctrl;
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

                node["_first"].Value = isFirst;
                node["_code"].Value = ctrlNode;

				RaiseActiveEvent(
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

		private void TokenizeMarkup(string mml)
		{
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

							RaiseActiveEvent(
								"magix.execute.code-2-node",
								tmp);

							DataSource.Add(new Node("controls", tmp["node"].Clone()));

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

