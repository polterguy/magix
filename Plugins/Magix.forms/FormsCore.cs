/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using Magix.Core;

namespace Magix.forms
{
	/*
	 * controller for creating web parts, either as control collections, or as ml
	 */
    internal sealed class FormsCore : ActiveController
	{
		/*
		 * will create a web part based upon a web control hierarchy
		 */
		[ActiveEvent(Name = "magix.forms.create-web-part")]
		private void magix_forms_create_web_part(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"], 
                    "Magix.forms", 
                    "Magix.forms.hyperlisp.inspect.hl", 
                    "[magix.forms.create-web-part-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.create-web-part-sample]");
                return;
			}

            Node dp = Dp(e.Params);

            if (!ip.Contains("controls") && !ip.ContainsValue("controls-file") && !ip.ContainsValue("controls-id"))
                throw new ArgumentException("[magix.forms.create-web-part] needs either a [controls-file], a [controls] or a [controls-id] parameter");

            if ((ip.Contains("controls") && (ip.Contains("controls-file") || ip.Contains("controls-id"))) || 
                (ip.Contains("controls-file") && (ip.Contains("controls-id"))))
                throw new ArgumentException("only one of [controls-file], [controls] or [controls-id] are legal parameters to [magix.forms.create-web-part]");

            if (ip.Contains("events") && ip.Contains("events-file"))
                throw new ArgumentException("either supply [events-file] or [events] to [magix.forms.create-web-part], not both of them");

            bool hasControlsFile = false;
            if (ip.Contains("controls-file"))
            {
                ip["controls-file"].Name = "file";
                try
                {
                    RaiseActiveEvent(
                        "magix.file.load",
                        e.Params);
                }
                finally
                {
                    ip["file"].Name = "controls-file";
                }

                Node toNode = new Node();
                toNode["code"].Value = ip["value"].Get<string>();
                ip["value"].UnTie();
                RaiseActiveEvent(
                    "magix.execute.code-2-node",
                    toNode);
                toNode["node"]["inspect"].UnTie();

                ip["controls"].Clear();
                ip["controls"].AddRange(toNode["node"]);

                hasControlsFile = true;
            }

            bool hasControlsId = false;
            if (ip.Contains("controls-id"))
            {
                Node fromData = new Node();
                fromData["id"].Value = Expressions.GetExpressionValue<string>(ip["controls-id"].Get<string>(), dp, ip, false);
                RaiseActiveEvent(
                    "magix.data.load",
                    fromData);
                if (!fromData.Contains("value"))
                    throw new ArgumentException("couldn't find data object with id of; '" + fromData["id"].Get<string>() + "'");
                if (fromData["value"]["type"].Get<string>() != "magix.forms.web-part")
                    throw new ArgumentException("database object was not a web-part");
                ip["controls"].Clear();
                ip["controls"].AddRange(fromData["value"]["surface"]);

                hasControlsId = true;
            }

            bool hasEventsFile = false;
            if (ip.Contains("events-file"))
            {
                ip["events-file"].Name = "file";
                try
                {
                    RaiseActiveEvent(
                        "magix.file.load",
                        e.Params);
                }
                finally
                {
                    ip["file"].Name = "events-file";
                }

                Node toNode = new Node();
                toNode["code"].Value = ip["value"].Get<string>();
                ip["value"].UnTie();
                RaiseActiveEvent(
                    "magix.execute.code-2-node",
                    toNode);

                ip["events"].Clear();
                ip["events"].AddRange(toNode["node"]);
                hasEventsFile = true;
            }

            string container = null;
            if (ip.Contains("container"))
                container = Expressions.GetExpressionValue<string>(ip["container"].Get<string>(), dp, ip, false);

            LoadActiveModule(
				"Magix.forms.WebPart",
                container,
                e.Params);

            ip["name"].UnTie();

            if (hasControlsFile || hasControlsId)
                ip["controls"].UnTie();

            if (hasEventsFile)
                ip["events"].UnTie();
		}
		
		/*
		 * will create a magix markup language web part
		 */
		[ActiveEvent(Name = "magix.forms.create-mml-web-part")]
		private void magix_forms_create_mml_web_part(object sender, ActiveEventArgs e)
		{
            Node ip = Ip(e.Params);
			if (ShouldInspect(ip))
			{
                AppendInspectFromResource(
                    ip["inspect"],
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.create-mml-web-part-dox].value");
                AppendCodeFromResource(
                    ip,
                    "Magix.forms",
                    "Magix.forms.hyperlisp.inspect.hl",
                    "[magix.forms.create-mml-web-part-sample]");
				return;
			}

            Node dp = Dp(e.Params);

            if (!ip.Contains("mml-file") && !ip.Contains("mml"))
                throw new ArgumentException("create-mml-web-part requires either an [mml-file] parameter or an [mml] parameter");

            if (ip.Contains("mml-file") && ip.Contains("mml"))
                throw new ArgumentException("create-mml-web-part requires either an [mml-file] parameter or an [mml] parameter, not both");

            bool hasMmlFile = false;
            if (ip.Contains("mml-file"))
            {
                ip["mml-file"].Name = "file";
                try
                {
                    RaiseActiveEvent(
                        "magix.file.load",
                        e.Params);
                }
                finally
                {
                    ip["file"].Name = "mml-file";
                }

                ip["mml"].Value = ip["value"].Get<string>();
                ip["value"].UnTie();
                hasMmlFile = true;
            }

            bool hasEventsFile = false;
            if (ip.Contains("events-file"))
            {
                ip["events-file"].Name = "file";
                try
                {
                    RaiseActiveEvent(
                        "magix.file.load",
                        e.Params);
                }
                finally
                {
                    ip["file"].Name = "events-file";
                }

                string mml = ip["mml"].Get<string>();
                mml += "\r\n{{\r\n// [ dynamically added events from events-file ]\r\n";
                mml += ip["value"].Get<string>() + "\r\n}}";
                hasEventsFile = true;
                ip["mml"].Value = mml;
                ip["value"].UnTie();
            }

            bool hasEventsNode = false;
            if (ip.Contains("events"))
            {
                Node eventsNodes = new Node();
                eventsNodes["node"].AddRange(ip["events"]);
                RaiseActiveEvent(
                    "magix.execute.node-2-code",
                    eventsNodes);

                string mml = ip["mml"].Get<string>();
                mml += "\r\n{{\r\n// [ dynamically added events from events node ]\r\n";
                mml += eventsNodes["code"].Get<string>() + "\r\n}}";
                hasEventsNode = true;
                ip["mml"].Value = mml;
            }

            string container = null;
            if (ip.Contains("container"))
                container = Expressions.GetExpressionValue<string>(ip["container"].Get<string>(), dp, ip, false);

            LoadActiveModule(
				"Magix.forms.WebPart", 
				container, 
				e.Params);

            if (hasMmlFile)
                ip["mml"].UnTie();
            else if (hasEventsFile)
            {
                string mml = ip["mml"].Get<string>();
                mml = mml.Substring(0, mml.IndexOf("\r\n{{\r\n// [ dynamically added events from events-file ]"));
                ip["mml"].Value = mml;
            }
            else if (hasEventsNode)
            {
                string mml = ip["mml"].Get<string>();
                mml = mml.Substring(0, mml.IndexOf("\r\n{{\r\n// [ dynamically added events from events node ]"));
                ip["mml"].Value = mml;
            }
        }
	}
}
