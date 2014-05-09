/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - isa.lightbringer@gmail.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
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
    /*
     * natural singleton class, manager for ajax library. ties together everything
     */
    public sealed class Manager
    {
        #region [ -- fields -- ]
        private List<BaseControl> _muxControls = new List<BaseControl>();
        private List<string> _scriptIncludes = new List<string>();
        private List<string> _dynamicScriptIncludes = new List<string>();
        private string _redirectUrl;
        private MemoryStream _internalJavaScriptMemStream;
        private HtmlTextWriter _internalJavaScriptWriter;
        private MemoryStream _javaScriptMemStream;
        private HtmlTextWriter _javascriptWriter;
        private string _requestViewState;
        #endregion

        /*
         * singleton instance
         */
        public static Manager Instance
        {
            get
            {
                if (((Page)HttpContext.Current.CurrentHandler).Items["Magix.UX.Manager"] == null)
                    ((Page)HttpContext.Current.CurrentHandler).Items["Magix.UX.Manager"] = new Manager();
                return (Manager)((Page)HttpContext.Current.CurrentHandler).Items["Magix.UX.Manager"];
            }
        }

        /*
         * returns true if response is an ajax callback
         */
        public bool IsAjaxCallback
        {
            get { return HttpContext.Current.Request.Params["magix.ux.manager.is-callback"] == "true"; }
        }

        /*
         * makes it possible to write to the ajax output stream, at the end of it, to include for 
         * instance javascript for execution on the client
         */
        public HtmlTextWriter JavaScriptWriter
        {
            get
            {
                if (_javascriptWriter == null)
                {
                    _javaScriptMemStream = new MemoryStream();
                    TextWriter textWriter = new StreamWriter(_javaScriptMemStream);
                    _javascriptWriter = new HtmlTextWriter(textWriter);
                }
                return _javascriptWriter;
            }
        }

        /*
         * includes a javascript file from a resource
         */
        public void IncludeResourceScript(Type type, string id)
        {
            IncludeResourceScript(type, id, true);
        }

        /*
         * includes a javascript file from a resource
         */
        public void IncludeResourceScript(Type type, string id, bool atEnd)
        {
            string resource = Page.ClientScript.GetWebResourceUrl(type, id);
            if (!string.IsNullOrEmpty(resource))
                IncludeScript((!atEnd ? "end:" : "") + resource);
        }

        /*
         * includes a javascript file dynamically
         */
        public void IncludeFileScript(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;
            IncludeScript(path);
        }

        /*
         * redirects to another page in an ajax callback
         */
        public void Redirect(string url)
        {
            if (!IsAjaxCallback)
                Page.Response.Redirect(url);
            else
            {
                Page.Response.AddHeader(
                    "Location",
                    Page.Response.ApplyAppPathModifier(url));

                // thx to fuckups at w3c, this is needed ...
                Page.Response.StatusCode = 222;
                _redirectUrl = url;
            }
        }

        #region [ -- private and internals -- ]

        private List<BaseControl> MuxControls
        {
            get { return _muxControls; }
        }

        internal HtmlTextWriter InternalJavaScriptWriter
        {
            get
            {
                if (_internalJavaScriptWriter == null)
                {
                    _internalJavaScriptMemStream = new MemoryStream();
                    TextWriter textwriter = new StreamWriter(_internalJavaScriptMemStream);
                    _internalJavaScriptWriter = new HtmlTextWriter(textwriter);
                }
                return _internalJavaScriptWriter;
            }
        }

        private Page Page
        {
            get { return HttpContext.Current.CurrentHandler as Page; }
        }

        internal void InitializePage(BaseControl ctrl)
        {
            MuxControls.Add(ctrl);
            if (MuxControls.Count == 1)
            {
                // we only run this logic once
                // basically we're adding up the callback filter for custom response rendering
                if (IsAjaxCallback)
                {
                    Page.LoadComplete += LoadComplete_Ajax_Callback;
                    Page.Response.Filter = new AjaxFilter(Page.Response.Filter);
                }
                else
                {
                    Page.LoadComplete += Page_Load_No_Ajax;
                    Page.Response.Filter = new ResponseFilter(Page.Response.Filter);
                }
            }
        }

        private void Page_Load_No_Ajax(object sender, EventArgs e)
        {
            // turning off the autocomplete attribute on the form html element
            string browser = Page.Request.Browser.Browser.ToLower();
            switch (browser)
            {
                case "firefox":
                case "iceweasel":
                case "applewebkit":
                case "mozilla":
                    Page.Form.Attributes.Add("autocomplete", "off");
                    break;
            }
        }

        private void LoadComplete_Ajax_Callback(object sender, EventArgs e)
        {
            // storing viewstate
            _requestViewState = Page.Request.Params["__VIEWSTATE"];

            // finding the control that startet the ajax callback
            string controlId = Page.Request.Params["magix.ux.callback-control"];
            Page.Response.ContentType = "text/javascript";

            if (string.IsNullOrEmpty(controlId))
            {
                // callback was initiated by a function invocation
                string function = Page.Request.Params["magix.ux.function-name"];
                
                if (string.IsNullOrEmpty(function))
                    return;

                Control objectToInvokeOn = Page;
                MethodInfo method = FindWebMethod(function, ref objectToInvokeOn);

                if (method == null || method.GetCustomAttributes(typeof(Magix.UX.Core.WebMethod), false).Length == 0)
                    throw new Exception("cannot call a method without a 'WebMethod' attribute");

                // extracting parameter values as correct types
                ParameterInfo[] parameters = method.GetParameters();
                List<object> args = new List<object>();
                for (int idxPar = 0; 
                    idxPar < parameters.Length && Page.Request.Params["magix.ux.function-parameter" + idxPar] != null; 
                    idxPar++)
                {
                    args.Add(Convert.ChangeType(
                        Page.Request.Params["magix.ux.function-parameter" + idxPar], 
                        parameters[idxPar].ParameterType));
                }

                object retVal = method.Invoke(objectToInvokeOn, args.ToArray());
                if (retVal != null)
                {
                    string retValString = string.Format(CultureInfo.InvariantCulture, "{0}", retVal);
                    retValString = retValString.Replace("\\", "\\\\").Replace("'", "\\'");
                    JavaScriptWriter.Write("MUX.Control._functionReturnValue='{0}';",retValString);
                }
                return;
            }

            BaseControl evtCtrl = MuxControls.Find(
                delegate(BaseControl idx)
                {
                    return idx.ClientID == controlId;
                });

            if (evtCtrl == null)
                throw new ApplicationException("oops, non-existing control initiated an ajax callback! wtf??");

            string evtName = Page.Request.Params["magix.ux.event-name"];

            // Dispatching the event to our MUX Control...
            evtCtrl.RaiseEvent(evtName);
        }

        private MethodInfo FindWebMethod(string functionName, ref Control ctrlToCallFor)
        {
            MethodInfo method = null;
            if (functionName.IndexOf(".") == -1)
            {
                // No UserControls in the picture here...
                method = Page.GetType().BaseType.GetMethod(functionName, 
                    BindingFlags.Instance | 
                    BindingFlags.Public | 
                    BindingFlags.NonPublic);
                if (method != null)
                {
                    ctrlToCallFor = Page;
                }
                else
                {
                    // searching through master page
                    method = Page.Master.GetType().BaseType.GetMethod(functionName,
                        BindingFlags.Instance | 
                        BindingFlags.Public | 
                        BindingFlags.NonPublic);
                    ctrlToCallFor = Page.Master;
                }
            }
            else
            {
                // this is an invocation to a web method in a user control. name of user control should be the first parts of the '.' split
                string[] entities = functionName.Split('.');
                Control ctrl = Selector.SelectFirst<UserControl>(
                    Page,
                    delegate(Control idx)
                    {
                        return entities[0] == idx.ClientID;
                    });
                method = 
                    ctrl.GetType().BaseType.GetMethod(
                        entities[entities.Length - 1],
                        BindingFlags.Instance | 
                        BindingFlags.Public | 
                        BindingFlags.NonPublic);
                ctrlToCallFor = ctrl;
            }
            return method;
        }

        internal void IncludeCoreControlScripts()
        {
            IncludeResourceScript("Control.js");
        }

        internal void IncludeCoreJavaScript()
        {
			IncludeResourceScript("Magix.js");
		}

		internal void IncludeResourceScript(string script)
		{
            string resource = Page.ClientScript.GetWebResourceUrl(typeof(Manager), "Magix.UX.Js." + script);

            if (IsAjaxCallback)
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

        private void IncludeScript(string path)
        {
            if (IsAjaxCallback)
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

        internal void RenderPageNormally(Stream next, MemoryStream content)
        {
            // rendering a normal page postback or initial loading
            content.Position = 0;
            TextReader reader = new StreamReader(content);
            string pageContent = reader.ReadToEnd();

            StringBuilder scripts = new StringBuilder();
            AddScriptIncludes(scripts, true);
            AddMuxInitScripts(scripts);
            scripts.Append("</body>");

            // appending our scripts at bottom of page
            Regex replacerRegEx = new Regex("</body>", RegexOptions.IgnoreCase);
            pageContent = replacerRegEx.Replace(pageContent, scripts.ToString());

            // flushing out ...
            using (TextWriter writer = new StreamWriter(next))
            {
                writer.Write(pageContent);
                writer.Flush();
            }
        }

        internal void RenderCallback(Stream next, MemoryStream content)
        {
            // rendering an ajax callback
            if (string.IsNullOrEmpty(_redirectUrl))
            {
                // response was not redirected
                InternalJavaScriptWriter.Flush();
                _internalJavaScriptMemStream.Flush();
                _internalJavaScriptMemStream.Position = 0;

                using (TextReader readerContent = new StreamReader(_internalJavaScriptMemStream))
                {
                    string allContent = readerContent.ReadToEnd();
                    using (TextWriter writer = new StreamWriter(next))
                    {
                        AddDynamicScriptIncludes(writer);
                        writer.WriteLine(allContent);

                        SetViewState(content, writer);

                        JavaScriptWriter.Flush();
                        _javaScriptMemStream.Flush();
                        _javaScriptMemStream.Position = 0;

                        using (TextReader readerContent2 = new StreamReader(_javaScriptMemStream))
                        {
                            string allContentAtBack = readerContent2.ReadToEnd();
                            writer.WriteLine(allContentAtBack);
                        }

                        writer.Flush();
                    }
                }
            }
        }
        
        private string GetHiddenFieldValue(string html, string query)
        {
            string queryStart = string.Format("<input type=\"hidden\" name=\"{0}\" id=\"{0}\" value=\"", query);
            string value = null;
            int indexOfField = html.IndexOf(queryStart);
            if (indexOfField != -1)
            {
                value = html.Substring(indexOfField + queryStart.Length);
                value = value.Substring(0, value.IndexOf('\"'));
            }
            return value;
        }

        private void SetViewState(MemoryStream content, TextWriter writer)
        {
            content.Position = 0;
            using (TextReader reader = new StreamReader(content))
            {
                string wholePageContent = reader.ReadToEnd();

                if (!string.IsNullOrEmpty(_requestViewState))
                {
                    if (wholePageContent.IndexOf("__VIEWSTATE") != -1)
                    {
                        int idxOfChange = 0;
                        string responseViewState = GetHiddenFieldValue(wholePageContent, "__VIEWSTATE");
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
                        GetHiddenFieldValue(wholePageContent, "__EVENTVALIDATION"));
                }
            }
        }
        
        private void AddMuxInitScripts(StringBuilder builder)
        {
            builder.Append("<script type=\"text/javascript\">");
            builder.Append("//<![CDATA[\r\nfunction MUXUnload(){MUX.Ajax._pageUnloads = true;}function MUXInit() {");

            InternalJavaScriptWriter.Flush();
            _internalJavaScriptMemStream.Flush();
            _internalJavaScriptMemStream.Position = 0;
            using (TextReader readerContent = new StreamReader(_internalJavaScriptMemStream))
            {
                string allContent = readerContent.ReadToEnd();
                builder.Append(allContent);

                JavaScriptWriter.Flush();
                _javaScriptMemStream.Flush();
                _javaScriptMemStream.Position = 0;
                using (TextReader readerPublicScripts = new StreamReader(_javaScriptMemStream))
                {
                    string allContentAtBack = readerContent.ReadToEnd();
                    builder.Append(allContentAtBack);
                }

                // Adding script closing element
                builder.Append("}(function(){if (window.addEventListener){window.addEventListener('load',MUXInit,false);" +
                    "window.addEventListener('unload',MUXUnload,false);}else{window.attachEvent('onload',MUXInit);" + 
                    "window.attachEvent('onunload',MUXUnload);}})();");

                foreach (string idxScript in _scriptIncludes)
                {
                    string script = idxScript;
                    if (idxScript.IndexOf("end:") == 0)
                        script = idxScript.Substring(4);
                    builder.AppendFormat("MUX._scripts['{0}']=true;", script.Replace("&", "&amp;"));
                }
                builder.Append("\r\n//]]>\r\n</script>");
            }
        }

        private void AddScriptIncludes(StringBuilder builder, bool end)
        {
            foreach (string idx in _scriptIncludes)
            {
                if (idx.IndexOf("end:") == 0 == end)
                    continue;
                string script = idx.Replace("&", "&amp;");
                if (script.IndexOf("end:") == 0)
                    script = script.Substring(4);
                string scriptInclusion = 
                    string.Format(
                        "<script src=\"{0}\" type=\"text/javascript\"></script>\r\n", script);
                builder.Append(scriptInclusion);
            }
        }

        private void AddDynamicScriptIncludes(TextWriter writer)
        {
            foreach (string idxScript in _dynamicScriptIncludes)
            {
                string script = idxScript;
                if (script.IndexOf("end:") == 0)
                    script = script.Substring(4);
                script = script.Replace("&", "&amp;");
                writer.WriteLine("MUX.$I('{0}');", script);
            }
        }

        #endregion
    }
}
