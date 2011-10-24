using System;
using System.Linq.Expressions;
using System.ComponentModel;
using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DynamicScript.Runtime
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using MethodImplAttribute = System.Runtime.CompilerServices.MethodImplAttribute;
    using MethodImplOptions = System.Runtime.CompilerServices.MethodImplOptions;
    using WebRequest = System.Net.WebRequest;
    using CompiledScriptAttribute = Hosting.CompiledScriptAttribute;
    using ScriptObject = Environment.ScriptObject;
    using RuntimeHelpers = Environment.RuntimeHelpers;

    /// <summary>
    /// Represents internal state of the interpreter.
    /// </summary>
    [ComVisible(false)]
    public sealed class InterpreterState
    {
        #region Nested Types
        [ComVisible(false)]
        private sealed class CompiledScriptCookie : IScriptCookie
        {
            private readonly ScriptInvoker m_compiled;

            public CompiledScriptCookie(ScriptInvoker compiledScript)
            {
                if (compiledScript == null) throw new ArgumentNullException("compiledScript");
                m_compiled = compiledScript;
            }

            public ScriptInvoker CompiledScript
            {
                get { return m_compiled; }
            }

            public IScriptObject ScriptResult
            {
                get;
                set;
            }
        }

        /// <summary>
        /// Represents default implementation of the compiled script cache.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class ScriptCache : IScriptCache
        {
            private readonly IDictionary<Uri, IScriptCookie> m_cache;

            public ScriptCache()
            {
                m_cache = new Dictionary<Uri, IScriptCookie>(10);
            }

            public IScriptCookie Lookup(Uri scriptFile, Func<IEnumerator<char>, ScriptInvoker> compiler)
            {
                if (scriptFile == null) throw new ArgumentNullException("scriptFile");
                if (compiler == null) throw new ArgumentNullException("compiler");
                lock (this)
                    return m_cache.ContainsKey(scriptFile) ? m_cache[scriptFile] : m_cache[scriptFile] = Compile(scriptFile, compiler);
            }

            /// <summary>
            /// Determines whether the specified script source is cached.
            /// </summary>
            /// <param name="scriptFile">The location of the script source.</param>
            /// <returns><see langword="true"/> if the specified script source is cached; otherwise, <see langword="false"/>.</returns>
            public bool Cached(Uri scriptFile)
            {
                lock (this) return m_cache.ContainsKey(scriptFile);
            }

            IEnumerable<Uri> IScriptCache.Modules
            {
                get { return m_cache.Keys; }
            }
        }

        [ComVisible(false)]
        private sealed class InternPool
        {
            private readonly Environment.StringPool m_strings;
            private readonly Environment.RealPool m_reals;
            private readonly Environment.IntegerPool m_integers;

            public InternPool(int capacity)
            {
                m_strings = new Environment.StringPool(capacity);
                m_reals = new Environment.RealPool(capacity);
                m_integers = new Environment.IntegerPool(capacity);
            }

            /// <summary>
            /// Adds reference to the specified object into pool of system references.
            /// </summary>
            /// <typeparam name="TScriptObject">Type of object to intern.</typeparam>
            /// <param name="obj">An object to intern.</param>
            /// <returns>An identifier of system reference.</returns>
            public long Intern<TScriptObject>(TScriptObject obj)
                where TScriptObject : ScriptObject
            {
                if (obj is Environment.ScriptString)
                    return m_strings.Intern(obj as Environment.ScriptString);
                else if (obj is Environment.ScriptInteger)
                    return m_integers.Intern(obj as Environment.ScriptInteger);
                else if (obj is Environment.ScriptReal)
                    return m_reals.Intern(obj as Environment.ScriptReal);
                else throw new NotSupportedException();
            }

            /// <summary>
            /// Determines whether the specified object is interned.
            /// </summary>
            /// <typeparam name="TScriptObject">Type of object to check.</typeparam>
            /// <param name="obj">An object to check.</param>
            /// <returns><see langword="true"/> if <paramref name="obj"/> is interned; otherwise, <see langword="false"/>.</returns>
            public bool IsInterned<TScriptObject>(TScriptObject obj)
                where TScriptObject : ScriptObject
            {
                if (obj is Environment.ScriptString)
                    return m_strings.IsInterned(obj as Environment.ScriptString);
                else if (obj is Environment.ScriptInteger)
                    return m_integers.IsInterned(obj as Environment.ScriptInteger);
                else if (obj is Environment.ScriptReal)
                    return m_reals.IsInterned(obj as Environment.ScriptReal);
                else return false;
            }

            public TScriptObject GetInternedObject<TScriptObject>(long id)
                where TScriptObject : ScriptObject
            {
                if (Equals(typeof(TScriptObject), Environment.IntegerPool.ObjectType))
                    return m_integers[id] as TScriptObject;
                else if (Equals(typeof(TScriptObject), Environment.StringPool.ObjectType))
                    return m_strings[id] as TScriptObject;
                else if (Equals(typeof(TScriptObject), Environment.RealPool.ObjectType))
                    return m_reals[id] as TScriptObject;
                else return null;
            }
        }
        #endregion
        [ThreadStatic]
        private static InterpreterState m_current;
        private static InterpreterState m_initial;

        private readonly InterpretationContext m_context;
        private readonly bool m_debug;
        private readonly IScriptObject m_global;
        private readonly ReadOnlyCollection<string> m_args;
        private readonly IScriptCache m_cache;
        private readonly InternPool m_intern;
        private readonly IDictionary<string, object> m_dataSlots;

        private InterpreterState(IScriptCache cache, IScriptObject global, ReadOnlyCollection<string> args, InternPool internPool, IDictionary<string, object> dataSlots, InterpretationContext context, bool debug)
        {
            if (cache == null) throw new ArgumentNullException("cache");
            if (global == null) throw new ArgumentNullException("global");
            m_context = context;
            m_debug = debug;
            m_global = global;
            m_args = args ?? new ReadOnlyCollection<string>(new string[0]);
            m_cache = cache;
            m_intern = internPool ?? new InternPool(10);
            m_dataSlots = dataSlots ?? new Dictionary<string, object>(StringComparer.Ordinal)
                    {
                        {RuntimeBehavior.DataSlotName, new RuntimeBehavior()}
                    };
        }

        /// <summary>
        /// Initializes a new interpreter state.
        /// </summary>
        /// <param name="cache">The cache that is used to store compiled scripts. Cannot be <see langword="null"/>.</param>
        /// <param name="global">The global script object. Cannot be <see langword="null"/>.</param>
        /// <param name="args">Represents the arguments passed to the script.</param>
        /// <param name="internCapacity">Capacity of intern pool.</param>
        /// <param name="context">Interpretation context.</param>
        /// <param name="debug">Specifies that the DynamicScript program running in the debug mode.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="global"/> or <paramref name="cache"/> is <see langword="null"/>.</exception>
        public InterpreterState(IScriptCache cache, IScriptObject global, IList<string> args, int internCapacity, InterpretationContext context = InterpretationContext.Default, bool debug = false)
            : this(cache, global, new ReadOnlyCollection<string>(args ?? new string[0]), new InternPool(internCapacity), null, context, debug)
        {
        }

        /// <summary>
        /// Initializes a new interpreter state.
        /// </summary>
        /// <param name="global">The global script object. Cannot be <see langword="null"/>.</param>
        /// <param name="args">Represents the arguments passed to the script.</param>
        /// <param name="internCapacity">Capacity of intern pool.</param>
        /// <param name="context">Interpretation context.</param>
        /// <param name="debug">Specifies that the DynamicScript program running in the debug mode.</param>
        /// <exception cref="System.ArgumentNullException"><paramref name="global"/> is <see langword="null"/>.</exception>
        public InterpreterState(IScriptObject global, IList<string> args, int internCapacity, InterpretationContext context = InterpretationContext.Default, bool debug = false)
            : this(new ScriptCache(), global, args, internCapacity, context, debug)
        {

        }

        /// <summary>
        /// Adds reference to the specified object into pool of system references.
        /// </summary>
        /// <typeparam name="TScriptObject">Type of object to intern.</typeparam>
        /// <param name="obj">An object to intern.</param>
        /// <returns>An identifier of system reference.</returns>
        public long Intern<TScriptObject>(TScriptObject obj)
            where TScriptObject : ScriptObject
        {
            return m_intern.Intern(obj);
        }

        /// <summary>
        /// Determines whether the specified object is interned.
        /// </summary>
        /// <typeparam name="TScriptObject">Type of object to check.</typeparam>
        /// <param name="obj">An object to check.</param>
        /// <returns><see langword="true"/> if <paramref name="obj"/> is interned; otherwise, <see langword="false"/>.</returns>
        public bool IsInterned<TScriptObject>(TScriptObject obj)
            where TScriptObject : ScriptObject
        {
            return obj != null ? m_intern.IsInterned(obj) : false;
        }

        /// <summary>
        /// Gets system's reference of object.
        /// </summary>
        /// <param name="id">Internal identifier of reference.</param>
        /// <returns>An object associated with the specified identifier.</returns>
        public TScriptObject GetInternedObject<TScriptObject>(long id)
            where TScriptObject: ScriptObject
        {
            return m_intern.GetInternedObject<TScriptObject>(id);
        }

        /// <summary>
        /// Saves or restores data slot.
        /// </summary>
        /// <param name="dataSlotName">The name of data slot. Cannot be <see langword="null"/> or empty.</param>
        /// <returns>An object stored in data slot.</returns>
        /// <exception cref="System.ArgumentNullException"><paramref name="dataSlotName"/> is <see langword="null"/> or empty.</exception>
        public object this[string dataSlotName]
        {
            get
            {
                if (string.IsNullOrEmpty(dataSlotName)) return null;
                lock (m_dataSlots)
                {
                    var result = default(object);
                    return m_dataSlots.TryGetValue(dataSlotName, out result) ? result : null;
                }
            }
            set
            {
                if (string.IsNullOrEmpty(dataSlotName)) throw new ArgumentNullException("dataSlotName");
                lock (m_dataSlots)
                    m_dataSlots[dataSlotName] = value;
            }
        }

        /// <summary>
        /// Gets runtime behavior of the interpreter.
        /// </summary>
        public RuntimeBehavior Behavior
        {
            get { return this[RuntimeBehavior.DataSlotName] as RuntimeBehavior ?? new RuntimeBehavior(); }
        }

        /// <summary>
        /// Gets cache of compiled scripts.
        /// </summary>
        public IScriptCache Cache
        {
            get { return m_cache; }
        }

        /// <summary>
        /// Gets collection of the arguments passed to the script.
        /// </summary>
        public ReadOnlyCollection<string> Arguments
        {
            get { return m_args; }
        }

        /// <summary>
        /// Gets global script object.
        /// </summary>
        public IScriptObject Global
        {
            get { return m_global; }
        }

        /// <summary>
        /// Gets interpretation context.
        /// </summary>
        public InterpretationContext Context
        {
            get { return m_context; }
        }

        /// <summary>
        /// Gets a value indicating that the DynamicScript program is running in the debug mode.
        /// </summary>
        public bool DebugMode
        {
            get { return m_debug; }
        }

        /// <summary>
        /// Gets initial interpreter state.
        /// </summary>
        /// <remarks>Initial state can be overridden in the derived class.</remarks>
        public static InterpreterState Initial
        {
            get 
            {
                if (m_initial == null) m_initial = new InterpreterState(ScriptObject.Void, new string[0], 1000);
                return m_initial;
            }
        }

        /// <summary>
        /// Gets or sets internal state of the interpreter at the current thread.
        /// </summary>
        public static InterpreterState Current
        {
            get { return m_current ?? Initial; }
            set { m_current = value; }
        }

        /// <summary>
        /// Changes interpreter state.
        /// </summary>
        /// <param name="scopeObj">A new scope object.</param>
        /// <returns>A new interpreter state that references modified scope object.</returns>
        internal InterpreterState Update(IScriptObject scopeObj)
        {
            return new InterpreterState(Cache, scopeObj, Arguments, m_intern, m_dataSlots, Context, DebugMode);
        }

        /// <summary>
        /// Changes interpreter state.
        /// </summary>
        /// <param name="context">A new interpretation context.</param>
        /// <returns>A new interpretation state.</returns>
        public InterpreterState Update(InterpretationContext context)
        {
            return new InterpreterState(Cache, Global, Arguments, m_intern, m_dataSlots, context, DebugMode);
        }

        /// <summary>
        /// Changes interpreter state.
        /// </summary>
        /// <param name="state">The current state of the interpreter.</param>
        /// <param name="context">A new interpretation context.</param>
        public static void Update(ref InterpreterState state, InterpretationContext context)
        {
            if (state != null)
                state = state.Update(context);
        }

        /// <summary>
        /// Changes interpreter state.
        /// </summary>
        /// <param name="debugMode">Specifies that the interpreter is in debug mode.</param>
        /// <returns>A new interpretation state.</returns>
        public InterpreterState Update(bool debugMode)
        {
            return new InterpreterState(Cache, Global, Arguments, m_intern, m_dataSlots, Context, debugMode);
        }

        /// <summary>
        /// Changes interpreter state.
        /// </summary>
        /// <param name="state">The current state of the interpreter.</param>
        /// <param name="debugMode">Specifies that the interpreter is in debug mode.</param>
        public static void Update(ref InterpreterState state, bool debugMode)
        {
            if (state != null) state = state.Update(debugMode);
        }

        internal static MethodCallExpression Update(ParameterExpression stateVar, ConstantExpression context)
        {
            var update = LinqHelpers.BodyOf<InterpreterState, InterpretationContext, InterpreterState, MethodCallExpression>((s, c) => s.Update(c));
            return update.Update(stateVar, new[] { context });
        }

        internal static MethodCallExpression Update(ParameterExpression stateVar, InterpretationContext context)
        {
            return Update(stateVar, LinqHelpers.Constant<InterpretationContext>(context));
        }

        internal static IScriptCookie Compile(Uri scriptLocation, Func<IEnumerator<char>, ScriptInvoker> compiler)
        {
            if (scriptLocation == null) throw new ArgumentNullException("scriptLocation");
            if (compiler == null) throw new ArgumentNullException("compiler");
            var compiledScript = CompiledScriptAttribute.Load(scriptLocation);
            if (compiledScript == null)
            {
                var request = WebRequest.Create(scriptLocation);
                using (var response = request.GetResponse())
                using (var stream = response.GetResponseStream())
                using (var enumerator = new CharEnumerator(stream))
                    compiledScript = compiler.Invoke(enumerator);
            }
            return new CompiledScriptCookie(compiledScript);
        }

        internal static MethodInfo GetInternMethodInfo<TScriptObject>()
            where TScriptObject : ScriptObject, IConvertible
        {
            return LinqHelpers.BodyOf<Func<InterpreterState, TScriptObject, long>, MethodCallExpression>((state, obj) => state.Intern<TScriptObject>(obj)).Method;
        }

        internal static Expression FromInternPool<TScriptObject>(ParameterExpression stateVar, long iid)
            where TScriptObject : ScriptObject, IConvertible
        {
            var idx = LinqHelpers.BodyOf<Func<InterpreterState, long, IScriptObject>, MethodCallExpression>((s, i) => s.GetInternedObject<TScriptObject>(i)).Method;
            return Expression.Call(stateVar, idx, Expression.Constant(iid));
        }

        internal static MemberExpression GlobalGetterExpression(ParameterExpression stateVar)
        {
            var globp = (PropertyInfo)LinqHelpers.BodyOf<Func<InterpreterState, IScriptObject>, MemberExpression>(s => s.Global).Member;
            return Expression.Property(stateVar, globp);
        }

        internal static ConstructorInfo Constructor 
        {
            get
            {
                var ctor = LinqHelpers.BodyOf<IScriptObject, IList<string>, int, InterpretationContext, bool, InterpreterState, NewExpression>((g, args, size, ctx, dbg) => new InterpreterState(g, args, size, ctx, dbg));
                return ctor.Constructor;
            }
        }

        /// <summary>
        /// Releases the value stored in the specified runtime slot.
        /// </summary>
        /// <param name="slot">A runtime slot to release.</param>
        /// <param name="state"></param>
        /// <returns>A default object that is satisfied to the slot contract.</returns>
        public static IScriptObject DeleteValue(IRuntimeSlot slot, InterpreterState state)
        {
            return slot.DeleteValue() ? slot.GetContractBinding().FromVoid(state) : slot.GetValue(state);
        }

        internal static MethodCallExpression DeleteValue(Expression slot, ParameterExpression state)
        {
            return LinqHelpers.Call<IRuntimeSlot, InterpreterState, IScriptObject>((s, t) => DeleteValue(s, t), null, slot, state);
        }
    }
}
