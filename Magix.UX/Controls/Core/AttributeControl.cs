/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Web.UI;
using System.ComponentModel;
using System.Collections.Generic;
using Magix.UX.Core;
using Magix.UX.Builder;
using Magix.UX.Effects;
using Magix.UX.Helpers;

namespace Magix.UX.Widgets.Core
{
    /*
     * control with attributes
     */
    public abstract class AttributeControl : BaseWebControl
    {
		/*
		 * attribute wrapper
		 */
		[Serializable]
		public class Attribute
		{
			public Attribute ()
			{ }

			public Attribute (string name, string value)
			{
				Name = name;
				Value = value;
			}

			/*
			 * name
			 */
			public string Name
			{
				get;
				set;
			}

			/*
			 * value
			 */
			public string Value
			{
				get;
				set;
			}
		}

		/*
		 * attributes
		 */
		public List<Attribute> Attributes
		{
			get
			{
				if (ViewState["Attributes"] == null)
					ViewState["Attributes"] = new List<Attribute>();
				return ViewState["Attributes"] as List<Attribute>;
			}
		}

        protected override void AddAttributes(Element el)
        {
            base.AddAttributes(el);
			foreach (Attribute idx in Attributes)
			{
				el.AddAttribute(idx.Name, idx.Value);
			}
        }
    }
}
