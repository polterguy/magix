/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.Xml;
using System.Web;
using Magix.Core;

namespace Magix.execute
{
	/*
	 * xml core logic
	 */
	public class XmlCore : ActiveController
	{
		/*
		 * returns [xml] as [dom]
		 */
		[ActiveEvent(Name = "magix.xml.xml-2-node")]
		public static void magix_xml_xml_2_node(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
            if (ShouldInspect(ip))
            {
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.xml",
                    "Magix.xml.hyperlisp.inspect.hl",
                    "[magix.xml.xml-2-node-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.xml",
                    "Magix.xml.hyperlisp.inspect.hl",
                    "[magix.xml.xml-2-node-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.ContainsValue("xml") && !ip.ContainsValue("file"))
                throw new ArgumentException("no [xml] or [file] value given to [magix.xml.xml-2-node]");
            if (ip.ContainsValue("xml") && ip.ContainsValue("file"))
                throw new ArgumentException("you cannot give both [file] and [xml] to [magix.xml.xml-2-node]");

            string xml = null;
            if (ip.ContainsValue("xml"))
                xml = Expressions.GetExpressionValue<string>(ip["xml"].Get<string>(), dp, ip, false);
            else
            {
                try
                {
                    RaiseActiveEvent(
                        "magix.file.load",
                        e.Params);

                    xml = ip["value"].Get<string>();
                }
                finally
                {
                    ip["value"].UnTie();
                }
            }

			XmlDocument doc = new XmlDocument();
			doc.LoadXml(xml);

            ParseNode(doc.DocumentElement, ip["dom"]["_tmp"]);
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

