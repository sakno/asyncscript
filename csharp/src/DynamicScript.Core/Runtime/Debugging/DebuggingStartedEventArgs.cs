using System;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;

    /// <summary>
    /// Provides data for event raised when debugging session is started.
    /// This class cannot be inherited.
    /// </summary>
    [ComVisible(false)]
    
    public sealed class DebuggingStartedEventArgs: EventArgs
    {
        private readonly IScriptDebuggerSession m_session;

        internal DebuggingStartedEventArgs(IScriptDebuggerSession session)
        {
            if (session == null) throw new ArgumentNullException("session");
            m_session = session;
        }

        /// <summary>
        /// Gets debugger session.
        /// </summary>
        public IScriptDebuggerSession DebuggerSession
        {
            get { return m_session; }
        }
    }
}
