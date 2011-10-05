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
        private static LinkedList<CallStackFrame> Stack;

        /// <summary>
        /// Specifies that the current thread is produced by script program.
        /// </summary>
        [ThreadStatic]
        private static bool IsScriptThread;

        [ThreadStatic]
        private static bool IsInitialized;

        /// <summary>
        /// Returns call stack snapshot of the current thread.
        /// </summary>
        /// <returns></returns>
        public static Stack<CallStackFrame> GetSnapshot()
        {
            return Stack != null ? new Stack<CallStackFrame>(Stack) : new Stack<CallStackFrame>();
        }

        /// <summary>
        /// Gets currently executed action.
        /// </summary>
        public static CallStackFrame Current
        {
            get { return Stack != null && Stack.Last != null ? Stack.Last.Value : null; }
        }

        /// <summary>
        /// Gets depth of the call stack.
        /// </summary>
        public static long Depth
        {
            get { return Stack != null ? Stack.Count : 0L; }
        }

        /// <summary>
        /// Gets a value indicating that the call stack is initialized for the current thread.
        /// </summary>
        private static bool Initialized
        {
            get { return IsInitialized; }
            set
            {
                switch (IsInitialized = value)
                {
                    case true:
                        if (Monitoring.IsEnabled)
                            Stack = new LinkedList<CallStackFrame>();
                        IsScriptThread = Thread.CurrentThread.IsThreadPoolThread;
                        return;
                    default:
                        Stack = null;
                        return;
                }
            }
        }

        /// <summary>
        /// Gets caller of the currently executed action.
        /// </summary>
        public static IScriptAction Caller
        {
            get { return Stack != null && Stack.Last != null && Stack.Last.Previous != null ? Stack.Last.Previous.Value.Action : null; }
        }

        /// <summary>
        /// Returns a script action located at the specified stack frame.
        /// </summary>
        /// <param name="frameNumber">The number of stack frame. 0 indicates top of the stack.</param>
        /// <returns>The script action located at the specified stack frame.</returns>
        public static CallStackFrame GetFrame(long frameNumber)
        {
            switch (Stack != null)
            {
                case true:
                    var frame = Stack.Last;
                    for (var i = 0L; i <= frameNumber; i++)
                        if (frame != null) frame = frame.Previous; else break;
                    return frame != null ? frame.Value : null;
                default:
                    return null;
            }
        }

        internal static void Push(CallStackFrame frame)
        {
            if (!Initialized) Initialized = true;
            if (Stack != null) Stack.AddLast(frame);
        }

        internal static void Push(IScriptAction action, InterpreterState state)
        {
            Push(new CallStackFrame(action, state));
        }

        internal static void Pop()
        {
            if (Stack != null)
            {
                Stack.RemoveLast();
                if (IsScriptThread && Stack.Count == 0)
                {
                    //Provides call stack cleanup of the script produced thread.
                    var generation = NativeGarbageCollector.GetGeneration(Stack);
                    Initialized = false;
                    NativeGarbageCollector.Collect(generation);
                }
            }
        }
    }
}
