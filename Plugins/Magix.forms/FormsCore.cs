/*
 * Magix - A Web Application Framework for Humans
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
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
	public class FormsCore : ActiveController
	{
		/*
		 * will create a web part based upon a web control hierarchy
		 */
		[ActiveEvent(Name = "magix.forms.create-web-part")]
		public void magix_forms_create_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>creates a web part and puts 
it into the [container] viewport container</p><p>[form-id] must be a unique id.
&nbsp;&nbsp;[controls] contains the controls themselves, and is a list of the 
controls the form contains.&nbsp;&nbsp;[class] is the css class(es) you wish to 
use for your form</p><p>you can also associate temporary active events with your 
form, which will only exist as long as your form exists.&nbsp;&nbsp;you do this 
by adding active events directly beneath the [events] node, where the name of the 
active event becomes the first node, and the code to execute is directly beneath 
the active event declaration</p><p>all parameters can be either constants or 
expressions.&nbsp;&nbsp;if you use expressions for the [events] and [controls] 
nodes, then you add the expression as the value of the [event] and/or the 
[controls] node, and whatever node these expressions returns, will become what 
the form uses to load its controls/events</p><p>you can optionally supply a 
[controls-file] node, instead of a [controls] node, which will be assumed to 
be a file containing the declaration of the web controls.&nbsp;&nbsp;in addition, 
you can also optionally supply a [events-file] node, which will be assumed to be 
a file containing the declaration of the events associated with the web part.&nbsp;
&nbsp;both [controls-file] and [events-file] can be either constants or expressions.
&nbsp;&nbsp;you cannot supply both [controls] and [controls-file].&nbsp;&nbsp;you 
cannot supply both [events] and [events-file]</p><p>both [events-file] and [controls-file] 
are useful for separating your code from your ui as an alternative to hardcoding in the 
controls and events together in the same piece of code.&nbsp;&nbsp;in addition, you can 
also separate code and ui by supplying the [controls] and [events] as expressions to other 
parts of your execution node tree</p><p>not thread safe</p>";
                e.Params["magix.forms.create-web-part"]["container"].Value = "content3";
                e.Params["magix.forms.create-web-part"]["class"].Value = "css class(es) of your form";
                e.Params["magix.forms.create-web-part"]["form-id"].Value = "unique-id";
                e.Params["magix.forms.create-web-part"]["events"]["magix.forms.control-clicked"]["magix.viewport.show-message"]["message"].Value = "yo";
                e.Params["magix.forms.create-web-part"]["controls"]["button"].Value = "btn";
                e.Params["magix.forms.create-web-part"]["controls"]["button"]["value"].Value = "click me";
                e.Params["magix.forms.create-web-part"]["controls"]["button"]["onclick"]["magix.forms.control-clicked"].Value = null;
                return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

            if (!ip.Contains("controls") && !ip.Contains("controls-file"))
                throw new ArgumentException("create-web-part needs either a [controls-file] parameter or a [controls] parameter");

            if (ip.Contains("controls") && ip.Contains("controls-file"))
                throw new ArgumentException("either supply [controls-file] or [controls] to create-web-part, not both of them");

            if (ip.Contains("events") && ip.Contains("events-file"))
                throw new ArgumentException("either supply [events-file] or [events] to create-web-part, not both of them");

            bool hasControlsFile = false;
            if (ip.Contains("controls-file"))
            {
                string file = Expressions.GetExpressionValue(ip["controls-file"].Get<string>(), dp, ip, false) as string;
                Node loadControls = new Node("magix.file.load", null);
                loadControls["file"].Value = file;
                RaiseActiveEvent(
                    "magix.file.load",
                    loadControls);

                Node toNode = new Node();
                toNode["code"].Value = loadControls["value"].Get<string>();
                RaiseActiveEvent(
                    "magix.execute.code-2-node",
                    toNode);

                ip["controls"].Clear();
                ip["controls"].AddRange(toNode["node"]);

                hasControlsFile = true;
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
                container = Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string;

			LoadActiveModule(
				"Magix.forms.WebPart",
                container,
                e.Params);

            if (hasControlsFile)
                ip["controls"].UnTie();

            if (hasEventsFile)
                ip["events"].UnTie();
		}
		
		/*
		 * will create a magix markup language web part
		 */
		[ActiveEvent(Name = "magix.forms.create-mml-web-part")]
		public void magix_forms_create_mml_web_part(object sender, ActiveEventArgs e)
		{
			if (ShouldInspect(e.Params))
			{
                e.Params["inspect"].Value = @"<p>creates a dynamic magix markup language 
web part from an [mml-file] or [mml] value, putting it into the [container] viewport container
</p><p>[form-id] must be a unique id.&nbsp;&nbsp;you can intermix webcontrols into your mml 
by creating a control collection, by typing them out inside of brackets such as {{...controls, 
using hyper lisp syntax, code goes into event handlers here too ...}}</p><p>internally it uses 
LoadActiveModule, hence all the parameters that goes into your [magix.viewport.load-module] 
active event, can also be passed into this, such as [class], and so on</p><p>the magix markup 
language can either be hardcoded in through the [mml] node, or exist on a file, which you 
de-reference through the [mml-file] node</p><p>you can also optionally supply an [events-file], 
which will be threated as a file that contains the active events for your magix markup language 
web part.&nbsp;&nbsp;in addition, you can also supply an [event] node, which will be threated 
as the active events for the mml web part, but active events can also be inlined directly in the 
magix markup, as long as they have at least one period '.' in their name</p><p>either supply 
[mml-file] or [mml], do not supply both.&nbsp;&nbsp;though you can supply both inline active 
event, [events-file] and an [events] node, and these will be mixed together to form the active 
events of your magix markup language web part</p><p>both [mml], [mml-file], [events-file], 
[container], [class] and [form-id], can be either constants or expressions</p><p>not thread 
safe</p>";
                e.Params["magix.forms.create-mml-web-part"]["container"].Value = "content3";
                e.Params["magix.forms.create-mml-web-part"]["form-id"].Value = "unique-id";
                e.Params["magix.forms.create-mml-web-part"]["class"].Value = "span-22 clear";
                e.Params["magix.forms.create-mml-web-part"]["mml"].Value = @"
&lt;p&gt;notice how you can combine html with {{
magix.test.hello-world
  magix.viewport.show-message
    message=>hello world
link-button=>btn-hello
  value=>web controls
  onclick
    magix.test.hello-world
}} and temporary active events, that 
exists only as long as the web part exists&lt;/p&gt;";
				return;
			}

            Node ip = Ip(e.Params);
            Node dp = ip;
            if (e.Params.Contains("_dp"))
                dp = e.Params["_dp"].Get<Node>();

			if (!ip.Contains("container"))
				throw new ArgumentException("create-mml-web-part needs a [container] parameter");

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

            LoadActiveModule(
				"Magix.forms.WebPart", 
				Expressions.GetExpressionValue(ip["container"].Get<string>(), dp, ip, false) as string, 
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
