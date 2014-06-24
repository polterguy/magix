/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;
using System.Web.UI;

namespace Magix.forms
{
	/*
	 * contains the rating control
	 */
    internal sealed class RatingController : BaseWebControlFormElementTextController
	{
		/*
		 * creates button widget
		 */
		[ActiveEvent(Name = "magix.forms.controls.rating")]
		private void magix_forms_controls_rating(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
				Inspect(ip);
				return;
			}

            Node codeNode = ip["_code"].Get<Node>();
            Panel ret = new Panel();
            ret.Class = "mux-rating";
            FillOutParameters(e.Params, ret);

            Hidden hid = new Hidden();
            hid.ID = ret.ID + "-value";
            ret.Controls.Add(hid);

            int maxValue = 5;
            if (codeNode.Contains("max-value"))
                maxValue = codeNode["max-value"].Get<int>();

            int value = 0;
            if (codeNode.Contains("value"))
                value = codeNode["value"].Get<int>();

            for (int idxNo = 0; idxNo < maxValue; idxNo++)
            {
                int buttonIndex = idxNo + 1;
                LinkButton btn = new LinkButton();
                btn.ID = "star_" + idxNo;
                if (idxNo < value)
                    btn.Class = "rating-checked";
                if (ShouldHandleEvent("onrate", codeNode))
                {
                    if (!e.Params.ContainsValue("id-prefix"))
                    {
                        Node onRateCode = codeNode["onrate"].Clone();
                        btn.Click += delegate(object sender2, EventArgs e2)
                        {
                            SetChecked(ret, buttonIndex);
                            onRateCode["$"]["value"].Value = buttonIndex;
                            hid.Value = buttonIndex.ToString();
                            RaiseActiveEvent(
                                "magix.execute",
                                onRateCode);
                        };
                    }
                }
                else
                {
                    if (!e.Params.ContainsValue("id-prefix"))
                    {
                        btn.Click += delegate(object sender2, EventArgs e2)
                        {
                            hid.Value = buttonIndex.ToString();
                            SetChecked(ret, buttonIndex);
                        };
                    }
                }
                ret.Controls.Add(btn);
            }

            ip["_ctrl"].Value = ret;
		}

        private static void SetChecked(Panel ret, int buttonIndex)
        {
            foreach (Control idxCtrl in ret.Controls)
            {
                LinkButton idxBtn = idxCtrl as LinkButton;
                if (idxBtn != null)
                {
                    if (int.Parse(idxBtn.ID.Substring(5)) < buttonIndex)
                    {
                        if (!idxBtn.Class.Contains(" rating-checked"))
                            idxBtn.Class += " rating-checked";
                    }
                    else
                        if (idxBtn.Class.Contains(" rating-checked"))
                            idxBtn.Class = idxBtn.Class.Replace(" rating-checked", "");
                }
            }
        }

		protected override void Inspect(Node node)
		{
            AppendInspectFromResource(
                node["inspect"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.rating-dox].value");
            AppendCodeFromResource(
                node,
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.rating-sample]");
            base.Inspect(node["magix.forms.create-web-part"]["controls"]["rating"]);
            node["magix.forms.create-web-part"]["controls"]["rating"]["class"].Value = "mux-rating";
            node["magix.forms.create-web-part"]["controls"]["rating"]["value"].Value = "2";
            AppendCodeFromResource(
                node["magix.forms.create-web-part"]["controls"]["rating"],
                "Magix.forms",
                "Magix.forms.hyperlisp.inspect.hl",
                "[magix.forms.rating-sample-end]");
        }
	}
}

