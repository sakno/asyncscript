using System;
using System.Threading;
using System.Diagnostics;
using System.Collections.Generic;

namespace DynamicScript.Runtime.Debugging
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using SystemEnvironment = System.Environment;

    /// <summary>
    /// Represents DynamicScript code debugger.
    /// This class cannot be inherited.
    /// </summary>
    
    [ComVisible(false)]
    public sealed class ScriptDebugger: IScriptDebuggerSession, IDisposable
    {
        private const string DebuggerSlotData = "DSCRIPT_DBG";
        private static EventHandler<DebuggingStartedEventArgs> m_debugHook;

        /// <summary>
        /// Represents an event occured when interpreter start debugging session.
        /// </summary>
        /// <remarks>This event provides singlecast subscription, therefore you can attach only one debugger hook.</remarks>
        public static event EventHandler<DebuggingStartedEventArgs> Debugging
        {
            add { m_debugHook = value; }
            remove { m_debugHook = null; }
        }

        internal static bool HookOverriden
        {
            get { return m_debugHook != null; }
        }

        private readonly Thread m_main;
        private bool m_disposed;
        private readonly ThreadLocal<Stopwatch> m_watchers;
        private readonly HashSet<Uri> m_modules;

        private ScriptDebugger()
        {
            m_disposed = false;
            m_watchers = new ThreadLocal<Stopwatch>();
            m_modules = new HashSet<Uri>();
        }

        internal ScriptDebugger(Thread mt = null)
            : this()
        {
            m_main = mt ?? Thread.CurrentThread;
        }

        internal static void Attach(ScriptDebugger debugger, IScriptObject global)
        {
            Monitoring.Enable();
            if (m_debugHook != null)
                m_debugHook.Invoke(debugger, new DebuggingStartedEventArgs(debugger));
            RegisterDebugger(debugger);
            //push entry point frame
            CallStack.Push(CallStackFrame.CreateEntryPoint(global));
        }

        internal static void Detach()
        {
            //pop entry point frame
            CallStack.Pop();
            m_debugHook = null;
            UnregisterDebugger();
        }

        /// <summary>
        /// Gets main thread.
        /// </summary>
        public Thread MainThread
        {
            get { return m_main; }
        }

        /// <summary>
        /// Occurs when break point located in the source code is reached.
        /// </summary>
        public event EventHandler<BreakPointReachedEventArgs> BreakPointReached;

        internal void OnBreakPoint(string comment, InterpreterState state)
        {
            if (BreakPointReached != null)
            {
                var cancel = false;
                lock (this)
                {
                    var e = new BreakPointReachedEventArgs(comment, Thread.CurrentThread, state);
                    BreakPointReached(this, e);
                    cancel = e.Cancel;
                }
                if (cancel) SystemEnvironment.Exit(1067);   //aborts the current process.
            }
        }

        /// <summary>
        /// Begins time measurement for the current thread.
        /// </summary>
        public void BeginTimeMeasurement()
        {
            m_watchers.Value = Stopwatch.StartNew();
        }

        /// <summary>
        /// Ends time measurement for the current thread.
        /// </summary>
        /// <returns>The amount of time elapsed since <see cref="BeginTimeMeasurement"/> method call.</returns>
        public TimeSpan EndTimeMeasurement()
        {
            switch (m_watchers.Value != null)
            {
                case true: m_watchers.Value.Stop();
                    return m_watchers.Value.Elapsed;
                default: return TimeSpan.Zero;
            }
        }

        private void Dispose(bool disposing)
        {
            if (!m_disposed && disposing)
            {
                m_watchers.Dispose();
            }
            m_disposed = true;
        }

        /// <summary>
        /// Releases all resources associated with the debugger.
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(false);
        }

        /// <summary>
        /// Prepares debugger for garbage collection.
        /// </summary>
        ~ScriptDebugger()
        {
            Dispose(false);
        }

        /// <summary>
        /// Gets collection of loaded script modules.
        /// </summary>
        private ICollection<Uri> Modules
        {
            get { return m_modules; }
        }

        IEnumerable<Uri> IScriptDebuggerSession.Modules
        {
            get 
            {
                lock (this)
                {
                    var result = new Uri[Modules.Count];
                    Modules.CopyTo(result, 0);
                    return result;
                }
            }
        }

        /// <summary>
        /// Occurs when script module is loaded.
        /// </summary>
        public event EventHandler<ScriptModuleLoadedEventArgs> ModuleLoaded;

        internal void OnLoadModule(Uri moduleLocation)
        {
            lock (this)
                Modules.Add(moduleLocation);
            if (ModuleLoaded != null)
                ModuleLoaded(this, new ScriptModuleLoadedEventArgs(moduleLocation));
        }

        internal static void RegisterDebugger(ScriptDebugger debugger, AppDomain domain = null)
        {
            if (debugger == null) throw new ArgumentNullException("debugger");
            if (domain == null) domain = AppDomain.CurrentDomain;
            domain.SetData(DebuggerSlotData, debugger);
        }

        internal static ScriptDebugger GetDebugger(AppDomain domain = null)
        {
            if (domain == null) domain = AppDomain.CurrentDomain;
            return domain.GetData(DebuggerSlotData) as ScriptDebugger;
        }

        internal static void UnregisterDebugger(AppDomain domain = null)
        {
            if (domain == null) domain = AppDomain.CurrentDomain;
            domain.SetData(DebuggerSlotData, null);
        }

        /// <summary>
        /// Gets debugger registered for the current application domain.
        /// </summary>
        internal static ScriptDebugger CurrentDebugger
        {
            get { return GetDebugger(AppDomain.CurrentDomain); }
        }
    }
}
