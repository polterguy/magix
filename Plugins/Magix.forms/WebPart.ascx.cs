/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web;
using System.Web.UI;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using Magix.Core;
using Magix.UX;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.forms
{
    /*
     * creates a web part consisting of magix markup language or a control collection
     */
    public class WebPart : ActiveModule
    {
		private bool isFirst;

        /*
         * node data source for web part
         */
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

        /*
         * unique form-id of web part
         */
		private string FormID
		{
			get { return ViewState["FormID"] as string; }
			set { ViewState["FormID"] = value; }
		}

        /*
         * list of all active events associated with web part
         */
		private Dictionary<string, Node> Methods
		{
			get
			{
				if (ViewState["Methods"] == null )
					ViewState["Methods"] = new Dictionary<string, Node>();
				return ViewState["Methods"] as Dictionary<string, Node>; }
		}

        /*
         * sets up the web part
         */
        public override void InitialLoading(Node node)
        {
            isFirst = true;
            Node ip = Ip(node);
            Node dp = Dp(node);

            if (!ip.Contains("controls") && !ip.Contains("mml"))
                throw new ArgumentException("you must supply either a [controls] segment or an [mml] segment for your web part");

            // controls
            Node controlsDefinition = null;
            if (ip["controls"].Value != null)
                controlsDefinition = Expressions.GetExpressionValue<Node>(ip["controls"].Get<string>(), dp, ip, false).Clone();
            else
                controlsDefinition = ip["controls"].Clone();

            // controls
            Node eventsDefinition = new Node();
            if (ip.Contains("events"))
            {
                if (ip["events"].Value != null)
                    eventsDefinition = Expressions.GetExpressionValue<Node>(ip["events"].Get<string>(), dp, ip, false).Clone();
                else
                    eventsDefinition = ip["events"].Clone();
            }

            // form-id
            string formId = Expressions.GetExpressionValue<string>(ip["form-id"].Get<string>(), dp, ip, false);

            // mml
            string mml = null;
            if (ip.Contains("mml"))
            {
                mml = Expressions.GetExpressionValue<string>(ip["mml"].Get<string>(), dp, ip, false);
            }

            Load +=
                delegate
                {
                    FormID = formId;

                    if (!string.IsNullOrEmpty(mml))
                    {
                        // mml form
                        TokenizeMarkup(mml);
                    }
                    else
                    {
                        DataSource["controls"].Value = controlsDefinition;
                        foreach (Node idxEvent in eventsDefinition)
                        {
                            Methods[idxEvent.Name] = idxEvent.Clone();
                        }
                    }
                };
            base.InitialLoading(node);
        }

		/*
		 * raises web part dynamic active events
		 */
		[ActiveEvent(Name = "")]
		private void magix_null_event_handler(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (Methods.ContainsKey(e.Name))
			{
				if (ShouldInspect(ip))
				{
                    AppendInspectFromResource(
                        ip["inspect"],
                        "Magix.forms",
                        "Magix.forms.hyperlisp.inspect.hl",
                        "[magix.forms.web-part-null-event-handler-dox].value");
                    ip.AddRange(Methods[e.Name].Clone());
					return;
				}

				Node tmp = Methods[e.Name].Clone();

				// cloning the incoming parameters
                if (ip.Count > 0)
                    tmp["$"].AddRange(ip.Clone());

				RaiseActiveEvent(
					"magix.execute",
					tmp);

				if (tmp.Contains("$"))
				{
                    ip.Clear();
                    ip.AddRange(tmp["$"]);
				}
			}

            // shortcutting execute keywords for efficiency
            if (DataSource.ContainsValue("controls"))
            {
                if (!HasSetCachedControlEvents)
                {
                    HasSetCachedControlEvents = true;
                    foreach (Node idx in SearchEvents(e.Name, DataSource["controls"].Get<Node>()))
                    {
                        Node tmp = idx.Clone();

                        // cloning the incoming parameters
                        if (ip.Count > 0)
                            tmp["$"].AddRange(ip.Clone());

                        RaiseActiveEvent(
                            "magix.execute",
                            tmp);

                        if (tmp.Contains("$"))
                        {
                            ip.Clear();
                            ip.AddRange(tmp["$"]);
                        }
                    }
                }
                else
                {
                    if (CachedControlEvents.ContainsKey(e.Name))
                    {
                        Node cache = CachedControlEvents[e.Name];
                        foreach (Node idx in cache)
                        {
                            Node tmp = idx.Get<Node>().Clone();

                            // cloning the incoming parameters
                            if (ip.Count > 0)
                                tmp["$"].AddRange(ip.Clone());

                            RaiseActiveEvent(
                                "magix.execute",
                                tmp);

                            if (tmp.Contains("$"))
                            {
                                ip.Clear();
                                ip.AddRange(tmp["$"]);
                            }
                        }
                    }
                }
            }
		}

        /*
         * contains a shallow copy of all control events
         */
        private Dictionary<string, Node> CachedControlEvents
        {
            get
            {
                if (ViewState["CachedControlEvents"] == null)
                    ViewState["CachedControlEvents"] = new Dictionary<string, Node>();
                return ViewState["CachedControlEvents"] as Dictionary<string, Node>;
            }
        }

        /*
         * if true, then control active events cache have been initialized
         */
        private bool HasSetCachedControlEvents
        {
            get
            {
                if (ViewState["HasSetCachedControlEvents"] == null)
                    return false;
                return (bool)ViewState["HasSetCachedControlEvents"];
            }
            set
            {
                ViewState["HasSetCachedControlEvents"] = value;
            }
        }

        /*
         * searches through all controls for event handlers for the given name
         */
        private IEnumerable<Node> SearchEvents(string name, Node node)
        {
            if (node != null)
            {
                foreach (Node idx in node)
                {
                    if (idx.Name == "events")
                    {
                        foreach (Node idxInner in idx)
                        {
                            if (!CachedControlEvents.ContainsKey(idxInner.Name))
                                CachedControlEvents[idxInner.Name] = new Node();
                            CachedControlEvents[idxInner.Name].Add("", idxInner);
                            if (idxInner.Name == name)
                                yield return idxInner;
                        }
                    }
                    else
                    {
                        foreach (Node idxInner in SearchEvents(name, idx))
                        {
                            yield return idxInner;
                        }
                    }
                }
            }
        }

        /*
         * retrieves the form-id for a given dynamic container
         */
        [ActiveEvent(Name = "magix.forms.get-form-id")]
        private void magix_forms_get_form_id(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (e.Params.Contains("inspect"))
            {
                e.Params["inspect"].Value = @"returns the [form-id] for a given [container]

[container] can be both a constant or an expression";
                return;
            }

            Node dp = Dp(e.Params);

            string container = Expressions.GetExpressionValue<string>(ip["container"].Get<string>(), dp, ip, false);
            if (this.Parent.ID == container)
                ip["form-id"].Value = FormID;
        }
		
		/*
		 * changes the mml of web part
		 */
		[ActiveEvent(Name = "magix.forms.change-mml")]
		private void magix_forms_change_mml(object sender, ActiveEventArgs e)
		{
			if (e.Params.Contains("inspect"))
			{
				e.Params["inspect"].Value = @"changes the mml of the given [form-id] 
to the value in [mml]";
				return;
			}

            HasSetCachedControlEvents = false;
            CachedControlEvents.Clear();

            if (FormID == Ip(e.Params)["form-id"].Get<string>())
			{
                Node ip = Ip(e.Params);
                Node dp = Dp(e.Params);

				DataSource = new Node();
				Methods.Clear();
                TokenizeMarkup(Expressions.GetExpressionValue<string>(ip["mml"].Get<string>(), dp, ip, false));
				this.Controls.Clear();
				isFirst = true;
				BuildControls();
				((DynamicPanel)this.Parent).ReRender();
			}
		}

		/*
		 * returns the given [id] control
		 */
		[ActiveEvent(Name = "magix.forms._get-control")]
		private void magix_forms__get_control(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                if (ip["inspect"].Value == null)
                    AppendInspectFromResource(
                        ip["inspect"],
                        "Magix.forms",
                        "Magix.forms.hyperlisp.inspect.hl",
                        "[magix.forms._get-control-dox].value");
				return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("id"))
				throw new ArgumentException("you need to supply an [id] to know which control to find");

            if (!ip.Contains("form-id") || Expressions.GetExpressionValue<string>(ip["form-id"].Get<string>(), dp, ip, false) == FormID)
			{
                string id = Expressions.GetExpressionValue<string>(ip["id"].Get<string>(), dp, ip, false);
                Control ctrl = Selector.FindControl<Control>(
                    this.Parent.Parent /* to include viewport itself */, 
                    id);
                if (ctrl != null)
                    ip["_ctrl"].Value = ctrl;
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

		private void BuildControl(Node ctrlNode, Control parent)
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

							tmp["code"].Value = buffer;

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

