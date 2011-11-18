using System;
using System.Collections.Generic;
using System.IO;
using System.Diagnostics;

namespace DynamicScript
{
    using Encoding = System.Text.Encoding;
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Represents enumerator through characters in the specified stream.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    sealed class CharEnumerator: IEnumerator<char>
    {
        private const char DefaultCharacter = '\0';
        private readonly StreamReader m_reader;
        private bool m_disposed;
        private char m_current;

        private CharEnumerator()
        {
            m_disposed = false;
            m_current = DefaultCharacter;
        }

        /// <summary>
        /// Initializes a new enumerator through characters in the specified stream.
        /// </summary>
        /// <param name="charStream">The stream with characters. Cannot be <see langword="null"/>.</param>
        /// <param name="enc">The encoding of the text stored in the stream. By default, it is <see cref="System.Text.Encoding.Unicode"/>.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="charStream"/> is <see langword="null"/>.</exception>
        public CharEnumerator(Stream charStream, Encoding enc = null)
            : this()
        {
            if (charStream == null) throw new ArgumentNullException("charStream");
            m_reader = new StreamReader(charStream, enc ?? DefaultEncoding);
        }

        public static Encoding DefaultEncoding
        {
            get { return Encoding.Unicode; }
        }

        private string ObjectName
        {
            get { return GetType().Name; }
        }

        [DebuggerHidden]
        [DebuggerNonUserCode]
        private void VerifyOnDisposed()
        {
            if (m_disposed) throw new ObjectDisposedException(ObjectName);
        }

        /// <summary>
        /// Gets the current character in the stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The enumerator is closed.</exception>
        public char Current
        {
            get 
            {
                VerifyOnDisposed();
                return m_current; 
            }
        }

        public bool EndOfStream
        {
            get
            {
                VerifyOnDisposed();
                return m_reader.EndOfStream;
            }
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        /// <summary>
        /// Reads the next characted from the stream.
        /// </summary>
        /// <returns><see langword="true"/> if the stream end is not reached; otherwise, <see langword="false"/>.</returns>
        /// <exception cref="System.ObjectDisposedException">The enumerator is closed.</exception>
        public bool MoveNext()
        {
            VerifyOnDisposed();
            if (m_reader.EndOfStream) return false;
            var buffer = default(int);
            var success = (buffer = m_reader.Read()) > 0;
            m_current = (char)buffer;
            return success;
        }

        /// <summary>
        /// Sets the position of the enumerator to the beginning of the stream.
        /// </summary>
        /// <exception cref="System.ObjectDisposedException">The enumerator is closed.</exception>
        public void Reset()
        {
            VerifyOnDisposed();
            m_reader.BaseStream.Position = 0;
        }

        /// <summary>
        /// Releases all resources associated with the enumerator.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed && disposing)
            {
                m_reader.Close();
            }
            m_disposed = true;
        }

        ~CharEnumerator()
        {
            Dispose(false);
        }
    }
}
