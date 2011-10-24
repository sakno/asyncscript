using System;
using System.Dynamic;
using System.Collections.Generic;
using System.IO;
using System.Linq.Expressions;
using System.Linq;

namespace DynamicScript.Runtime.Environment.ObjectModel
{
    using ComVisibleAttribute = System.Runtime.InteropServices.ComVisibleAttribute;
    using Thread = System.Threading.Thread;
    using QScriptIO = Hosting.DynamicScriptIO;
    using SystemConverter = System.Convert;
    using ConstructorInfo = System.Reflection.ConstructorInfo;
    using Process = System.Diagnostics.Process;
    using SystemEnvironment = System.Environment;
    using CallStack = Debugging.CallStack;
    using InterpretationContext = Compiler.Ast.InterpretationContext;
    using Resources = Properties.Resources;
    using ScriptDebugger = Debugging.ScriptDebugger;
    using TransparentActionAttribute = Debugging.TransparentActionAttribute;
    using Enumerable = System.Linq.Enumerable;
    using CultureInfo = System.Globalization.CultureInfo;

    /// <summary>
    /// Represents an object that holds basic routines for DynamicScript programs.
    /// </summary>
    [ComVisible(false)]
    public class ScriptModule : ScriptCompositeObject
    {
        #region Nested Type

        [ComVisible(false)]
        private sealed class WeakRefAction : ScriptFunc<IScriptObject>
        {
            public const string Name = "weakref";
            private const string FirstParamName = "obj";

            public WeakRefAction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptObject arg0)
            {
                return arg0 is ScriptWeakReference ? arg0 : new ScriptWeakReference(arg0);
            }
        }

        [ComVisible(false)]
        private sealed class AdjustAction : ScriptFunc<ScriptActionBase, IScriptObject>
        {
            public const string Name = "adjust";
            private const string FirstParamName = "act";
            private const string SecondParamName = "obj";

            public AdjustAction()
                : base(FirstParamName, ScriptSuperContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptActionBase action, IScriptObject @this)
            {
                return action != null ? action.ChangeThis(@this) : null;
            }
        }

        [ComVisible(false)]
        private sealed class GetDataAction : ScriptFunc<ScriptString>
        {
            public const string Name = "getdata";
            private const string FirstParamName = "name";

            public GetDataAction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString name)
            {
                return GetData(ctx, name);
            }
        }

        [ComVisible(false)]
        private sealed class SetDataAction : ScriptAction<ScriptString, IScriptObject>
        {
            public const string Name = "setdata";
            private const string FirstParamName = "name";
            private const string SecondParamName = "data";

            public SetDataAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptSuperContract.Instance)
            {
            }

            protected override void Invoke(InvocationContext ctx, ScriptString name, IScriptObject data)
            {
                SetData(ctx, name, data);
            }
        }

        [ComVisible(false)]
        private sealed class ReflectAction : ScriptFunc<IScriptObject>
        {
            public const string Name = "reflect";
            private const string FirstParamName = "obj";

            public ReflectAction()
                : base(FirstParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptObject obj)
            {
                if (obj is ScriptCompositeContract)
                    return ScriptModule.Reflect(ctx, (ScriptCompositeContract)obj);
                else if (obj is ScriptCompositeObject)
                    return ScriptModule.Reflect(ctx, (ScriptCompositeObject)obj);
                else return ScriptCompositeContract.Empty.CreateCompositeObject(new IScriptObject[0], ctx.RuntimeState);
            }
        }

        /// <summary>
        /// Represents action that is used to run DynamicScript programs.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        private sealed class EvalAction : ScriptFunc<ScriptString, IScriptObject>
        {
            public const string Name = "eval";
            private const string FirstParamName = "script";
            private const string SecondParamName = "scopeObj";

            /// <summary>
            /// Initializes a new instance of the action.
            /// </summary>
            public EvalAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptSuperContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString scriptCode, IScriptObject global)
            {
                if (IsVoid(global)) global = ctx.Global;
                if (scriptCode == null) scriptCode = ScriptString.Empty;
                using (var enumerator = scriptCode.Value.GetEnumerator())
                {
                    var compiledScript = DynamicScriptInterpreter.OnFlyCompiler.Invoke(enumerator);
                    return compiledScript.Invoke(ctx.RuntimeState);
                }
            }
        }

        [ComVisible(false)]
        private sealed class SplitAction : ScriptFunc<ScriptCompositeObject>
        {
            public const string Name = "split";
            private const string FirstParamName = "obj";

            public SplitAction()
                : base(FirstParamName, ScriptCompositeContract.Empty, ScriptIterable.GetContractBinding())
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptCompositeObject obj)
            {
                return new ScriptIterable(obj != null ? obj.Split() : Enumerable.Empty<ScriptCompositeObject>());
            }
        }

        [ComVisible(false)]
        private sealed class ParseAction : ScriptFunc<ScriptString, IScriptContract, ScriptString>
        {
            private const string FirstParamName = "value";
            private const string SecondParamName = "t";
            private const string ThirdParamName = "lang";
            public const string Name = "parse";

            public ParseAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptMetaContract.Instance, ThirdParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            private static IScriptObject Invoke(ScriptString value, IScriptContract type, CultureInfo formatProvider)
            {
                if (type is ScriptIntegerContract)
                    return ScriptInteger.TryParse(value, formatProvider);
                else if (type is ScriptStringContract || type is ScriptSuperContract)
                    return value;
                else if (type is ScriptRealContract)
                    return ScriptRealContract.TryParse(value, formatProvider);
                else if (type is ScriptBooleanContract)
                    return ScriptBooleanContract.TryParse(value);
                else if (type is ExpressionTrees.ScriptExpressionFactory)
                    return ExpressionTrees.ScriptExpressionFactory.Parse(value);
                else if (type is ScriptVoid)
                    return Void;
                else return Void;
            }

            private static IScriptObject Invoke(ScriptString value, IScriptContract type, string language)
            {
                return Invoke(value, type, string.IsNullOrWhiteSpace(language) ? CultureInfo.InvariantCulture : CultureInfo.GetCultureInfoByIetfLanguageTag(language));
            }

            public override IScriptObject Invoke(InvocationContext ctx, ScriptString value, IScriptContract type, ScriptString language)
            {
                return Invoke(value, type, language);
            }
        }

        [ComVisible(false)]
        private sealed class EnumAction : ScriptFunc<IScriptArray>
        {
            public const string Name = "enum";
            private const string FirstParamName = "elements";

            public EnumAction()
                : base(FirstParamName, ScriptDimensionalContract.Instance, ScriptFinSetContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptArray array)
            {
                return array.GetContractBinding().Rank == 1 || array.GetLength(0) > 1L ? new ScriptSetContract(array) : null;
            }
        }

        [ComVisible(false)]
        private sealed class PutsAction : ScriptAction<IScriptObject>
        {
            /// <summary>
            /// Represents name of the action.
            /// </summary>
            public const string Name = "puts";
            private static string FirstParamName = "obj";

            public PutsAction()
                : base(FirstParamName, ScriptSuperContract.Instance)
            {
            }

            protected override void Invoke(InvocationContext ctx, IScriptObject obj)
            {
                ScriptModule.Puts(ctx, obj);
            }
        }

        [ComVisible(false)]
        private sealed class GetsAction : ScriptFunc
        {
            public const string Name = "gets";

            public GetsAction()
                : base(ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx)
            {
                return ScriptModule.Gets(ctx);
            }
        }

        [ComVisible(false)]
        private sealed class NewObjAction : ScriptFunc<ScriptString, IScriptContract>
        {
            public const string Name = "newobj";
            private const string FirstParamName = "name";
            private const string SecondParamName = "contract";

            public NewObjAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptMetaContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString name, IScriptContract contract)
            {
                return name == null || IsVoid(contract) ? Void : ScriptModule.NewObj(ctx, name, contract);
            }
        }

        [ComVisible(false)]
        private sealed class DebugSlot : ObservableSlot
        {
            public const string Name = "debug";

            public DebugSlot()
                : base(ScriptBoolean.False, ScriptBooleanContract.Instance, true)
            {
            }

            protected override IScriptObject GetValue(IScriptObject value, InterpreterState state)
            {
                return (ScriptBoolean)state.DebugMode;
            } 
        }

        [ComVisible(false)]
        private sealed class CompiledSlot : ObservableSlot
        {
            public const string Name = "compiled";

            public CompiledSlot()
                : base(ScriptBoolean.True, ScriptBooleanContract.Instance, true)
            {
            }

            protected override IScriptObject GetValue(IScriptObject value, InterpreterState state)
            {
                return ScriptBoolean.True;
            }
        }

        [ComVisible(false)]
        private sealed class ArgsSlot : ObservableSlot
        {
            public const string Name = "args";
            private ScriptArray m_cached;

            public ArgsSlot()
                : base(ScriptArray.Empty(ScriptStringContract.Instance), true)
            {
            }

            protected override IScriptObject GetValue(IScriptObject value, InterpreterState state)
            {
                if (m_cached == null || m_cached.GetLength(0) != state.Arguments.Count)
                {
                    m_cached = state.Arguments.Count == 0 ? ScriptArray.Empty(ScriptStringContract.Instance) :
                        ScriptArray.Create(state.Arguments.Select(a => new ScriptString(a)).ToArray());
                }
                return m_cached;
            }
        }

        [ComVisible(false)]
        private sealed class UseAction : ScriptFunc<ScriptString>
        {
            /// <summary>
            /// Represents name of the action.
            /// </summary>
            public const string Name = "use";
            private const string FirstParamName = "scriptFile";


            public UseAction()
                : base(FirstParamName, ScriptStringContract.Instance, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, ScriptString scriptFile)
            {
                return Use(ctx, scriptFile);
            }
        }

        [ComVisible(false)]
        private sealed class PrepareAction : ScriptAction<ScriptString, ScriptBoolean>
        {
            public const string Name = "prepare";
            private const string FirstParamName = "scriptFile";
            private const string SecondParamName = "pc";

            public PrepareAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptBooleanContract.Instance)
            {
            }

            protected override void Invoke(InvocationContext ctx, ScriptString scriptFile, ScriptBoolean cache)
            {
                Prepare(ctx, scriptFile, cache);
            }
        }

        [ComVisible(false)]
        private sealed class CmdAction : ScriptFunc<ScriptString, ScriptString, ScriptInteger>
        {
            public const string Name = "cmd";
            private const string FirstParamName = "command";
            private const string SecondParamName = "arguments";
            private const string ThirdParamName = "timeout";

            public CmdAction()
                : base(FirstParamName, ScriptStringContract.Instance, SecondParamName, ScriptStringContract.Instance, ThirdParamName, ScriptIntegerContract.Instance, ScriptIntegerContract.Instance)
            {
            }

            public override IScriptObject Invoke(InvocationContext ctx, ScriptString fileName, ScriptString arguments, ScriptInteger timeout)
            {
                return Cmd(ctx, fileName, arguments, timeout);
            }
        }

        [ComVisible(false)]
        private sealed class WorkingDirectorySlot : ObservableSlot
        {
            public const string Name = "wdir";

            public WorkingDirectorySlot()
                : base(ScriptString.Empty)
            {
            }

            protected override IScriptObject GetValue(IScriptObject value, InterpreterState state)
            {
                return new ScriptString(SystemEnvironment.CurrentDirectory);
            }

            protected override void SetValue(ref IScriptObject value, InterpreterState state)
            {
                SystemEnvironment.CurrentDirectory = SystemConverter.ToString(value);
            }
        }

        [ComVisible(false)]
        private sealed class ReadOnlyAction : ScriptFunc<IScriptCompositeObject>
        {
            public const string Name = "readonly";
            private const string FirstParamName = "obj";

            public ReadOnlyAction()
                : base(FirstParamName, ScriptCompositeContract.Empty, ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx, IScriptCompositeObject obj)
            {
                return obj != null ? obj.AsReadOnly(ctx.RuntimeState) : null;
            }
        }

        [ComVisible(false)]
        private sealed class RegexAction : ScriptFunc
        {
            public const string Name = "regex";

            public RegexAction()
                : base(ScriptSuperContract.Instance)
            {
            }

            protected override IScriptObject Invoke(InvocationContext ctx)
            {
                return new Regex();
            }
        }

        /// <summary>
        /// Represents collection of the predefined slots.
        /// This class cannot be inherited.
        /// </summary>
        [ComVisible(false)]
        protected class ModuleSlots : ObjectSlotCollection
        {
            /// <summary>
            /// Initializes a new collection of the predefined slots.
            /// </summary>
            public ModuleSlots()
            {
                AddConstant<ThreadingLibrary>(ThreadingLibrary.Name);
                AddConstant<PutsAction>(PutsAction.Name);
                Add<DebugSlot>(DebugSlot.Name);
                Add<CompiledSlot>(CompiledSlot.Name);
                AddConstant<NewObjAction>(NewObjAction.Name);
                AddConstant<UseAction>(UseAction.Name);
                AddConstant<Math>(Math.Name);
                AddConstant<GetsAction>(GetsAction.Name);
                AddConstant<PrepareAction>(PrepareAction.Name);
                Add<ArgsSlot>(ArgsSlot.Name);
                AddConstant<CmdAction>(CmdAction.Name);
                AddConstant<GC>(GC.Name);
                AddConstant<RuntimeDiagnostics>(RuntimeDiagnostics.Name);
                Add<WorkingDirectorySlot>(WorkingDirectorySlot.Name);
                AddConstant("ver", new ScriptInteger(DynamicScriptInterpreter.Version.Major));
                AddConstant<ReadOnlyAction>(ReadOnlyAction.Name);
                AddConstant<EnumAction>(EnumAction.Name);
                AddConstant<SplitAction>(SplitAction.Name);
                AddConstant<EvalAction>(EvalAction.Name);
                AddConstant<SetDataAction>(SetDataAction.Name);
                AddConstant<GetDataAction>(GetDataAction.Name);
                AddConstant<RegexAction>(RegexAction.Name);
                AddConstant<ReflectAction>(ReflectAction.Name);
                AddConstant<AdjustAction>(AdjustAction.Name);
                AddConstant<WeakRefAction>(WeakRefAction.Name);
                AddConstant<ParseAction>(ParseAction.Name);
                AddConstant<RuntimeBehaviorSlots>(RuntimeBehaviorSlots.Name);
            }
        }
        #endregion

        /// <summary>
        /// Initializes a new module with the specified restrictions.
        /// </summary>
        /// <param name="slots">The collection of the modules slots.</param>
        protected ScriptModule(ModuleSlots slots)
            : base(slots ?? new ModuleSlots())
        {
        }

        /// <summary>
        /// Initializes a new module.
        /// </summary>
        public ScriptModule()
            : this(null)
        {
        }

        /// <summary>
        /// Constructs a new object dynamically.
        /// </summary>
        /// <param name="ctx">The invocation context.</param>
        /// <param name="name">The name of the slot.</param>
        /// <param name="contract">The contract binding for the slot. Cannot be <see cref="ScriptObject.Void"/>.</param>
        /// <returns>The created object.</returns>
        /// <exception cref="VoidException"><paramref name="contract"/> is <see cref="ScriptObject.Void"/>.</exception>
        [CLSCompliant(false)]
        public static IScriptObject NewObj(InvocationContext ctx, ScriptString name, IScriptObject contract)
        {
            if (ScriptObject.IsVoid(contract)) throw new VoidException(ctx.RuntimeState);
            return new ScriptCompositeObject(new[] { Variable(name, contract) });
        }

        /// <summary>
        /// Writes the specified object to the output stream.
        /// </summary>
        /// <param name="ctx">The invocation context.</param>
        /// <param name="obj">The object to be written to the output stream.</param>
        public static void Puts(InvocationContext ctx, IScriptObject obj)
        {
            QScriptIO.WriteLine(obj);
        }

        /// <summary>
        /// Reads object from the input stream.
        /// </summary>
        /// <param name="ctx">The invocation context.</param>
        /// <returns>The object restored from the input stream.</returns>
        public static IScriptObject Gets(InvocationContext ctx)
        {
            return QScriptIO.ReadLine();
        }

        private static IScriptObject Use(InvocationContext ctx, IEnumerable<Uri> scriptLocations)
        {
            IScriptObject module = Void;
            foreach (var location in scriptLocations)
                if (Use(ctx, location, out module)) break;
            return module;
        }

        private static bool Use(InvocationContext ctx, Uri scriptLocation, out IScriptObject module)
        {
            //if script file is not existed the return void.
            if (scriptLocation.IsFile && !File.Exists(scriptLocation.LocalPath))
            {
                module = Void;
                return false;
            }
            var compiledScript = ctx.RuntimeState.Cache.Cached(scriptLocation) ?
                ctx.RuntimeState.Cache.Lookup(scriptLocation, DynamicScriptInterpreter.OnFlyCompiler).CompiledScript :
                InterpreterState.Compile(scriptLocation, DynamicScriptInterpreter.OnFlyCompiler).CompiledScript;
            //trace module loading
            if (ScriptDebugger.CurrentDebugger != null) ScriptDebugger.CurrentDebugger.OnLoadModule(scriptLocation);
            module = compiledScript.Invoke(ctx.RuntimeState);
            return true;
        }

        private static IScriptObject Use(InvocationContext ctx, string scriptFile)
        {
            if (string.IsNullOrWhiteSpace(scriptFile)) return Void;
            return Use(ctx, RuntimeHelpers.PrepareScriptFilePath(scriptFile));
        }

        /// <summary>
        /// Loads script file or compiled script.
        /// </summary>
        /// <param name="ctx">The invocation context.</param>
        /// <param name="scriptFile">The path to the script file or URL address of script location.</param>
        /// <returns>Script invocation result.</returns>
        [CLSCompliant(false)]
        public static IScriptObject Use(InvocationContext ctx, ScriptString scriptFile)
        {
            return Use(ctx, (string)scriptFile);
        }

        private static bool Prepare(InvocationContext ctx, Uri scriptFile, bool cacheResult)
        {
            var cookie = ctx.RuntimeState.Cache.Lookup(scriptFile, DynamicScriptInterpreter.OnFlyCompiler);
            switch (cacheResult && cookie != null)
            {
                case true:
                    cookie.ScriptResult = cookie.CompiledScript.Invoke(ctx.RuntimeState);
                    return true;
                default: return false;
            }
        }

        private static bool Prepare(InvocationContext ctx, IEnumerable<Uri> scriptFiles, bool cacheResult)
        {
            foreach (var f in scriptFiles)
                if (Prepare(ctx, f, cacheResult)) return true;
            return false;
        }

        private static void Prepare(InvocationContext ctx, string scriptFile, bool cacheResult)
        {
            Prepare(ctx, RuntimeHelpers.PrepareScriptFilePath(scriptFile), cacheResult);
        }

        /// <summary>
        /// Loads script file and saves it to the cache.
        /// </summary>
        /// <param name="ctx">The action invocation context.</param>
        /// <param name="scriptFile">The path to the script file.</param>
        /// <param name="cacheResult">Specifies that the loaded script should be executed and its result saved into cache.</param>
        [CLSCompliant(false)]
        public static void Prepare(InvocationContext ctx, ScriptString scriptFile, ScriptBoolean cacheResult)
        {
            Prepare(ctx, (string)scriptFile, (bool)cacheResult);
        }

        internal static ConstructorInfo DefaultConstructor
        {
            get { return LinqHelpers.BodyOf<Func<ScriptModule>, NewExpression>(() => new ScriptModule()).Constructor; }
        }

        /// <summary>
        /// Executes external program.
        /// </summary>
        /// <param name="ctx">The invocation context.</param>
        /// <param name="fileName">The name of an applicatio file to run.</param>
        /// <param name="arguments">Command-line arguments to pass when starting process.</param>
        /// <param name="timeout">The amount of time, in milliseconds, to wait for the associated process to exit.</param>
        /// <returns>Process exit code.</returns>
        [CLSCompliant(false)]
        public static ScriptInteger Cmd(InvocationContext ctx, ScriptString fileName, ScriptString arguments, ScriptInteger timeout)
        {
            return Cmd(ctx, (string)fileName, (string)arguments, (long)timeout);
        }

        private static ScriptInteger Cmd(InvocationContext ctx, string command, string arguments, long timeout)
        {
            using (var p = Process.Start(command, arguments))
                return p.WaitForExit((int)timeout) ? new ScriptInteger(p.ExitCode) : ScriptInteger.MinValue;
        }

        /// <summary>
        /// Stores object into the internal data store.
        /// </summary>
        /// <param name="ctx">Internal interpreter state.</param>
        /// <param name="name">The name of data slot.</param>
        /// <param name="obj">An object to store.</param>
        public static void SetData(InvocationContext ctx, ScriptString name, IScriptObject obj)
        {
            if (name == null) throw new VoidException(ctx.RuntimeState);
            ctx.RuntimeState[name] = IsVoid(obj) ? null : obj;
        }

        /// <summary>
        /// Restores object from the internal data store.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        public static ScriptObject GetData(InvocationContext ctx, ScriptString name)
        {
            if (name == null) throw new VoidException(ctx.RuntimeState);
            return (ctx.RuntimeState[name] as ScriptObject) ?? Void;
        }

        /// <summary>
        /// Reflects the specified composite contract.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static ScriptCompositeObject Reflect(InvocationContext ctx, ScriptCompositeContract contract)
        {
            return contract != null ? new ScriptIterable(contract.Reflect()) : ScriptIterable.Empty();
        }

        /// <summary>
        /// Reflects the specified composite contract.
        /// </summary>
        /// <param name="ctx"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ScriptCompositeObject Reflect(InvocationContext ctx, ScriptCompositeObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            return new ScriptCompositeObjectMetadata(obj);
        }
    }
}
