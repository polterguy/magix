/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
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
    /**
     * Control with support for custom attributes
     */
    public abstract class AttributeControl : BaseWebControl
    {
		/**
		 * One custom attribute
		 */
		public class Attribute
		{
			/**
			 * Name of attribute
			 */
			public string Name
			{
				get;
				set;
			}

			/**
			 * Value of attribute
			 */
			public string Value
			{
				get;
				set;
			}
		}

		// TODO: implement support for changing in ajax callbacks, see how StyleCollection is implemented
		/**
		 * List of custom attributes associated with control
		 */
		List<Attribute> Attributes
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
