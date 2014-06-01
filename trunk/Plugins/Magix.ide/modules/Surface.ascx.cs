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
			get { return ViewState["ClipBoard"] as Node; }
			set { ViewState["ClipBoard"] = value; }
		}

        /*
         * the currently selected control, if any
         */
		private string SelectedControlDna
		{
            get { return ViewState["SelectedControlDna"] as string; }
            set { ViewState["SelectedControlDna"] = value; }
		}

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

		protected override void OnLoad (EventArgs e)
		{
			BuildForm();
			base.OnLoad(e);
		}

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

			RaiseActiveEvent(
				controlType,
				nodeCreateControl);

            if (!nodeCreateControl.Contains("_ctrl"))
                throw new ArgumentException("control '" + controlType + "' not found");

            if (nodeCreateControl["_ctrl"].Value != null)
            {
                Control ctrlObject = nodeCreateControl["_ctrl"].Value as Control;
                AddSingleControl(nodeControl, parent, ctrlObject);
            }
            else
            {
                foreach (Node idxCtrl in nodeCreateControl["_ctrl"])
                {
                    Control ctrlObject = idxCtrl.Value as Control;
                    AddSingleControl(nodeControl, parent, ctrlObject);
                }
            }
		}

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

		protected void wrp_Click(object sender, EventArgs e)
		{
			SelectedControlDna = null;

			BuildForm();
			wrp.ReRender();

			RaiseActiveEvent("magix.ide.surface-changed");
            RaiseActiveEvent("magix.ide.control-selected");
		}

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
				GetControl(idxControl, ip["controls"]);
            }
		}

		private void GetControl(Node controlNodeSource, Node root)
		{
            Node currentControlDestination = new Node();
			currentControlDestination.Name = controlNodeSource.Dna;
            currentControlDestination.Value = controlNodeSource.Name + ":" + controlNodeSource.Get<string>();
            root.Add(currentControlDestination);

			if (controlNodeSource.Contains("controls"))
			{
				foreach (Node idxInnerControl in controlNodeSource["controls"])
				{
					Node innerControlNode = new Node();
                    GetControl(idxInnerControl, root);
				}
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

			if (!ip.Contains("control"))
				throw new ArgumentException("no [control] given to [magix.ide.add-control]");

			if (ip["control"].Count != 1)
				throw new ArgumentException("you must supply exactly one control to [magix.ide.add-control]");

			string dna 		= SelectedControlDna ?? "root";
            string position = "after";

            if (ip.Contains("where"))
            {
                if (ip["where"].ContainsValue("dna"))
                    dna = ip["where"]["dna"].Get<string>();
                if (ip["where"].ContainsValue("position"))
                    position = ip["where"]["position"].Get<string>();
            }

			Node controlNode = ip["control"][0].Clone();

            if (!controlNode.Contains("id") &&
                string.IsNullOrEmpty(controlNode.Get<string>()))
                controlNode.Value = GetNextAvailableControlId();

            if (controlNode.Contains("id"))
            {
                if (controlNode.Value != null)
                    throw new ArgumentException("either supply [id] or value, not both");
                controlNode.Value = controlNode["id"].Get<string>();
                controlNode["id"].UnTie();
            }

            Node getControlsNode = new Node();
            RaiseActiveEvent(
                "magix.ide.list-controls",
                getControlsNode);
            foreach (Node idxCtrl in getControlsNode["controls"])
            {
                string idOfIdx = idxCtrl.Get<string>().Split(':')[1];
                if (controlNode.Get<string>() == idOfIdx)
                {
                    // id was occupied
                    controlNode.Value = GetNextAvailableControlId();
                }
            }

            // root can only have children, no after or before controls
            if (dna == "root")
            {
                if (position == "before")
                {
                    if (DataSource["controls"].Count > 0)
                        dna = "root-0-0";
                }
                else
                    position = "child";
            }

            Node whereNode = DataSource.FindDna(dna);
            switch (position)
			{
			case "before":
				whereNode.AddBefore(controlNode);
				break;
			case "after":
				whereNode.AddAfter(controlNode);
				break;
			case "child":
			{
				if (whereNode.Name == "surface")
					whereNode["controls"].Add(controlNode);
				else
				{
					Node inspectParent = new Node();
					inspectParent["inspect"].Value = null;

					RaiseActiveEvent(
						"magix.forms.controls." + whereNode.Name,
						inspectParent);

                    if (inspectParent["magix.forms.create-web-part"]["controls"][0].Contains("controls"))
                        whereNode["controls"].Add(controlNode);
                    else
                    {
                        Node msg = new Node();
                        msg["message"].Value = "control didn't support children, added the control after your selection";
                        msg["color"].Value = "#ffbbbb";
                        RaiseActiveEvent(
                            "magix.viewport.show-message",
                            msg);
                        whereNode.AddAfter(controlNode); // defaulting to after
                    }
				}
			} break;
			default:
				throw new ArgumentException("only before, after or child are legal positions");
			}

			if (ip.Contains("auto-select") && ip["auto-select"].Get<bool>())
			{
				SelectedControlDna = controlNode.Dna;
				BuildForm();
				wrp.ReRender();
				RaiseActiveEvent("magix.ide.surface-changed");

				Node selectedControlChangedNode = new Node();
				selectedControlChangedNode["dna"].Value = SelectedControlDna;
				RaiseActiveEvent(
					"magix.ide.control-selected",
					selectedControlChangedNode);
			}
			else
			{
				BuildForm();
				wrp.ReRender();
				RaiseActiveEvent("magix.ide.surface-changed");
			}
		}

        /*
         * returns the next automatic available control id
         */
        private string GetNextAvailableControlId()
        {
            Node getControlsNode = new Node();
            RaiseActiveEvent(
                "magix.ide.list-controls",
                getControlsNode);
            int idxNo = getControlsNode["controls"].Count;
            string retVal = "ctrl" + idxNo;
            while (true)
            {
                bool found = false;
                foreach (Node idxCtrl in getControlsNode["controls"])
                {
                    if (idxCtrl.Get<string>().Contains(":" + retVal))
                    {
                        found = true;
                        idxNo += 1;
                        retVal = "ctrl" + idxNo;
                        break;
                    }
                }
                if (!found)
                    break;
            }
            return retVal;
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

			if (!ip.ContainsValue("dna"))
				throw new ArgumentException("no [dna] given to [magix.ide.remove-control]");
			string dna = ip["dna"].Get<string>();

			Node whereNode = DataSource.FindDna(dna);
            whereNode.UnTie();
			BuildForm();
			wrp.ReRender();
			RaiseActiveEvent("magix.ide.surface-changed");
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

			if (!ip.Contains("change") && !ip.Contains("remove"))
				throw new ArgumentException("no [change] or [remove] given to [magix.ide.change-control]");

            if (!ip.ContainsValue("dna"))
                throw new ArgumentException("no [dna] given to [magix.ide.change-control]");

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
                        else if (string.IsNullOrEmpty(idxChange.Get<string>()))
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
		 * moves a control to another position
		 */
		[ActiveEvent(Name = "magix.ide.move-control")]
		protected void magix_ide_move_control(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.move-control-dox].Value");
                AppendCodeFromResource(
                    ip,
                    "Magix.ide",
                    "Magix.ide.hyperlisp.inspect.hl",
                    "[magix.ide.move-control-sample]");
                return;
			}

			if (!ip.ContainsValue("dna"))
				throw new ArgumentException("no [dna] given to [magix.ide.move-control]");

			if (!ip.ContainsValue("to"))
				throw new ArgumentException("no [to] given to [magix.ide.move-control]");

			if (!ip.ContainsValue("position"))
				throw new ArgumentException("no [position] given to [magix.ide.move-control]");

			Node controlToMoveNode = DataSource.FindDna(ip["dna"].Get<string>());
            Node destinationControlNode = DataSource.FindDna(ip["to"].Get<string>());
            if (controlToMoveNode.Dna.IndexOf(destinationControlNode.Dna) != -1)
                throw new ArgumentException("you cannot move a control into one of its own descendants");
            controlToMoveNode.UnTie();
			switch (ip["position"].Get<string>())
			{
			case "before":
				destinationControlNode.AddBefore(controlToMoveNode);
				break;
			case "after":
                destinationControlNode.AddAfter(controlToMoveNode);
				break;
			case "child":
                destinationControlNode.Add(controlToMoveNode);
				break;
			default:
				throw new ArgumentException("only before, after or child are legal values as [position]");
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
            DataSource = new Node("surface");
            SelectedControlDna = null;
            BuildForm();
            wrp.ReRender();
            RaiseActiveEvent("magix.ide.surface-changed");
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

			if (ClipBoard == null)
				throw new ArgumentException("no control in clipboard");

			Node destinationNode = DataSource["controls"];
			if (!string.IsNullOrEmpty(SelectedControlDna))
				destinationNode = DataSource.FindDna(SelectedControlDna);

			string position = "child";
			if (ip.ContainsValue("position"))
				position = ip["position"].Get<string>();

			Node toAdd = ClipBoard.Clone();
            toAdd.Value = GetNextAvailableControlId();

			switch (position)
			{
			    case "before":
				    destinationNode.AddBefore(toAdd);
				    break;
			    case "after":
				    destinationNode.AddAfter(toAdd);
				    break;
			    case "child":
			    {
                    if (string.IsNullOrEmpty(SelectedControlDna))
					    destinationNode.Add(toAdd);
				    else
				    {
					    Node inspectControl = new Node();
					    inspectControl["inspect"].Value = null;

					    RaiseActiveEvent(
						    "magix.forms.controls." + destinationNode.Get<string>(),
						    inspectControl);

					    if (inspectControl["magix.forms.create-web-part"]["controls"][0].Contains("controls"))
						    destinationNode["controls"].Add(toAdd);
					    else
						    destinationNode.AddAfter(toAdd); // defaulting to add 'after'
				    }
			    } break;
			    default:
				    throw new ArgumentException("only before, after or child are legal values");
			}

			if (ip.Contains("auto-select") && ip["auto-select"].Get<bool>())
			{
				SelectedControlDna = toAdd.Dna;
				BuildForm();
				wrp.ReRender();
				RaiseActiveEvent("magix.ide.surface-changed");

				Node selectedControlChangedNode = new Node();
				selectedControlChangedNode["dna"].Value = SelectedControlDna;
				RaiseActiveEvent(
					"magix.ide.control-selected",
					selectedControlChangedNode);
			}
			else
			{
				BuildForm();
				wrp.ReRender();
				RaiseActiveEvent("magix.ide.surface-changed");
			}
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

            ip.Clear();
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

            SelectedControlDna = null;
            DataSource["controls"].Clear();
            if (ip.Contains("value"))
                DataSource["controls"].AddRange(value.Clone());

            BuildForm();
            wrp.ReRender();
            RaiseActiveEvent("magix.ide.surface-changed");
        }

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
    }
}

