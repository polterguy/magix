/*
 * Magix UX - A Managed Ajax Library for ASP.NET
 * Copyright 2010 - 2013 - MareMara13@gmail.com
 * Magix is licensed as bastardized MITx11, see enclosed License.txt File for Details.
 */

using System;

namespace Magix.UX.Builder
{
    public class DeterministicExecutor : IDisposable
    {
        public delegate void Functor();

        private Functor _end;
        private Functor _start;
        private bool disposed;

        public DeterministicExecutor()
        { }

        public DeterministicExecutor(Functor end)
            : this(null, end)
        { }

        public DeterministicExecutor(Functor start, Functor end)
        {
            if (end == null)
                throw new NullReferenceException("No point in using a DeterminsticExecutor unless you supply both a start delegate and an end delegate");
            Start = start;
            _end = end;
        }

        public Functor Start
        {
            set
            {
                if (_start != null)
                    throw new ArgumentException("Can't set Start property twice on DeterministicExecutor");
                _start = value;
                if (_start != null)
                    _start();
            }
        }

        public Functor End
        {
            set
            {
                _end = value;
            }
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
                    if (_end != null)
                        _end();
                }
            }
            disposed = true;
        }
    }
}
