/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.UX.Builder
{
    public class Element : DeterministicExecutor
    {
        private HtmlBuilder _builder;
        private bool _closed;

        public Element(HtmlBuilder builder, string elementName)
        {
            _builder = builder;
            _builder.Writer.Write("<" + elementName);
            End = delegate 
            {
                this.CloseOpeningElement();
                this._builder.Writer.Write("</" + elementName + ">");
            };
        }

        public void AddAttribute(string name, string value)
        {
            if (_closed)
                throw new Exception("Can't add an attribute once the attribute is closed due to accessing the underlaying Writer or something else");
            if (value == null)
                _builder.WriterUnClosed.Write(" " + name);
            else
                _builder.WriterUnClosed.Write(" " + name + "=\"" + value.Replace("\"", "&quot;") + "\"");
        }

        public void Write(string content, params object[] args)
        {
            _builder.Writer.Write(content, args);
        }

        public void Write(string content)
        {
            _builder.Writer.Write(content);
        }

        internal void CloseOpeningElement()
        {
            if (_closed)
                return;
            _closed = true;
            _builder.Writer.Write(">");
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _builder.Writer.Flush();
        }
    }
}
