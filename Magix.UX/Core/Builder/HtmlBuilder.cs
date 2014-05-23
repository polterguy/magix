/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2014 - thomas@magixilluminate.com
 * Magix is licensed as MITx11, see enclosed License.txt File for Details.
 */

using System;
using System.IO;
using System.Web.UI;

namespace Magix.UX.Builder
{
    public class HtmlBuilder : IDisposable
    {
        private HtmlTextWriter _writer;
        private Element _lastElement;
        private bool disposed;
        private MemoryStream _stream;

        public HtmlBuilder()
            : this(null)
        { }

        public HtmlBuilder(HtmlTextWriter writer)
        {
            if (writer == null)
            {
                _stream = new MemoryStream();
                TextWriter tmp = new StreamWriter(_stream);
                _writer = new HtmlTextWriter(tmp);
            }
            else
            {
                _writer = writer;
            }
        }

        public HtmlTextWriter Writer
        {
            get
            {
                if (_lastElement != null)
                    _lastElement.CloseOpeningElement();
                return _writer;
            }
        }

        internal HtmlTextWriter WriterUnClosed
        {
            get { return _writer; }
        }

        internal Stream Stream
        {
            get { return _stream; }
        }

        public Element CreateElement(string elementName)
        {
            if (_lastElement != null)
                _lastElement.CloseOpeningElement();
            _lastElement = new Element(this, elementName);
            return _lastElement;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposed)
            {
                if (disposing)
                {
                    if (_lastElement != null)
                        _lastElement.CloseOpeningElement();
                    if (_stream != null)
                        _stream.Dispose();
                }
            }
            disposed = true;
        }

        public override string ToString()
        {
            if (_stream == null)
                return base.ToString();
            _writer.Flush();
            _stream.Flush();
            _stream.Position = 0;
            TextReader reader = new StreamReader(_stream);
            return reader.ReadToEnd();
        }

        public string ToStringJson()
        {
            if (_stream == null)
                return base.ToString();
            _writer.Flush();
            _stream.Flush();
            _stream.Position = 0;
            TextReader reader = new StreamReader(_stream);
            string retVal = reader.ReadToEnd();
            retVal = retVal.Replace("\\", "\\\\").Replace("'", "\\'").Replace("\r", "\\r").Replace("\n", "\\n");
            return retVal;
        }
    }
}
