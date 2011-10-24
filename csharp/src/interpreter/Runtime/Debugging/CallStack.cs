using System;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using NativeGarbageCollector = System.GC;
    using Thread = System.Threading.Thread;

    /// <summary>
    /// Represents actions call stack.
    /// </summary>
    /// <remarks>The call stack is worked only when <see cref="Monitoring.IsEnabled"/> is <see langword="true"/>.</remarks>
    [ComVisible(false)]
    
    public static class CallStack
    {
        /// <summary>
        /// Represents call stack for the current thread.
        /// </summary>
        [ThreadStatic]
        private static LinkedList<CallStackFrame> m_stack;

        /// <summary>
        /// Specifies that the current thread is produced by script program.
        /// </summary>
        [ThreadStatic]
        private static bool IsScriptThread;

        /// <summary>
        /// Returns call stack snapshot of the current thread.
        /// </summary>
        /// <returns></returns>
        public static Stack<CallStackFrame> GetSnapshot()
        {
            return m_stack != null ? new Stack<CallStackFrame>(m_stack) : new Stack<CallStackFrame>();
        }

        /// <summary>
        /// Gets currently executed action.
        /// </summary>
        public static CallStackFrame Current
        {
            get { return m_stack != null && m_stack.Last != null ? m_stack.Last.Value : null; }
        }

        /// <summary>
        /// Gets depth of the call stack.
        /// </summary>
        public static long Depth
        {
            get { return m_stack != null ? m_stack.Count : 0L; }
        }

        /// <summary>
        /// Gets a value indicating that the call stack is initialized for the current thread.
        /// </summary>
        private static bool Initialized
        {
            get { return m_stack != null; }
        }

        /// <summary>
        /// Gets caller of the currently executed action.
        /// </summary>
        public static IScriptAction Caller
        {
            get { return m_stack != null && m_stack.Last != null && m_stack.Last.Previous != null ? m_stack.Last.Previous.Value.Action : null; }
        }

        /// <summary>
        /// Returns a script action located at the specified stack frame.
        /// </summary>
        /// <param name="frameNumber">The number of stack frame. 0 indicates top of the stack.</param>
        /// <returns>The script action located at the specified stack frame.</returns>
        public static CallStackFrame GetFrame(long frameNumber)
        {
            switch (m_stack != null)
            {
                case true:
                    var frame = m_stack.Last;
                    for (var i = 0L; i <= frameNumber; i++)
                        if (frame != null) frame = frame.Previous; else break;
                    return frame != null ? frame.Value : null;
                default:
                    return null;
            }
        }

        internal static void Push(CallStackFrame frame)
        {
            if (m_stack == null)
            {
                m_stack = new LinkedList<CallStackFrame>();
                IsScriptThread = Thread.CurrentThread.IsThreadPoolThread;
            }
            m_stack.AddLast(frame);
        }

        internal static void Push(IScriptAction action, InterpreterState state)
        {
            if (Monitoring.IsEnabled)
                Push(new CallStackFrame(action, state));
        }

        internal static void Pop()
        {
            if (m_stack != null && m_stack.Count > 0)
            {
                m_stack.RemoveLast();
                if (IsScriptThread && m_stack.Count == 0)
                {
                    //Provides call stack cleanup of the script produced thread.
                    var generation = NativeGarbageCollector.GetGeneration(m_stack);
                    m_stack = null;
                    NativeGarbageCollector.Collect(generation);
                }
            }
        }
    }
}
