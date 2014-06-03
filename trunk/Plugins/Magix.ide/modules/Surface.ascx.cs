/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Web.UI;
using System.Diagnostics;
using System.Configuration;
using System.Collections.Generic;
using Magix.UX;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Effects;
using Magix.UX.Widgets.Core;

namespace Magix.ide.modules
{
    /*
     * wysiwyg form builder surface
     */
    public class Surface : ActiveModule
    {
		protected Panel wrp;

        #region [ -- private properties -- ]

        /*
         * contains form controls
         */
		private Node DataSource
		{
			get { return ViewState["DataSource"] as Node; }
			set { ViewState["DataSource"] = value; }
		}

        /*
         * contains clipboard data
         */
        private Node ClipBoard
        {
            get { return Session["magix.ide.current-clipboard"] as Node; }
            set { Session["magix.ide.current-clipboard"] = value.Clone(); }
        }

        /*
         * contains undo chain
         */
        private Node UndoChain
        {
            get
            {
                if (ViewState["UndoChain"] == null)
                    ViewState["UndoChain"] = new Node();
                return ViewState["UndoChain"] as Node;
            }
        }

        /*
         * contains current undo chain index
         */
        private int UndoChainIndex
        {
            get { return ViewState["UndoChainIndex"] == null ? 0 : (int)ViewState["UndoChainIndex"]; }
            set { ViewState["UndoChainIndex"] = value; }
        }

        /*
         * the currently selected control, if any
         */
		private string SelectedControlDna
		{
            get { return Session["magix.ide.selected-control-dna"] as string; }
            set { Session["magix.ide.selected-control-dna"] = value; }
		}

        /*
         * true if surface is enabled
         */
        private bool SurfaceEnabled
        {
            get { return ViewState["SurfaceEnabled"] == null ? true : (bool)ViewState["SurfaceEnabled"]; }
            set { ViewState["SurfaceEnabled"] = value; }
        }

        #endregion

        #region [ -- page life cycle methods, and helpers -- ]

        public override void InitialLoading(Node node)
		{
			base.InitialLoading(node);
			Load +=
				delegate
				{
					DataSource = new Node("surface");
					DataSource["controls"].Value = null;
					BuildForm();
				};
		}

		protected override void OnLoad(EventArgs e)
		{
			BuildForm();
			base.OnLoad(e);
		}

        /*
         * builds the design form according to controls in datasource
         */
		private void BuildForm()
		{
			wrp.Controls.Clear();
            if (DataSource != null && DataSource.Contains("controls"))
			{
				foreach (Node idx in DataSource["controls"])
				{
					BuildControl(idx, wrp);
				}
			}
		}

        /*
         * creates one control
         */
		private void BuildControl(Node nodeControl, Control parent)
		{
			string controlType = "magix.forms.controls." + nodeControl.Name;

            Node nodeCodeCreateControl = new Node(nodeControl.Name, nodeControl.Get<string>());
			foreach (Node idxProperty in nodeControl)
			{
				// no event handlers here
                if (!idxProperty.Name.StartsWith("on"))
                {
                    nodeCodeCreateControl[idxProperty.Name].Value = idxProperty.Value;
                    if (idxProperty.Name != "controls")
                        nodeCodeCreateControl[idxProperty.Name].AddRange(idxProperty.Clone());
                }
			}

			// click event handler for selecting control
			nodeCodeCreateControl["onclick"]["magix.ide.select-control"]["dna"].Value = nodeControl.Dna;

			Node nodeCreateControl = new Node();
			nodeCreateControl["_code"].Value = nodeCodeCreateControl;
            nodeCreateControl["id-prefix"].Value = "__designer__";

			RaiseActiveEvent(
				controlType,
				nodeCreateControl);

            if (!nodeCreateControl.Contains("_ctrl"))
                throw new ArgumentException("control '" + controlType + "' not found");

            if (nodeCreateControl["_ctrl"].Value != null)
            {
                Control ctrlObject = nodeCreateControl["_ctrl"].Value as Control;
                VerifyGoodID(ctrlObject);
                AddSingleControl(nodeControl, parent, ctrlObject);
            }
            else
            {
                foreach (Node idxCtrl in nodeCreateControl["_ctrl"])
                {
                    Control ctrlObject = idxCtrl.Value as Control;
                    VerifyGoodID(ctrlObject);
                    AddSingleControl(nodeControl, parent, ctrlObject);
                }
            }
		}

        /*
         * verifies the id of one single control is good, according to html standard
         */
        private void VerifyGoodID(Control ctrlObject)
        {
            string id = ctrlObject.ID.Replace("__designer__", "");
            foreach (char idxChar in id)
            {
                if ("abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890_-".IndexOf(idxChar) == -1)
                    throw new ArgumentException("illegal control id, '" + id + "' is not legal");
            }
        }

        /*
         * adds one single control to parent control
         */
        private void AddSingleControl(Node nodeControl, Control parent, Control ctrlObject)
        {
            if (nodeControl.Dna == SelectedControlDna)
            {
                if (ctrlObject is BaseWebControl)
                    (ctrlObject as BaseWebControl).Class += " selected";
                else if (ctrlObject is System.Web.UI.WebControls.WebControl)
                    (ctrlObject as System.Web.UI.WebControls.WebControl).CssClass += " selected";
            }

            if (nodeControl.Contains("controls"))
            {
                foreach (Node idxChildControl in nodeControl["controls"])
                {
                    BuildControl(idxChildControl, ctrlObject);
                }
            }

            parent.Controls.Add(ctrlObject);
        }

        #endregion

        #region [ -- asp.net event handlers -- ]

        /*
         * design surface clicked, resetting selected control
         */
		protected void wrp_Click(object sender, EventArgs e)
		{
			SelectedControlDna = null;

			BuildForm();
			wrp.ReRender();

            RaiseSurfaceAndSelectionChangedChanged();
		}

        #endregion

        #region [ -- active events -- ]

        /*
         * lists controls in form object
         */
        [ActiveEvent(Name = "magix.ide.list-controls")]
		protected void magix_ide_list_controls(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.list-controls-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.list-controls-sample]");
                return;
			}

			foreach (Node idxControl in DataSource["controls"])
			{
				GetControl(idxControl, ip["controls"], 0);
            }
		}

		/*
		 * adds control to surface
		 */
		[ActiveEvent(Name = "magix.ide.add-control")]
		protected void magix_ide_add_control(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.add-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.add-control-sample]");
                return;
			}

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

			if (!ip.Contains("control"))
				throw new ArgumentException("no [control] given to [magix.ide.add-control]");

			if (ip["control"].Count != 1)
				throw new ArgumentException("you must supply exactly one control to [magix.ide.add-control]");

            AddControlToSurface(
                ip, 
                ip["control"][0].Clone());
		}

        /*
         * paste control
         */
        [ActiveEvent(Name = "magix.ide.paste-control")]
        protected void magix_ide_paste_control(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.paste-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.paste-control-sample]");
                return;
            }

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

            if (ClipBoard == null)
                throw new ArgumentException("no control in clipboard");

            Node controlToAddNode = ClipBoard.Clone();
            controlToAddNode.Value = GetNextAvailableControlId(controlToAddNode);

            AddControlToSurface(
                ip,
                controlToAddNode);
        }

		/*
		 * removes control from surface
		 */
		[ActiveEvent(Name = "magix.ide.remove-control")]
		protected void magix_ide_remove_control(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.remove-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.remove-control-sample]");
                return;
			}

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

            string dna = SelectedControlDna;
            if (ip.ContainsValue("dna"))
			    dna = ip["dna"].Get<string>();

            AddToUndoChain(!ip.ContainsValue("skip-undo") || !ip["skip-undo"].Get<bool>());

            SelectedControlDna = null;
            
            Node whereNode = DataSource.FindDna(dna);
            whereNode.UnTie();
			BuildForm();
			wrp.ReRender();
            RaiseSurfaceAndSelectionChangedChanged();
        }

        /*
         * changes properties of control
         */
        [ActiveEvent(Name = "magix.ide.change-control")]
		protected void magix_ide_change_control_(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.change-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.change-control-sample]");
                return;
			}

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

            if (!ip.Contains("change") || ip["change"].Count == 0)
				throw new ArgumentException("no [change] given to [magix.ide.change-control]");

            if (!ip.ContainsValue("dna"))
                throw new ArgumentException("no [dna] given to [magix.ide.change-control]");

            AddToUndoChain(!ip.ContainsValue("skip-undo") || !ip["skip-undo"].Get<bool>());
            
            Node controlNode = DataSource.FindDna(ip["dna"].Get<string>());

			if (ip.Contains("change"))
			{
				foreach (Node idxChange in ip["change"])
				{
					if (idxChange.Count > 0)
					{
						// code in tree format
						controlNode[idxChange.Name].AddRange(idxChange.Clone());
					}
					else
					{
                        if (idxChange.Name == "id")
                        {
                            controlNode.Value = idxChange.Get<string>();

                            Node getControlsNode = new Node();
                            RaiseActiveEvent(
                                "magix.ide.list-controls",
                                getControlsNode);
                            foreach (Node idxCtrl in getControlsNode["controls"])
                            {
                                string idOfIdx = idxCtrl.Get<string>().Split(':')[1];
                                if (controlNode.Get<string>() == idOfIdx)
                                    throw new ArgumentException("that [id] was already occupied");
                            }
                        }
                        else if (idxChange.Value == null)
                            controlNode[idxChange.Name].UnTie();
                        else
                            controlNode[idxChange.Name].Value = idxChange.Value;
					}
				}
			}

			BuildForm();
			wrp.ReRender();
            RaiseActiveEvent("magix.ide.surface-changed");
        }

        /*
         * selects control
         */
        [ActiveEvent(Name = "magix.ide.select-control")]
		protected void magix_ide_select_control(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.select-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.select-control-sample]");
                return;
			}

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

            if (!ip.ContainsValue("dna"))
				SelectedControlDna = null;
			else
				SelectedControlDna = ip["dna"].Get<string>();

			BuildForm();
			wrp.ReRender();

            Node controlSelectedNode = new Node();
			controlSelectedNode["dna"].Value = SelectedControlDna;
            RaiseActiveEvent(
				"magix.ide.control-selected",
				controlSelectedNode);
		}

        /*
         * returns selected control
         */
        [ActiveEvent(Name = "magix.ide.get-selected-control")]
        protected void magix_ide_get_selected_control(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-selected-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-selected-control-sample]");
                return;
            }

            if (!string.IsNullOrEmpty(SelectedControlDna))
            {
                ip["dna"].Value = DataSource.FindDna(SelectedControlDna).Dna;
                ip["value"].Add(DataSource.FindDna(SelectedControlDna).Clone());
            }
        }

        /*
         * returns selected control
         */
        [ActiveEvent(Name = "magix.ide.get-clipboard-control")]
        protected void magix_ide_get_clipboard_control(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-clipboard-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-clipboard-control-sample]");
                return;
            }

            if (ClipBoard != null)
                ip["control"].Add(ClipBoard.Clone());
        }

        /*
         * copies control into clipboard
         */
		[ActiveEvent(Name = "magix.ide.copy-control")]
		protected void magix_ide_copy_control(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.copy-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.copy-control-sample]");
                return;
			}

			if (SelectedControlDna == null)
				throw new ArgumentException("no control selected");

			ClipBoard = DataSource.FindDna(SelectedControlDna).Clone();
		}

		/*
		 * clears all controls on form
		 */
        [ActiveEvent(Name = "magix.ide.clear-controls")]
        protected void magix_ide_clear_controls(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.clear-controls-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.clear-controls-sample]");
                return;
            }

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

            AddToUndoChain(!ip.ContainsValue("skip-undo") || !ip["skip-undo"].Get<bool>());

            DataSource["controls"].Clear();
            SelectedControlDna = null;
            BuildForm();
            wrp.ReRender();
            RaiseSurfaceAndSelectionChangedChanged();
        }

        /*
         * returns the current form's data
         */
        [ActiveEvent(Name = "magix.ide.get-form")]
        protected void magix_ide_get_form(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-form-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-form-sample]");
                return;
            }

            ip.AddRange(DataSource["controls"].Clone());
        }

        /*
         * sets the current form's data
         */
        [ActiveEvent(Name = "magix.ide.set-form")]
        protected void magix_ide_set_form(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.set-form-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.set-form-sample]");
                return;
            }

            if (!SurfaceEnabled)
                throw new ArgumentException("wysiwyg surface is not enabled");

            Node value = ip["value"];
            Dictionary<string, bool> ids = new Dictionary<string, bool>();
            bool allUnique = true;
            foreach (Node idxControl in value)
            {
                if (CheckControlIdUnique(idxControl, ids) == false)
                {
                    allUnique = false;
                    break;
                }
            }

            if (!allUnique)
            {
                Node msg = new Node();
                msg["message"].Value = "not all controls had unique ids, please fix before surface can be updated";
                RaiseActiveEvent(
                    "magix.viewport.show-message",
                    msg);
                return;
            }

            AddToUndoChain(!ip.ContainsValue("skip-undo") || !ip["skip-undo"].Get<bool>());

            DataSource["controls"].Clear();
            if (ip.Contains("value"))
                DataSource["controls"].AddRange(value.Clone());

            // settting the selected control to null, unless we can find a control again on the surface, at the same dna
            if (SelectedControlDna != null && ((ip.ContainsValue("clear-selection") && ip["clear-selection"].Get<bool>()) ||
                (DataSource.FindDna(SelectedControlDna) == null ||
                (DataSource.FindDna(SelectedControlDna).Parent != null && DataSource.FindDna(SelectedControlDna).Parent.Name != "controls"))))
                SelectedControlDna = null;

            BuildForm();
            wrp.ReRender();

            if (!ip.ContainsValue("no-update") || !ip["no-update"].Get<bool>())
                RaiseSurfaceAndSelectionChangedChanged();
        }

        /*
         * enables or disables the wysiwyg design surface
         */
        [ActiveEvent(Name = "magix.ide.enable-surface")]
        protected void magix_ide_enable_surface(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.enable-surface-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.enable-surface-sample]");
                return;
            }

            if (!ip.ContainsValue("value"))
                throw new ArgumentException("no [value] passed into [magix.ide.enable-surface]");
            SurfaceEnabled = ip["value"].Get<bool>();

            if (!SurfaceEnabled && !wrp.Class.Contains(" wysiwyg-disabled"))
                wrp.Class = wrp.Class + " wysiwyg-disabled";
            else if (SurfaceEnabled && wrp.Class.Contains(" wysiwyg-disabled"))
                wrp.Class = wrp.Class.Replace(" wysiwyg-disabled", "");
        }

        /*
         * retrieves if the surface is enabled or not
         */
        [ActiveEvent(Name = "magix.ide.get-enabled")]
        protected void magix_ide_get_enabled(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-enabled-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.get-enabled-sample]");
                return;
            }

            ip["value"].Value = SurfaceEnabled;
        }

        /*
         * undo the last action on the form
         */
        [ActiveEvent(Name = "magix.ide.undo")]
        protected void magix_ide_undo(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.undo-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.undo-sample]");
                return;
            }

            if (UndoChainIndex <= 0)
            {
                Node msg = new Node();
                msg["message"].Value = "cannot further undo on the surface";
                RaiseActiveEvent(
                    "magix.viewport.show-message",
                    msg);
                return;
            }

            // checking to see if this is first undo, if so, we need to add this into the "redo chain"
            if (UndoChain.Count == UndoChainIndex)
            {
                UndoChain.Add(DataSource.Clone());
                UndoChain[UndoChain.Count - 1]["_selected"].Value = SelectedControlDna;
            }

            DataSource = UndoChain[--UndoChainIndex].Clone();
            SelectedControlDna = DataSource["_selected"].Get<string>();
            DataSource["_selected"].UnTie();

            BuildForm();
            wrp.ReRender();

            if (!ip.ContainsValue("no-update") || !ip["no-update"].Get<bool>())
                RaiseSurfaceAndSelectionChangedChanged();

            RaiseUndoChainIndexChanged();
        }

        /*
         * redo the last action on the form
         */
        [ActiveEvent(Name = "magix.ide.redo")]
        protected void magix_ide_redo(object sender, ActiveEventArgs e)
        {
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.redo-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.redo-sample]");
                return;
            }

            if (UndoChainIndex >= UndoChain.Count)
            {
                Node msg = new Node();
                msg["message"].Value = "cannot further redo on the surface";
                RaiseActiveEvent(
                    "magix.viewport.show-message",
                    msg);
                return;
            }

            DataSource = UndoChain[++UndoChainIndex].Clone();
            SelectedControlDna = DataSource["_selected"].Get<string>();
            DataSource["_selected"].UnTie();

            BuildForm();
            wrp.ReRender();

            if (!ip.ContainsValue("no-update") || !ip["no-update"].Get<bool>())
                RaiseSurfaceAndSelectionChangedChanged();

            RaiseUndoChainIndexChanged();
        }

        #endregion

        #region [ -- active event helper methods -- ]

        /*
         * helper for above, returns one single control, and its children, flat
         */
        private void GetControl(Node controlNodeSource, Node root, int level)
        {
            Node currentControlDestination = new Node();
            currentControlDestination.Name = controlNodeSource.Dna;

            string value = controlNodeSource.Name + ":" + controlNodeSource.Get<string>();
            int idxLevel = level;
            while (idxLevel-- != 0)
                value = "-" + value;
            currentControlDestination.Value = value;

            root.Add(currentControlDestination);

            if (controlNodeSource.Contains("controls"))
            {
                level += 1;
                foreach (Node idxInnerControl in controlNodeSource["controls"])
                {
                    Node innerControlNode = new Node();
                    GetControl(idxInnerControl, root, level);
                }
            }
        }

        /*
         * adds a control to wysiwyg surface
         */
        private void AddControlToSurface(
            Node ip,
            Node controlToAddNode)
        {
            // defaulting to currently selected if exists, otherwise main surface
            string dna = SelectedControlDna ?? DataSource["controls"].Dna;
            string position = "after";

            if (ip.Contains("where"))
            {
                if (ip["where"].ContainsValue("dna"))
                    dna = ip["where"]["dna"].Get<string>();
                if (ip["where"].ContainsValue("position"))
                    position = ip["where"]["position"].Get<string>();
            }

            if (dna == DataSource["controls"].Dna)
            {
                // main surface is destination, making sure position gets correct if user chose "before", and we can do such a thing
                if (position == "before" && DataSource["controls"].Count > 0)
                    dna = DataSource["controls"][0].Dna;
                else
                    position = "child";
            }

            if (!controlToAddNode.Contains("id") &&
                string.IsNullOrEmpty(controlToAddNode.Get<string>()))
                controlToAddNode.Value = GetNextAvailableControlId(controlToAddNode);
            else if (controlToAddNode.Contains("id"))
            {
                if (controlToAddNode.Value != null)
                    throw new ArgumentException("either supply [id] or value, not both");

                // moving id into value for consistency
                controlToAddNode.Value = controlToAddNode["id"].Get<string>();
                controlToAddNode["id"].UnTie();
            }

            AddToUndoChain(!ip.ContainsValue("skip-undo") || !ip["skip-undo"].Get<bool>());

            Node destinationNode = DataSource.FindDna(dna);

            switch (position)
            {
                case "before":
                    destinationNode.AddBefore(controlToAddNode);
                    break;
                case "after":
                    destinationNode.AddAfter(controlToAddNode);
                    break;
                case "child":
                    {
                        if (destinationNode.Dna == DataSource["controls"].Dna)
                            destinationNode.Add(controlToAddNode);
                        else
                        {
                            // verifying control supports having children, otherwise defaulting to 'after'
                            Node inspectParent = new Node();
                            inspectParent["inspect"].Value = null;
                            RaiseActiveEvent(
                                "magix.forms.controls." + destinationNode.Name,
                                inspectParent);

                            if (inspectParent["magix.forms.create-web-part"]["controls"][0].Contains("controls"))
                                destinationNode["controls"].Add(controlToAddNode);
                            else
                            {
                                Node msg = new Node();
                                msg["message"].Value = "control didn't support children, added the control after your selection";
                                msg["color"].Value = "#ffbbbb";
                                RaiseActiveEvent(
                                    "magix.viewport.show-message",
                                    msg);
                                destinationNode.AddAfter(controlToAddNode); // defaulting to after
                            }
                        }
                    } break;
                default:
                    throw new ArgumentException("only before, after or child are legal positions");
            }

            if (ip.Contains("auto-select") && ip["auto-select"].Get<bool>())
                SelectedControlDna = controlToAddNode.Dna;
            BuildForm();
            wrp.ReRender();
            RaiseSurfaceAndSelectionChangedChanged();
        }

        /*
         * returns the next automatic available control id
         */
        private string GetNextAvailableControlId(Node controlNode)
        {
            string ctrlString = controlNode.Name;
            Node getControlsNode = new Node();
            RaiseActiveEvent(
                "magix.ide.list-controls",
                getControlsNode);
            int idxNo = 0;
            string retVal = ctrlString + "-" + idxNo;
            while (true)
            {
                bool found = false;
                foreach (Node idxCtrl in getControlsNode["controls"])
                {
                    if (idxCtrl.Get<string>().Contains(":" + retVal))
                    {
                        found = true;
                        idxNo += 1;
                        retVal = ctrlString + "-" + idxNo;
                        break;
                    }
                }
                if (!found)
                    break;
            }
            return retVal;
        }

        /*
         * raises the surface-changed and the control-selected events
         */
        private void RaiseSurfaceAndSelectionChangedChanged()
        {
            RaiseActiveEvent("magix.ide.surface-changed");

            Node selectedControlChangedNode = new Node();
            if (!string.IsNullOrEmpty(SelectedControlDna))
                selectedControlChangedNode["dna"].Value = SelectedControlDna;
            RaiseActiveEvent(
                "magix.ide.control-selected",
                selectedControlChangedNode);
        }

        /*
         * verifies all controls have unique ids recursively
         */
        private bool CheckControlIdUnique(Node idxControl, Dictionary<string, bool> ids)
        {
            if (ids.ContainsKey(idxControl.Get<string>()))
                return false;
            ids[idxControl.Get<string>()] = true;
            if (idxControl.Contains("controls"))
            {
                foreach (Node idxChild in idxControl["controls"])
                {
                    if (!CheckControlIdUnique(idxChild, ids))
                        return false;
                }
            }
            return true;
        }

        /*
         * raises the undo-chain-index-changed event
         */
        private void RaiseUndoChainIndexChanged()
        {
            Node undoNode = new Node();
            undoNode["can-undo"].Value = UndoChainIndex > 0;
            undoNode["can-redo"].Value = UndoChainIndex < UndoChain.Count - 1;
            RaiseActiveEvent(
                "magix.ide.undo-chain-index-changed",
                undoNode);
        }

        /*
         * adds current form to the stack of undo chain items
         */
        private void AddToUndoChain(bool raiseUndoChainIndexChanged)
        {
            // we don't add this to the undo stack, unless it is either explicitly asked for, or it is the first 
            // update to the form
            if (raiseUndoChainIndexChanged || UndoChain.Count == 0)
            {
                // removes any undo chains ahead of our curent index, in case user has executed undo, before this action
                int idxRemove = UndoChain.Count;
                while (idxRemove > UndoChainIndex)
                    UndoChain[--idxRemove].UnTie();

                // adding current datasource, and selected control to undo chain list
                UndoChain.Add(DataSource.Clone());
                UndoChain[UndoChain.Count - 1]["_selected"].Value = SelectedControlDna;
            }

            if (raiseUndoChainIndexChanged)
            {
                UndoChainIndex = UndoChain.Count;
                RaiseUndoChainIndexChanged();
            }
        }

        #endregion
    }
}
