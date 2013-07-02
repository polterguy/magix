/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web;
using System.Text;
using System.Web.UI;
using System.Reflection;
using System.Globalization;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Magix.UX.Core;
using Magix.UX.Widgets;
using Magix.UX.Widgets.Core;

namespace Magix.UX
{
    public sealed class AjaxManager
    {
        private List<BaseControl> _raControls = new List<BaseControl>();
        private List<string> _scriptIncludes = new List<string>();
        private List<string> _dynamicScriptIncludes = new List<string>();
        private string _redirectUrl;
        private MemoryStream _memStream;
        private HtmlTextWriter _writer;
        private MemoryStream _memStreamBack;
        private HtmlTextWriter _writerBack;
        private string _requestViewState;

        /**
         * Public accessor, only way to access instance of AjaxManager. The AjaxManager is
         * the "glue" that ties everything together. Use this property to retrieve the only
         * existing object of type AjaxManager.
         */
        public static AjaxManager Instance
        {
            get
            {
                if (((Page)HttpContext.Current.CurrentHandler).Items["_AjaxManagerInstance"] == null)
                    ((Page)HttpContext.Current.CurrentHandler).Items["_AjaxManagerInstance"] = new AjaxManager();
                return (AjaxManager)((Page)HttpContext.Current.CurrentHandler).Items["_AjaxManagerInstance"];
            }
        }

        private List<BaseControl> RaControls
        {
            get { return _raControls; }
        }

        /**
         * Returns true if this is a Ra-Ajax callback. A Ra-Ajax callback is also a "subset" of
         * a normal Postback, which means that if this property is true, then the IsPostBack from 
         * ASP.NET's System.Web.UI.Page will also be true.
         */
        public bool IsCallback
        {
            get { return HttpContext.Current.Request.Params["__RA_CALLBACK"] == "true" && HttpContext.Current.Request.Params["HTTP_X_MICROSOFTAJAX"] == null; }
        }

        internal HtmlTextWriter Writer
        {
            get
            {
                if (_writer == null)
                {
                    _memStream = new MemoryStream();
                    TextWriter tw = new StreamWriter(_memStream);
                    _writer = new HtmlTextWriter(tw);
                }
                return _writer;
            }
        }

        /**
         * Use this method to append you own script and/or HTML or other output which will be executed 
         * on the client when the request returns. Notice though that the JavaScript Ajax engine of
         * Ra-Ajax will expect whatever is being returned to the client to be JavaScript. This means that
         * if you want to return other types of values and objects back to the client you need to wrap this
         * somehow inside of JavaScript by using e.g. JSON or some similar mechanism. Also you should always
         * end your JavaScript statements with semicolon (;) since otherwise other parts of the JavaScript
         * returned probably will fissle.
         */
        public HtmlTextWriter WriterAtBack
        {
            get
            {
                if (_writerBack == null)
                {
                    _memStreamBack = new MemoryStream();
                    TextWriter tw = new StreamWriter(_memStreamBack);
                    _writerBack = new HtmlTextWriter(tw);
                }
                return _writerBack;
            }
        }

        internal void InitializeControl(BaseControl ctrl)
        {
            // We store all Ra Controls in a list for easily access later down the road...
            RaControls.Add(ctrl);

            // Making sure we only run the initialization logic ONCE...!!
            if (RaControls.Count == 1)
            {
                if (((Page)HttpContext.Current.CurrentHandler).Request.Params["HTTP_X_MICROSOFTAJAX"] != null)
                    return;

                if (IsCallback)
                {
                    // This is a Ra-Ajax callback, we need to wait until the Page Load 
                    // events are finished loading and then find the control which
                    // wants to fire an event and do so...
                    ((Page)HttpContext.Current.CurrentHandler).LoadComplete += CurrentPage_LoadComplete;

                    // Checking to see if the Filtering logic has been supressed
                    ((Page)HttpContext.Current.CurrentHandler).Response.Filter = new CallbackFilter(((Page)HttpContext.Current.CurrentHandler).Response.Filter);
                }
                else
                {
                    ((Page)HttpContext.Current.CurrentHandler).LoadComplete += CurrentPage_LoadComplete_NO_AJAX;

                    // Checking to see if the Filtering logic has been supressed
                    ((Page)HttpContext.Current.CurrentHandler).Response.Filter = new PostbackFilter(((Page)HttpContext.Current.CurrentHandler).Response.Filter);
                }
            }
        }

        void CurrentPage_LoadComplete_NO_AJAX(object sender, EventArgs e)
        {
            string browser = 
                ((Page) HttpContext.Current.CurrentHandler).Request.Browser.Browser.ToLower();
            if (browser == "firefox" || 
                browser == "iceweasel" ||
                browser == "netscape" ||
                browser == "applewebkit" ||
                browser.Contains("mozilla"))
                ((Page)HttpContext.Current.CurrentHandler).Form.Attributes.Add("autocomplete", "off");
        }

        private void CurrentPage_LoadComplete(object sender, EventArgs e)
        {
            // Storing request viewstate in order to be able to "diff" 
            // them in rendering of response
            _requestViewState = ((Page)HttpContext.Current.CurrentHandler).Request.Params["__VIEWSTATE"];

            // Finding the Control which initiated the request
            string idOfControl = ((Page)HttpContext.Current.CurrentHandler).Request.Params["__MUX_CONTROL_CALLBACK"];

            // Setting Response Content-Type to JavaScript
            ((Page)HttpContext.Current.CurrentHandler).Response.ContentType = "text/javascript";

            // Checking to see if this is a "non-Control" callback...
            if (string.IsNullOrEmpty(idOfControl))
            {
                string functionName = ((Page)HttpContext.Current.CurrentHandler).Request.Params["__FUNCTION_NAME"];
                
                if (string.IsNullOrEmpty(functionName))
                    return;

                Control ctrlToCallFor = ((Page)HttpContext.Current.CurrentHandler);
                MethodInfo webMethod = ExtractMethod(functionName, ref ctrlToCallFor);

                if (webMethod == null || webMethod.GetCustomAttributes(typeof(Magix.UX.Core.WebMethod), false).Length == 0)
                    throw new Exception("Cannot call a method without a WebMethod attribute");

                ParameterInfo[] parameters = webMethod.GetParameters();

                object[] args = new object[parameters.Length];
                for (int idx = 0; idx < parameters.Length && ((Page)HttpContext.Current.CurrentHandler).Request.Params["__ARG" + idx] != null; idx++)
                {
                    args[idx] = Convert.ChangeType(((Page)HttpContext.Current.CurrentHandler).Request.Params["__ARG" + idx], parameters[idx].ParameterType);
                }

                object retVal = webMethod.Invoke(ctrlToCallFor, args);
                
                if (retVal != null)
                    WriterAtBack.Write("MUX.Control._methodReturnValue='{0}';", 
                        string.Format(CultureInfo.InvariantCulture, "{0}", retVal).Replace("\\", "\\\\").Replace("'", "\\'"));
                return;
            }

            BaseControl ctrl = RaControls.Find(
                delegate(BaseControl idx)
                {
                    return idx.ClientID == idOfControl;
                });

            if (ctrl == null)
                throw new ApplicationException("There seems to be something wrong with the way you [re-] load controls in your page. The control that raised this event, doesn't exist, which means it was never re-created on the server on the second request ...");

            // Getting the name of the event the control raised
            string eventName = ((Page)HttpContext.Current.CurrentHandler).Request.Params["__MUX_EVENT"];

            // Dispatching the event to our Ra Control...
            ctrl.RaiseEvent(eventName);
        }

        private MethodInfo ExtractMethod(string functionName, ref Control ctrlToCallFor)
        {
            MethodInfo webMethod = null;
            if (functionName.IndexOf(".") == -1)
            {
                // No UserControls in the picture here...
                webMethod = ((Page)HttpContext.Current.CurrentHandler).GetType().BaseType.GetMethod(functionName,
                    BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (webMethod != null)
                {
                    ctrlToCallFor = ((Page)HttpContext.Current.CurrentHandler);
                }
                else
                {
                    // Couldn't find method in Page, looking in MasterPage
                    webMethod = ((Page)HttpContext.Current.CurrentHandler).Master.GetType().BaseType.GetMethod(functionName,
                        BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                    ctrlToCallFor = ((Page)HttpContext.Current.CurrentHandler).Master;
                }
            }
            else
            {
                // A "." means there's a UserControl hosting this method
                string[] entities = functionName.Split('.');
                Control ctrl = Selector.SelectFirst<UserControl>(
                    ((Page)HttpContext.Current.CurrentHandler),
                    delegate(Control idx)
                    {
                        return entities[0] == idx.ClientID;
                    });
                webMethod = 
                    ctrl.GetType().BaseType.GetMethod(
                        entities[entities.Length - 1],
                        BindingFlags.Instance | 
                        BindingFlags.Public | 
                        BindingFlags.NonPublic);
                ctrlToCallFor = ctrl;
            }
            return webMethod;
        }

        internal void IncludeMainRaScript()
        {
			IncludeScriptFromResource("Magix.js");
		}

        internal void IncludeMainControlScripts()
        {
			IncludeScriptFromResource("Control.js");
        }

		internal void IncludeScriptFromResource(string script)
		{
            string resource = 
                ((Page)HttpContext.Current.CurrentHandler)
                    .ClientScript.GetWebResourceUrl(typeof(AjaxManager), "Magix.UX.Js." + script);

            if (IsCallback)
            {
                if (_dynamicScriptIncludes.Exists(
                    delegate(string idx)
                        {
                            return idx == resource;
                        }))
                    return;
                _dynamicScriptIncludes.Add(resource);
            }
            else
            {
                if (_scriptIncludes.Exists(
                    delegate(string idx)
                    {
                        return idx == resource;
                    }))
                    return;
                _scriptIncludes.Add(resource);
            }
		}

        /**
         * Includes a JavaScript file from a resource with the given resource id. This one 
         * is mostly for Control developers to make sure your script files are being included.
         * Note that Ra-Ajax can include ANY JavaScrip file even in Ajax Callbacks. But in order for
         * this to work you must use the methods for JavaScript file inclusions in the
         * AjaxManager like for instance this method.
         */
        public void IncludeScriptFromResource(Type type, string id)
        {
            IncludeScriptFromResource(type, id, true);
        }

        public void IncludeScriptFromResource(Type type, string id, bool atEnd)
        {
            string resource =
                ((Page)HttpContext.Current.CurrentHandler)
                    .ClientScript.GetWebResourceUrl(type, id);

            if (!string.IsNullOrEmpty(resource))
                IncludeScript((!atEnd ? "<" : "") + resource);
        }

        /**
         * Includes a JavaScript file from a file path. Note that Ra-Ajax can 
         * include ANY JavaScrip file even in Ajax Callbacks. But in order for
         * this to work you must use the methods for JavaScript file inclusions in the
         * AjaxManager like for instance this method.
         */
        public void IncludeScriptFromFile(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            IncludeScript(path);
        }

        private void IncludeScript(string path)
        {
            if (IsCallback)
            {
                if (!_dynamicScriptIncludes.Contains(path))
                {
                    _dynamicScriptIncludes.Add(path);
                }
            }
            else
            {
                if (!_scriptIncludes.Contains(path))
                {
                    _scriptIncludes.Add(path);
                }
            }
        }

        /**
         * Use to redirect to another page from a Ra-Ajax Callback. Note the default ASP.NET implementation
         * that redirects on Response.Redirect does NOT work in Ra-Ajax callbacks. This method might be used
         * instead of the default Response.Redirect method though.
         */
        public void Redirect(string url)
        {
            if (!IsCallback)
                ((Page)HttpContext.Current.CurrentHandler).Response.Redirect(url);
            else
            {
                ((Page)HttpContext.Current.CurrentHandler).Response.AddHeader(
                    "Location", 
                    ((Page)HttpContext.Current.CurrentHandler).Response.ApplyAppPathModifier(url));

                // Note that due to w3c standardizing the XHR should TRANSPARENTLY
                // do 301 and 302 redirects we need another mechanism to inform client side that
                // the user code is RE-directing to another page!
                ((Page)HttpContext.Current.CurrentHandler).Response.StatusCode = 222;
                _redirectUrl = url;
            }
        }

        // Here we are if this is a POSTBACK or the initial rendering of the page... (Page.IsPostBack == false)
        // The content parameter is a stream containing the entire page's HTML output
        internal void RenderPostback(Stream next, MemoryStream content)
        {
            // First reading the WHOLE page content into memory since we need
            // to add up the "register control" script for all of our visible controls
            content.Position = 0;
            TextReader reader = new StreamReader(content);
            string wholePageContent = reader.ReadToEnd();

            // Stringbuilder to hold our "register script" parts...
            StringBuilder builder = new StringBuilder();
            AddScriptIncludes(builder, true);
            AddInitializationScripts(builder);

            // Replacing the </body> element with the client-side object creation scripts for the Ra Controls...
            Regex reg = new Regex("</body>", RegexOptions.IgnoreCase);
            wholePageContent = reg.Replace(wholePageContent, builder.ToString());

            builder = new StringBuilder();
            AddScriptIncludes(builder, false);
            builder.Append("<body");

            reg = new Regex("<body", RegexOptions.IgnoreCase);
            wholePageContent = reg.Replace(wholePageContent, builder.ToString());

            // Now writing everything back to client (or next Filter)
            TextWriter writer = new StreamWriter(next);
            writer.Write(wholePageContent);
            writer.Flush();
        }

        // We only come here if this is a Ra-Ajax Callback (IsCallback == true)
        // We don't really care about the HTML rendered by the page here.
        // We just short-circut the whole HTML rendering phase here and only render
        // back changes to the client
        internal void RenderCallback(Stream next, MemoryStream content)
        {
            if (string.IsNullOrEmpty(_redirectUrl))
            {
                Writer.Flush();
                _memStream.Flush();
                _memStream.Position = 0;
                
                TextReader readerContent = new StreamReader(_memStream);
                string allContent = readerContent.ReadToEnd();
                using (TextWriter writer = new StreamWriter(next))
                {
                    AddDynamicScriptIncludes(writer);
                    writer.WriteLine(allContent);

                    UpdateViewState(content, writer);

                    WriterAtBack.Flush();
                    _memStreamBack.Flush();
                    _memStreamBack.Position = 0;

                    readerContent = new StreamReader(_memStreamBack);
                    string allContentAtBack = readerContent.ReadToEnd();
                    writer.WriteLine(allContentAtBack);

                    writer.Flush();
                }
            }
        }
        
        private string GetViewState(string wholePageContent, string searchString)
        {
            string viewStateStart = string.Format("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"", searchString);
            string viewStateValue = GetHiddenInputValue(wholePageContent, viewStateStart);
            return viewStateValue;
        }

        private string GetHiddenInputValue(string html, string marker)
        {
            string value = null;
            int i = html.IndexOf(marker);
            if (i != -1)
            {
                value = html.Substring(i + marker.Length);
                value = value.Substring(0, value.IndexOf('\"'));
            }
            return value;
        }

        private void UpdateViewState(MemoryStream content, TextWriter writer)
        {
            content.Position = 0;
            TextReader reader = new StreamReader(content);
            string wholePageContent = reader.ReadToEnd();

            if (!string.IsNullOrEmpty(_requestViewState))
            {
                if (wholePageContent.IndexOf("__VIEWSTATE") != -1)
                {
                    int idxOfChange = 0;
                    string responseViewState = GetViewState(wholePageContent, "__VIEWSTATE");
                    for (; idxOfChange < responseViewState.Length && idxOfChange < _requestViewState.Length; idxOfChange++)
                    {
                        if (_requestViewState[idxOfChange] != responseViewState[idxOfChange])
                            break;
                    }
                    if (idxOfChange >= responseViewState.Length)
                        responseViewState = "";
                    else
                        responseViewState = responseViewState.Substring(idxOfChange);
                    writer.WriteLine("MUX.$F('__VIEWSTATE', '{0}', {1});", responseViewState, idxOfChange);
                }
            }
            if (wholePageContent.IndexOf("__EVENTVALIDATION") != -1)
            {
                writer.WriteLine("MUX.$F('__EVENTVALIDATION', '{0}', 0);", 
                    GetViewState(wholePageContent, "__EVENTVALIDATION"));
            }
        }
        
        private void AddInitializationScripts(StringBuilder builder)
        {
            builder.Append("<script type=\"text/javascript\">");
            builder.Append(@"
//<![CDATA[
function MUXUnload() {
  MUX.Ajax._pageUnloads = true;
}
function MUXInit() {
");

            Writer.Flush();
            _memStream.Flush();
            _memStream.Position = 0;
            TextReader readerContent = new StreamReader(_memStream);
            string allContent = readerContent.ReadToEnd();
            builder.Append(allContent);

            WriterAtBack.Flush();
            _memStreamBack.Flush();
            _memStreamBack.Position = 0;
            readerContent = new StreamReader(_memStreamBack);
            string allContentAtBack = readerContent.ReadToEnd();
            builder.Append(allContentAtBack);

            // Adding script closing element
            builder.Append(@"
}
(function() {
if (window.addEventListener) {
  window.addEventListener('load', MUXInit, false);
  window.addEventListener('unload', MUXUnload, false);
} else {
  window.attachEvent('onload', MUXInit);
  window.attachEvent('onunload', MUXUnload);
}
})();

");
            _scriptIncludes.ForEach(
                delegate(string script)
                {
                    if (script.IndexOf("<") == 0)
                        script = script.Substring(1);
                    builder.AppendFormat("MUX._scripts['{0}']=true;\r\n", script.Replace("&", "&amp;")); 
                });
            builder.Append("//]]>\r\n</script>");
            builder.Append("</body>");
        }

        private void AddScriptIncludes(StringBuilder builder, bool end)
        {
            foreach (string idx in _scriptIncludes)
            {
                if (idx.IndexOf("<") == 0 == end)
                    continue;
                string script = idx.Replace("&", "&amp;");
                if (script.IndexOf("<") == 0)
                    script = script.Substring(1);
                string scriptInclusion = 
                    string.Format(
                        "<script src=\"{0}\" type=\"text/javascript\"></script>\r\n", script);
                builder.Append(scriptInclusion);
            }
        }

        private void AddDynamicScriptIncludes(TextWriter writer)
        {
            _dynamicScriptIncludes.ForEach(
                delegate(string script)
                {
                    if (script.IndexOf("<") == 0)
                        script = script.Substring(1);
                    writer.WriteLine("MUX.$I('{0}');", script.Replace("&", "&amp;"));
                });
        }
    }
}
