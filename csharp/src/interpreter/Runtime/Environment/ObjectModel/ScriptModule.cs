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
    using DScriptIO = Hosting.DynamicScriptIO;
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
    using ThreadingLibrary = Threading.ThreadingLibrary;

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

            protected override IScriptObject Invoke(IScriptObject arg0, InterpreterState state)
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

            protected override IScriptObject Invoke(ScriptActionBase action, IScriptObject @this, InterpreterState state)
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

            protected override IScriptObject Invoke(ScriptString name, InterpreterState state)
            {
                return GetData(name, state);
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

            protected override void Invoke(ScriptString name, IScriptObject obj, InterpreterState state)
            {
                SetData(name, obj, state);
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

            protected override IScriptObject Invoke(IScriptObject obj, InterpreterState state)
            {
                if (obj is ScriptCompositeContract)
                    return Reflect((ScriptCompositeContract)obj);
                else if (obj is ScriptCompositeObject)
                    return ScriptModule.Reflect((ScriptCompositeObject)obj);
                else return ScriptCompositeContract.Empty.CreateCompositeObject(new IScriptObject[0], state);
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

            protected override IScriptObject Invoke(ScriptString scriptCode, IScriptObject global, InterpreterState state)
            {
                if (IsVoid(global)) global = state.Global;
                if (scriptCode == null) scriptCode = ScriptString.Empty;
                using (var enumerator = scriptCode.Value.GetEnumerator())
                {
                    var compiledScript = DynamicScriptInterpreter.OnFlyCompiler.Invoke(enumerator);
                    return compiledScript.Invoke(state);
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

            protected override IScriptObject Invoke(ScriptCompositeObject obj, InterpreterState state)
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

            public override IScriptObject Invoke(ScriptString value, IScriptContract type, ScriptString language, InterpreterState state)
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

            protected override IScriptObject Invoke(IScriptArray array, InterpreterState state)
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

            protected override void Invoke(IScriptObject obj, InterpreterState state)
            {
                Puts(obj);
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

            protected override IScriptObject Invoke(InterpreterState state)
            {
                return Gets();
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

            protected override IScriptObject Invoke(ScriptString name, IScriptContract contract, InterpreterState state)
            {
                return name == null || IsVoid(contract) ? Void : NewObj(name, contract, state);
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

            protected override IScriptObject Invoke(ScriptString scriptFile, InterpreterState state)
            {
                return Use(scriptFile, state);
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

            protected override void Invoke(ScriptString scriptFile, ScriptBoolean cacheResult, InterpreterState state)
            {
                Prepare(scriptFile, cacheResult, state);
            }
        }

        [ComVisible(false)]
        private sealed class QuitAction : ScriptAction<ScriptInteger>
        {
            public const string Name = "quit";
            private const string FirstParamName = "exitCode";

            public QuitAction()
                : base(FirstParamName, ScriptIntegerContract.Instance)
            {
            }

            protected override void Invoke(ScriptInteger exitCode, InterpreterState state)
            {
                SystemEnvironment.Exit(exitCode.IsInt32 ? (int)exitCode : 0);
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

            public override IScriptObject Invoke(ScriptString command, ScriptString arguments, ScriptInteger timeout, InterpreterState state)
            {
                return Cmd(command, arguments, timeout);
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

            protected override IScriptObject Invoke(IScriptCompositeObject obj, InterpreterState state)
            {
                return obj != null ? obj.AsReadOnly(state) : null;
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

            protected override IScriptObject Invoke(InterpreterState state)
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
                AddConstant<QuitAction>(QuitAction.Name);
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
        /// <param name="name">The name of the slot.</param>
        /// <param name="contract">The contract binding for the slot. Cannot be <see cref="ScriptObject.Void"/>.</param>
        /// <param name="state"></param>
        /// <returns>The created object.</returns>
        /// <exception cref="VoidException"><paramref name="contract"/> is <see cref="ScriptObject.Void"/>.</exception>
        [CLSCompliant(false)]
        public static IScriptObject NewObj(ScriptString name, IScriptContract contract, InterpreterState state)
        {
            if (ScriptObject.IsVoid(contract)) throw new VoidException(state);
            return new ScriptCompositeObject(new[] { Variable(name, contract) });
        }

        /// <summary>
        /// Writes the specified object to the output stream.
        /// </summary>
        /// <param name="obj">The object to be written to the output stream.</param>
        public static void Puts(IScriptObject obj)
        {
            DScriptIO.WriteLine(obj);
        }

        /// <summary>
        /// Reads object from the input stream.
        /// </summary>
        /// <returns>The object restored from the input stream.</returns>
        public static IScriptObject Gets()
        {
            return DScriptIO.ReadLine();
        }

        private static IScriptObject Use(IEnumerable<Uri> scriptLocations, InterpreterState state)
        {
            IScriptObject module = Void;
            foreach (var location in scriptLocations)
                if (Use(location, out module, state)) break;
            return module;
        }

        private static bool Use(Uri scriptLocation, out IScriptObject module, InterpreterState state)
        {
            //if script file is not existed the return void.
            if (scriptLocation.IsFile && !File.Exists(scriptLocation.LocalPath))
            {
                module = Void;
                return false;
            }
            var compiledScript = state.Cache.Cached(scriptLocation) ?
                state.Cache.Lookup(scriptLocation, DynamicScriptInterpreter.OnFlyCompiler).CompiledScript :
                InterpreterState.Compile(scriptLocation, DynamicScriptInterpreter.OnFlyCompiler).CompiledScript;
            //trace module loading
            if (ScriptDebugger.CurrentDebugger != null) ScriptDebugger.CurrentDebugger.OnLoadModule(scriptLocation);
            module = compiledScript.Invoke(state);
            return true;
        }

        /// <summary>
        /// Executes a script stored in the specified file.
        /// </summary>
        /// <param name="scriptFile"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static IScriptObject Use(string scriptFile, InterpreterState state)
        {
            if (string.IsNullOrWhiteSpace(scriptFile)) return Void;
            return Use(RuntimeHelpers.PrepareScriptFilePath(scriptFile), state);
        }

        private static bool Prepare(Uri scriptFile, bool cacheResult, InterpreterState state)
        {
            var cookie = state.Cache.Lookup(scriptFile, DynamicScriptInterpreter.OnFlyCompiler);
            switch (cacheResult && cookie != null)
            {
                case true:
                    cookie.ScriptResult = cookie.CompiledScript.Invoke(state);
                    return true;
                default: return false;
            }
        }

        private static bool Prepare(IEnumerable<Uri> scriptFiles, bool cacheResult, InterpreterState state)
        {
            foreach (var f in scriptFiles)
                if (Prepare(f, cacheResult, state)) return true;
            return false;
        }

        /// <summary>
        /// Compiles the specified script file.
        /// </summary>
        /// <param name="scriptFile"></param>
        /// <param name="cacheResult"></param>
        /// <param name="state"></param>
        public static void Prepare(string scriptFile, bool cacheResult, InterpreterState state)
        {
            Prepare(RuntimeHelpers.PrepareScriptFilePath(scriptFile), cacheResult, state);
        }

        internal static ConstructorInfo DefaultConstructor
        {
            get { return LinqHelpers.BodyOf<Func<ScriptModule>, NewExpression>(() => new ScriptModule()).Constructor; }
        }

        /// <summary>
        /// Executes external program.
        /// </summary>
        /// <param name="command"></param>
        /// <param name="arguments"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        public static ScriptInteger Cmd(string command, string arguments, long timeout)
        {
            using (var p = Process.Start(command, arguments))
                return p.WaitForExit((int)timeout) ? new ScriptInteger(p.ExitCode) : ScriptInteger.MinValue;
        }

        /// <summary>
        /// Stores object into the internal data store.
        /// </summary>
        /// <param name="name">The name of data slot.</param>
        /// <param name="obj">An object to store.</param>
        /// <param name="state">Internal interpreter state.</param>
        public static void SetData(ScriptString name, IScriptObject obj, InterpreterState state)
        {
            if (name == null) throw new VoidException(state);
            state[name] = IsVoid(obj) ? null : obj;
        }

        /// <summary>
        /// Restores object from the internal data store.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="state"></param>
        /// <returns></returns>
        public static ScriptObject GetData(ScriptString name, InterpreterState state)
        {
            if (name == null) throw new VoidException(state);
            return (state[name] as ScriptObject) ?? Void;
        }

        /// <summary>
        /// Reflects the specified composite contract.
        /// </summary>
        /// <param name="contract"></param>
        /// <returns></returns>
        public static ScriptCompositeObject Reflect(ScriptCompositeContract contract)
        {
            return contract != null ? new ScriptIterable(contract.Reflect()) : ScriptIterable.Empty();
        }

        /// <summary>
        /// Reflects the specified composite contract.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static ScriptCompositeObject Reflect(ScriptCompositeObject obj)
        {
            if (obj == null) throw new ArgumentNullException("obj");
            return new ScriptCompositeObjectMetadata(obj);
        }
    }
}
