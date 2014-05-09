/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Xml;
using System.Web;
using Magix.Core;

namespace Magix.execute
{
	/**
	 * xml core logic
	 */
	public class XmlCore : ActiveController
	{
		/**
		 * returns [xml] as [dom]
		 */
		[ActiveEvent(Name = "magix.xml.xml-2-node")]
		public static void magix_web_get(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
				e.Params["event:magix.execute"].Value = null;
				e.Params["inspect"].Value = @"will return 
[dom] node structure, parsed from xml in value.&nbsp;&nbsp;thread safe";
				e.Params["magix.xml.xml-2-node"].Value = @"
<xml>
  <value1>howdy</value1>
  <value2>hello</value2>
</xml>";
				return;
			}

            string xml = Ip(e.Params).Get<string>();

			if (string.IsNullOrEmpty(xml))
				throw new ArgumentException("need xml value to [magix.xml.xml-2-node]");

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

            ParseNode(doc.DocumentElement, Ip(e.Params)["dom"]["_tmp"]);
		}

		private static void ParseNode(XmlNode xml, Node node)
		{
			node.Name = xml.Name;

			if (xml.Attributes != null)
			{
				foreach (XmlAttribute idx in xml.Attributes)
				{
					node["@" + idx.Name].Value = idx.Value;
				}
			}

			if (!xml.HasChildNodes)
			{
				if (!string.IsNullOrEmpty(xml.InnerText))
					node.Value = xml.InnerText;
				return;
			}

			if (xml.ChildNodes.Count == 1 && xml.ChildNodes.Item(0).NodeType == XmlNodeType.Text)
			{
				node.Value = xml.ChildNodes.Item(0).InnerText;
			}
			else
			{
				foreach (XmlNode idx in xml.ChildNodes)
				{
					if (idx == null)
						continue;
					ParseNode(idx, node["_tmp"]);
				}
			}
		}
	}
}

