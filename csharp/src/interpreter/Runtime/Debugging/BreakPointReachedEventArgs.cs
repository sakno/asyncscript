using System;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Thread = System.Threading.Thread;
    using CancelEventArgs = System.ComponentModel.CancelEventArgs;

    /// <summary>
    /// Represents data of the event occured when break point is reached.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class BreakPointReachedEventArgs : CancelEventArgs
    {
        private readonly string m_comment;
        private readonly Thread m_thread;
        private readonly InterpreterState m_state;

        internal BreakPointReachedEventArgs(string comment, Thread t, InterpreterState state)
            : base(false)
        {
            if (state == null) throw new ArgumentNullException("state");
            m_comment = comment;
            m_thread = t ?? Thread.CurrentThread;
            m_state = state;
        }

        /// <summary>
        /// Gets thread in which break point reached.
        /// </summary>
        public Thread SourceThread
        {
            get { return m_thread; }
        }

        /// <summary>
        /// Gets interpreter state at the break point.
        /// </summary>
        public InterpreterState State
        {
            get { return m_state; }
        }

        /// <summary>
        /// Gets break point comment.
        /// </summary>
        public string Comment
        {
            get { return m_comment ?? string.Empty; }
        }
    }
}
